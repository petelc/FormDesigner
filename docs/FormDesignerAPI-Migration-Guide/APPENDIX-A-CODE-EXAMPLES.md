# Appendix A: Complete Code Examples

## Using Traxs.SharedKernel

### Entity with Guid ID
```csharp
using Traxs.SharedKernel;

public class MyEntity : EntityBase<Guid>, IAggregateRoot
{
    public string Name { get; private set; }
    
    private MyEntity() { }
    
    public static MyEntity Create(string name)
    {
        var entity = new MyEntity 
        { 
            Id = Guid.NewGuid(),
            Name = name 
        };
        
        entity.RegisterDomainEvent(new MyEntityCreatedEvent(entity.Id));
        return entity;
    }
}
```

### Domain Event
```csharp
using Traxs.SharedKernel;

public record MyEntityCreatedEvent(Guid EntityId) : DomainEventBase;
```

### Repository Interface
```csharp
using Traxs.SharedKernel;

public interface IMyRepository : IRepository<MyEntity>
{
    Task<MyEntity?> GetByNameAsync(string name);
}
```

### Specification
```csharp
using Ardalis.Specification;

public class GetActiveEntitiesSpec : Specification<MyEntity>
{
    public GetActiveEntitiesSpec()
    {
        Query.Where(e => e.IsActive)
             .OrderBy(e => e.Name);
    }
}
```

[Additional examples...]
