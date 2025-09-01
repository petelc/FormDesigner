namespace FormDesignerAPI.UseCases.Forms.Delete;

public record DeleteFormCommand(int FormId) : ICommand<Result>;
