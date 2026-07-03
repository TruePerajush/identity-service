using FluentResults;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Jwt.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Logout;

public sealed class LogoutHandler(
    IdentityDbContext db,
    IJwtTokenGenerator jwt) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand cmd, CancellationToken ct)
    {
        var hash = jwt.Hash(cmd.RefreshToken);
        var token = await db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is null || !token.IsActive)
            return Result.Ok();

        token.Revoke();
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}