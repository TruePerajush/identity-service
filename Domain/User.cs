namespace IdentityService.Domain;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public bool IsLockedOut => LockedUntil is not null && DateTime.UtcNow < LockedUntil;
    private readonly List<RefreshToken> _refreshTokens = [];

    private User() { } // EF Core

    public static User Create(string email, string passwordHash, string name) => new()
    {
        Id = Guid.NewGuid(),
        Email = email.ToLowerInvariant(),
        PasswordHash = passwordHash,
        Name = name,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    public RefreshToken IssueRefreshToken(string tokenHash, DateTime expiresAt, string? ip)
    {
        var token = RefreshToken.Create(Id, tokenHash, expiresAt, ip);
        _refreshTokens.Add(token);
        return token;
    }

    public void RecordFailedLogin(int maxAttempts, TimeSpan lockoutDuration)
    {
        if (LockedUntil is not null && DateTime.UtcNow >= LockedUntil)
        {
            FailedLoginAttempts = 0;
            LockedUntil = null;
        }

        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            LockedUntil = DateTime.UtcNow.Add(lockoutDuration);
        }
    }

    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }
}