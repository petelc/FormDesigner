using System;
using FormDesignerAPI.Core.ProjectContext.Interfaces;

namespace FormDesignerAPI.UseCases.ProjectContext.AddPdf;

public class AddFormToProjectHandler : IRequestHandler<AddFormToProjectCommand, Result<bool>>
{
    private readonly IProjectRepository _repository;

    public AddFormToProjectHandler(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(AddFormToProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
        {
            return Result.NotFound("Project not found");
        }

        project.AddForm(request.FormId);
        project.MarkPdfUploaded(request.UpdatedBy);

        await _repository.UpdateAsync(project, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
