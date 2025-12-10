using Ardalis.Specification;
using FormDesignerAPI.Core.FormContext.Aggregates;

namespace FormDesignerAPI.Core.FormContext.Specifications;

/// <summary>
/// Specification to get all active forms
/// </summary>
public class GetActiveFormsSpec : Specification<Form>
{
    public GetActiveFormsSpec()
    {
        Query
          .Where(f => f.IsActive)
          .OrderByDescending(f => f.Origin.CreatedAt);
    }
}