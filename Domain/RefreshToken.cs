namespace IdentityService.Domain;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public string TokenHash { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAt, string? ip) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        TokenHash = tokenHash,
        ExpiresAt = expiresAt,
        CreatedAt = DateTime.UtcNow,
        CreatedByIp = ip
    };

    public void Revoke(Guid? replacedByTokenId = null)
    {
        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}