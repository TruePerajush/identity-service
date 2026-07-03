using FluentResults;
using IdentityService.Common;
using IdentityService.Domain;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Jwt.Interfaces;
using IdentityService.Infrastructure.Kafka.Events;
using IdentityService.Infrastructure.Kafka.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Register;

public sealed class RegisterHandler(
    IdentityDbContext db,
    IPasswordHasher hasher,
    IKafkaProducer kafka) : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> Handle(RegisterCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(user => user.Email == email, ct))
            return Result.Fail(IdentityErrors.EmailAlreadyExists(email));

        var user = User.Create(email, hasher.Hash(cmd.Password), cmd.Name);
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        await kafka.PublishAsync("identity.user-registered",
            new UserRegisteredEvent(user.Id, user.Email, user.Name, user.CreatedAt), ct);

        return Result.Ok(new RegisterResponse(user.Id, user.Email));
    }
}