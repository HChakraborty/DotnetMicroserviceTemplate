using Microsoft.Extensions.Hosting;
using ServiceName.Application.Events;
using ServiceName.Application.Interfaces;

namespace ServiceName.Infrastructure.BackgroundServices;

public class SampleUpdatedConsumer : BackgroundService { 
    private readonly IEventBus _eventBus; 
    
    public SampleUpdatedConsumer(IEventBus eventBus) { 
        _eventBus = eventBus; 
    } 
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) { 
        await _eventBus.Subscribe<SampleUpdatedEvent>(Handle); 
    } 
    
    private Task Handle(SampleUpdatedEvent evt) {
        Console.WriteLine($"Sample update event received: {evt.Id}"); 
        return Task.CompletedTask; 
    } 
}