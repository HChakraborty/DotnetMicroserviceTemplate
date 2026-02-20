using SampleAuthService.Application.Events;
using Microsoft.Extensions.Hosting;
using SampleAuthService.Application.Interfaces;

namespace SampleAuthService.Infrastructure.BackgroundServices;

public class UserCreatedConsumer : BackgroundService { 
    private readonly IEventBus _eventBus; 
    
    public UserCreatedConsumer(IEventBus eventBus) { 
        _eventBus = eventBus; 
    } 
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) { 
        await _eventBus.Subscribe<UserCreatedEvent>(Handle); 
    } 
    
    private Task Handle(UserCreatedEvent evt) {
        Console.WriteLine($"User created event received: {evt.Email}"); 
        return Task.CompletedTask; 
    } 
}