using FluentResults;
using IdentityService.Common;
using IdentityService.Features.Login;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Jwt.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.RefreshToken;

public sealed class RefreshTokenHandler(
    IdentityDbContext db,
    IJwtTokenGenerator jwt) : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        var hash = jwt.Hash(cmd.RefreshToken);
        var token = await db.RefreshTokens.Include(t => t.User)
            .SingleOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is null || !token.IsActive)
            return Result.Fail(IdentityErrors.RefreshTokenInvalid());

        var (accessToken, expiresAt) = jwt.GenerateAccessToken(token.User);
        var newRaw = jwt.GenerateRefreshTokenRaw();
        var newToken = token.User.IssueRefreshToken(jwt.Hash(newRaw), DateTime.UtcNow.AddDays(30), cmd.Ip);
        token.Revoke(newToken.Id);

        await db.SaveChangesAsync(ct);
        return Result.Ok(new TokenResponse(accessToken, newRaw, expiresAt));
    }
}