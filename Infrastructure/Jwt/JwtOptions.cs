namespace IdentityService.Infrastructure.Jwt;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int AccessTokenMinutes { get; init; } = 15;
}