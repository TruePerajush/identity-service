namespace IdentityService.Infrastructure;

public sealed class LoginOptions
{
    public const string SectionName = "Login";
    public int MaxFailedAttempts { get; init; } = 5;
    public int LockoutMinutes { get; init; } = 15;
}