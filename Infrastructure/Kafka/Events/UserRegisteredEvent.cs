namespace IdentityService.Infrastructure.Kafka.Events;

public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string Name,
    DateTime RegisteredAt);