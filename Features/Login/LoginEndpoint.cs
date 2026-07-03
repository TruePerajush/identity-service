using IdentityService.Common;
using MediatR;

namespace IdentityService.Features.Login;

public static class LoginEndpoint
{
    public static RouteHandlerBuilder MapLogin(this IEndpointRouteBuilder app) =>
        app.MapPost("/api/identity/login", async (LoginRequestDto dto, HttpContext http, ISender sender, CancellationToken ct) =>
        {
            var ip = http.Connection.RemoteIpAddress?.ToString();
            var result = await sender.Send(new LoginCommand(dto.Email, dto.Password, ip), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        })
        .WithTags("Identity");
}