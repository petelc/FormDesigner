using Ardalis.Specification;
using FormDesignerAPI.Core.FormContext.Aggregates;

namespace FormDesignerAPI.Core.FormContext.Specifications;

/// <summary>
/// Specification to search forms by name (case-insensitive)
/// </summary>
public class SearchFormsByNameSpec : Specification<Form>
{
    public SearchFormsByNameSpec(string searchTerm)
    {
        Query
          .Where(f => f.Name.ToLower().Contains(searchTerm.ToLower()))
          .OrderBy(f => f.Name);
    }
}