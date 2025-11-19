namespace FormDesignerAPI.UseCases.Forms.Delete;

public record DeleteFormCommand(Guid FormId) : ICommand<Result>;
