namespace FormDesignerAPI.UseCases.Forms.List;

/// <summary>
/// Query to list all forms
/// </summary>
public record ListFormsQuery(string? Division, string? SearchTerm) : IRequest<Result<IEnumerable<FormDTO>>>;
