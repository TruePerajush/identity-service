using FluentResults;
using IdentityService.Common;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Jwt.Interfaces;
using IdentityService.Infrastructure.Kafka.Events;
using IdentityService.Infrastructure.Kafka.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IdentityService.Features.Login;

public sealed class LoginHandler(
    IdentityDbContext db,
    IPasswordHasher hasher,
    IJwtTokenGenerator jwt,
    IKafkaProducer kafka,
    IOptions<LoginOptions> loginOptions) : IRequestHandler<LoginCommand, Result<TokenResponse>>
{
    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), workFactor: 12);

    public async Task<Result<TokenResponse>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email, ct);

        if (user is null)
        {
            hasher.Verify(cmd.Password, DummyHash);
            return Result.Fail(IdentityErrors.InvalidCredentials());
        }

        if (user.IsLockedOut)
            return Result.Fail(IdentityErrors.AccountLocked(user.LockedUntil!.Value));

        if (!hasher.Verify(cmd.Password, user.PasswordHash))
        {
            user.RecordFailedLogin(loginOptions.Value.MaxFailedAttempts, TimeSpan.FromMinutes(loginOptions.Value.LockoutMinutes));
            await db.SaveChangesAsync(ct);
            return Result.Fail(IdentityErrors.InvalidCredentials());
        }

        if (!user.IsActive)
            return Result.Fail(IdentityErrors.UserInactive());

        user.RecordSuccessfulLogin();

        var (accessToken, expiresAt) = jwt.GenerateAccessToken(user);
        var rawRefresh = jwt.GenerateRefreshTokenRaw();
        user.IssueRefreshToken(jwt.Hash(rawRefresh), DateTime.UtcNow.AddDays(30), cmd.Ip);
        await db.SaveChangesAsync(ct);

        await kafka.PublishAsync("identity.user-logged-in",
            new UserLoggedInEvent(user.Id, DateTime.UtcNow, cmd.Ip), ct);

        return Result.Ok(new TokenResponse(accessToken, rawRefresh, expiresAt));
    }
}
