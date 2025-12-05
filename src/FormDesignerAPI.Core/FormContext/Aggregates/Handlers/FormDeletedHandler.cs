using FormDesignerAPI.Core.FormAggregate.Events;
using FormDesignerAPI.Core.Interfaces;

namespace FormDesignerAPI.Core.FormAggregate.Handlers;

internal class FormDeletedHandler(ILogger<FormDeletedHandler> logger, IEmailSender emailSender) : INotificationHandler<FormDeletedEvent>
{
    public async Task Handle(FormDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Form Deleted event for {FormId}.", domainEvent.FormId);

        await emailSender.SendEmailAsync("admin@example.com", "app@example.com", "Form Deleted", $"Form with ID {domainEvent.FormId} has been deleted.");
    }
}


