namespace FormDesignerAPI.UseCases.FormContext.List;

/// <summary>
/// Query to list all forms with optional filters
/// </summary>
public record ListFormsQuery(
    bool? ActiveOnly = null,
    string? SearchTerm = null
) : IRequest<Result<IEnumerable<FormContextDTO>>>;
