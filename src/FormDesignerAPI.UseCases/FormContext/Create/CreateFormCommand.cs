using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.UseCases.FormContext.Create;

/// <summary>
/// Command to create a new form with a FormDefinition
/// </summary>
public record CreateFormCommand(
    string Name,
    FormDefinition Definition,
    string CreatedBy
) : IRequest<Result<Guid>>;
