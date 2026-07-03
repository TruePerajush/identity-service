using System.Text.Json;
using Confluent.Kafka;
using IdentityService.Infrastructure.Kafka.Interfaces;
using Microsoft.Extensions.Options;

namespace IdentityService.Infrastructure.Kafka;

public sealed class KafkaProducer : IKafkaProducer, IAsyncDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(IOptions<KafkaOptions> options)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, T message, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = json
        }, ct);
    }

    public ValueTask DisposeAsync()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
        return ValueTask.CompletedTask;
    }
}