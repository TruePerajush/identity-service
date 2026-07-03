using FluentResults;

namespace IdentityService.Common;

public static class IdentityErrors
{
    public static Error EmailAlreadyExists(string email) =>
        new Error($"User with email '{email}' already exists")
            .WithMetadata("Code", "Identity.EmailAlreadyExists");

    public static Error InvalidCredentials() =>
        new Error("Invalid email or password")
            .WithMetadata("Code", "Identity.InvalidCredentials");

    public static Error RefreshTokenInvalid() =>
        new Error("Refresh token is invalid or expired")
            .WithMetadata("Code", "Identity.RefreshTokenInvalid");

    public static Error UserInactive() =>
        new Error("User account is inactive")
            .WithMetadata("Code", "Identity.UserInactive");

    public static Error AccountLocked(DateTime until) =>
        new Error("Account is temporarily locked due to too many failed login attempts")
            .WithMetadata("Code", "Identity.AccountLocked")
            .WithMetadata("LockedUntil", until);
}