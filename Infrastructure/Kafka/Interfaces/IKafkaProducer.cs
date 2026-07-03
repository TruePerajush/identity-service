namespace IdentityService.Infrastructure.Kafka.Interfaces;

public interface IKafkaProducer
{
    public Task PublishAsync<T>(string topic, T message, CancellationToken ct = default);

}