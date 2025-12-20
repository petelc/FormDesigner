namespace FormDesignerAPI.UseCases.ProjectContext.AddPdf;

public record AddFormToProjectCommand(
    Guid ProjectId,
    Guid FormId,
    string UpdatedBy
) : IRequest<Result<bool>>;

