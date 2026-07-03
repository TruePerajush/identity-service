namespace IdentityService.Infrastructure.Jwt.Interfaces;

public interface IPasswordHasher
{
    public string Hash(string password);
    public bool Verify(string password, string hash);
}