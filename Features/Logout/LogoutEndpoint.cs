using IdentityService.Common;
using MediatR;

namespace IdentityService.Features.Logout;

public static class LogoutEndpoint
{
    public static void MapLogout(this IEndpointRouteBuilder app) =>
        app.MapPost("/api/identity/logout", async (LogoutRequestDto dto, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new LogoutCommand(dto.RefreshToken), ct);
            return result.IsSuccess ? Results.NoContent() : result.ToProblem();
        })
        .WithTags("Identity");
}