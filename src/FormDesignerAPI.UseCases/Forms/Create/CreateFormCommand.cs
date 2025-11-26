using Ardalis.Result;
using FastEndpoints;
using FormDesignerAPI.Core.FormAggregate;
using MediatR;

namespace FormDesignerAPI.UseCases.Forms.Create;

/// <summary>
/// Command to create a new form.
/// </summary>
/// <param name="FormNumber">Required: The unique form number</param>
/// <param name="FormTitle">Optional: The title of the form</param>
/// <param name="Division">Optional: The division this form belongs to</param>
/// <param name="Owner">Optional: The owner of the form</param>
/// <param name="CreatedDate">Optional: The creation date (defaults to UtcNow)</param>
/// <param name="RevisedDate">Optional: The last revision date</param>
public record CreateFormCommand(
    string FormNumber,
    string? FormTitle = null,
    string? Division = null,
    Owner? Owner = null,
    Core.FormAggregate.Revision? Revision = null,
    DateTime? CreatedDate = null,
    DateTime? RevisedDate = null
) : FastEndpoints.ICommand<Result<Guid>>, IRequest<Result<Guid>>;
