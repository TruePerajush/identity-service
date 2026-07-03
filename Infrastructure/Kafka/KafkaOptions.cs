namespace IdentityService.Infrastructure.Kafka;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; init; } = default!;
}