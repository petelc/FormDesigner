using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.Core.FormAggregate.Events;
using FormDesignerAPI.Core.Interfaces;

namespace FormDesignerAPI.Core.Services;

public class FormUpdateService(IRepository<Form> _formRepository, IMediator _mediator, ILogger<FormUpdateService> logger) : IFormUpdateService
{
    public async Task<Result> UpdateFormAsync(int formId, FormUpdateDto formUpdateDto, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating Form {formId}", formId);
        var form = await _formRepository.GetByIdAsync(formId, cancellationToken);
        if (form == null)
        {
            return Result.NotFound();
        }

        form.UpdateDetails(
            formUpdateDto.FormNumber,
            formUpdateDto.FormTitle,
            formUpdateDto.Division ?? string.Empty,
            formUpdateDto.Owner ?? string.Empty,
            formUpdateDto.Version ?? string.Empty,
            //formUpdateDto.CreatedDate,
            formUpdateDto.RevisedDate ?? DateTime.UtcNow,
            formUpdateDto.ConfigurationPath ?? string.Empty
        );

        await _formRepository.UpdateAsync(form, cancellationToken);
        var domainEvent = new FormUpdatedEvent(form);
        await _mediator.Publish(domainEvent, cancellationToken);

        return Result.Success();
    }
}
