namespace SampleAuthService.Application.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T message);
    Task Subscribe<T>(Func<T, Task> handler);
}
