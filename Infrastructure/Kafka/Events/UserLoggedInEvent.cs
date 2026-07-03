namespace IdentityService.Infrastructure.Kafka.Events;

public sealed record UserLoggedInEvent(
    Guid UserId,
    DateTime LoggedInAt,
    string? Ip);