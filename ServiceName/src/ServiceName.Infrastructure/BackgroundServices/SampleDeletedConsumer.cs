using Microsoft.Extensions.Hosting;
using ServiceName.Application.Events;
using ServiceName.Application.Interfaces;

namespace ServiceName.Infrastructure.BackgroundServices;

public class SampleDeletedConsumer : BackgroundService { 
    private readonly IEventBus _eventBus; 
    
    public SampleDeletedConsumer(IEventBus eventBus) { 
        _eventBus = eventBus; 
    } 
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) { 
        await _eventBus.Subscribe<SampleDeletedEvent>(Handle); 
    } 
    
    private Task Handle(SampleDeletedEvent evt) {
        Console.WriteLine($"Sample deleted event received: {evt.Id}"); 
        return Task.CompletedTask; 
    } 
}