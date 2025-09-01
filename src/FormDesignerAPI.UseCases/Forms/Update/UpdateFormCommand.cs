namespace FormDesignerAPI.UseCases.Forms.Update;

public record UpdateFormCommand(int FormId, string newFormNumber, string newFormTitle, string newDivision, string newOwner, string newVersion, string newConfigurationPath) : ICommand<Result<FormDTO>>;
