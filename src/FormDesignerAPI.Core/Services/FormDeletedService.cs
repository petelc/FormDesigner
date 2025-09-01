using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.Core.FormAggregate.Events;
using FormDesignerAPI.Core.Interfaces;

namespace FormDesignerAPI.Core.Services;

/// <summary>
/// Service for handling form deletion.
/// </summary>
/// <param name="_formRepository"></param>
/// <param name="_mediator"></param>
/// <param name="logger"></param>
public class FormDeletedService(IRepository<Form> _formRepository, IMediator _mediator, ILogger<FormDeletedService> logger) : IDeleteFormService
{
    public async Task<Result> DeleteFormAsync(int formId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting Form {formId}", formId);
        Form? aggregateToDelete = await _formRepository.GetByIdAsync(formId);
        if (aggregateToDelete == null)
        {
            return Result.NotFound();
        }

        await _formRepository.DeleteAsync(aggregateToDelete);
        var domainEvent = new FormDeletedEvent(formId);
        await _mediator.Publish(domainEvent);
        await _formRepository.SaveChangesAsync();

        return Result.Success();
    }
}
