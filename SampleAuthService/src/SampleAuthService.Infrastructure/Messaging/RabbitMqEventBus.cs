using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SampleAuthService.Application.Interfaces;
using SampleAuthService.Infrastructure.Configuration;
using System.Text;
using System.Text.Json;

public class RabbitMqEventBus : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private RabbitMqEventBus(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public static async Task<RabbitMqEventBus> CreateAsync(RabbitMqSettings settings)
    {
        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        return new RabbitMqEventBus(connection, channel);
    }

    public async Task PublishAsync<T>(T message)
    {
        var queueName = typeof(T).Name;

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message));

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            body: body);
    }

    public async Task Subscribe<T>(Func<T, Task> handler)
    {
        var queueName = typeof(T).Name;

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());
            var message = JsonSerializer.Deserialize<T>(json);

            if (message != null)
                await handler(message);
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true,
            consumer: consumer);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
