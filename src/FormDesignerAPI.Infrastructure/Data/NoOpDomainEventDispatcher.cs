using Traxs.SharedKernel;

namespace FormDesignerAPI.Infrastructure.Data;

/// <summary>
/// No-op implementation of IDomainEventDispatcher
/// Domain events are not currently being used in this application
/// </summary>
public class NoOpDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAndClearEvents(IEnumerable<IHasDomainEvents> entitiesWithEvents)
    {
        // No-op: domain events not implemented yet
        return Task.CompletedTask;
    }
}
