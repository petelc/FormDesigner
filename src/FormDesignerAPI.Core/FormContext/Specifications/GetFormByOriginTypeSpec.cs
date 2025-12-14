using Ardalis.Specification;
using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.Core.FormContext.Specifications;

/// <summary>
/// Specification to get forms by origin type
/// </summary>
public class GetFormsByOriginTypeSpec : Specification<Form>
{
    public GetFormsByOriginTypeSpec(OriginType originType)
    {
        Query
          .Where(f => f.Origin.Type == originType)
          .OrderByDescending(f => f.Origin.CreatedAt);
    }
}