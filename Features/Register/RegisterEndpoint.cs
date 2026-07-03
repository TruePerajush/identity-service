using IdentityService.Common;
using MediatR;

namespace IdentityService.Features.Register;

public static class RegisterEndpoint
{
    public static void MapRegister(this IEndpointRouteBuilder app) =>
        app.MapPost("/api/identity/register", async (RegisterRequestDto dto, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new RegisterCommand(dto.Email, dto.Password, dto.Name), ct);
            return result.IsSuccess
                ? Results.Created($"/api/identity/users/{result.Value.UserId}", result.Value)
                : result.ToProblem();
        })
        .WithName("Register")
        .WithTags("Identity");
}