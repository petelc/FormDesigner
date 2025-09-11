using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UseCases.Forms.Update;

public record UpdateFormCommand(int FormId, string newFormNumber, string newFormTitle, string newDivision, string newOwnerName, string newOwnerEmail, string newVersion, DateTime RevisedDate, string newConfigurationPath) : ICommand<Result<FormDTO>>;
