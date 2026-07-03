using IdentityService.Domain;

namespace IdentityService.Infrastructure.Jwt.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);
    string GenerateRefreshTokenRaw();
    string Hash(string value);
}