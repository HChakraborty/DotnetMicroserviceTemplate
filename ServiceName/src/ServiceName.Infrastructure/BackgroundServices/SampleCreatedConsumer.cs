using Microsoft.Extensions.Hosting;
using ServiceName.Application.Events;
using ServiceName.Application.Interfaces;

namespace ServiceName.Infrastructure.BackgroundServices;

public class SampleCreatedConsumer : BackgroundService { 
    private readonly IEventBus _eventBus; 
    
    public SampleCreatedConsumer(IEventBus eventBus) { 
        _eventBus = eventBus; 
    } 
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) { 
        await _eventBus.Subscribe<SampleCreatedEvent>(Handle); 
    } 
    
    private Task Handle(SampleCreatedEvent evt) {
        Console.WriteLine($"Sample created event received: {evt.Id}"); 
        return Task.CompletedTask; 
    } 
}