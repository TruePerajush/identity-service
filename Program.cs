using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Jwt;
using IdentityService.Infrastructure.Jwt.Interfaces;
using IdentityService.Infrastructure.Kafka;
using IdentityService.Infrastructure.Kafka.Interfaces;
using FluentValidation;
using IdentityService.Features.Register;
using IdentityService.Features.Login;
using IdentityService.Features.RefreshToken;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Ardalis.ListStartupServices;
using MediatR;
using IdentityService.Common;
using IdentityService.Features.Logout;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection(KafkaOptions.SectionName));

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("IdentityDb")));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<ServiceConfig>(config =>
{
    config.Services = [.. builder.Services];
    config.Path = "/api/identity/listallservices";
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.Configure<LoginOptions>(builder.Configuration.GetSection(LoginOptions.SectionName));

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 10,
                QueueLimit = 0
            }));

    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            code = "Identity.TooManyRequests",
            detail = "Too many login attempts from this IP. Try again later."
        }, ct);
    };
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseShowAllServicesMiddleware();
    app.UseDeveloperExceptionPage();
}

app.MapRegister();
app.MapLogin().RequireRateLimiting("login");
app.MapRefreshToken();
app.MapLogout();

app.Run();