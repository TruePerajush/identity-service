
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IdentityService.Domain;
using IdentityService.Infrastructure.Jwt.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Infrastructure.Jwt;

public sealed class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    private readonly JwtOptions _o = options.Value;

    public (string, DateTime) GenerateAccessToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_o.AccessTokenMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_o.SigningKey));
        var token = new JwtSecurityToken(_o.Issuer, _o.Audience, claims,
            expires: expiresAt, signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string GenerateRefreshTokenRaw() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    public string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}