using IdentityService.Common;
using MediatR;

namespace IdentityService.Features.RefreshToken;

public static class RefreshTokenEndpoint
{
    public static void MapRefreshToken(this IEndpointRouteBuilder app) =>
        app.MapPost("/api/identity/refresh", async (
            RefreshTokenRequestDto dto, HttpContext http, ISender sender, CancellationToken ct) =>
        {
            var ip = http.Connection.RemoteIpAddress?.ToString();
            var result = await sender.Send(new RefreshTokenCommand(dto.RefreshToken, ip), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        })
        .WithTags("Identity");
}