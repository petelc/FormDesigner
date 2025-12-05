using FormDesignerAPI.Core.FormAggregate.Events;
using FormDesignerAPI.Core.Interfaces;

namespace FormDesignerAPI.Core.FormAggregate.Handlers;

public class FormUpdatedHandler(ILogger<FormUpdatedHandler> logger, IEmailSender emailSender) : INotificationHandler<FormUpdatedEvent>
{
    public async Task Handle(FormUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Form Updated event for {FormId}.", domainEvent.FormId);

        await emailSender.SendEmailAsync("admin@example.com", "app@example.com",
        "Form Updated", $" Form with ID - {domainEvent.FormId} and Title - {domainEvent.FormTitle} has been updated.");

    }
}
