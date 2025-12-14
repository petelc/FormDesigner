using Ardalis.Result;
using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UseCases.Forms.Create;

/// <summary>
/// Command to create a new form
/// </summary>
public record CreateFormCommand(
    string FormNumber,
    string FormTitle,
    string? Division,
    Owner? Owner,
    string? Version,
    DateTime? CreatedDate,
    DateTime? RevisedDate,
    string? ConfigurationPath
) : IRequest<Result<int>>;
