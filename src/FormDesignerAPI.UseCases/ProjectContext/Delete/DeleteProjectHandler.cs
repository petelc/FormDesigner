using FormDesignerAPI.Core.ProjectContext.Interfaces;

namespace FormDesignerAPI.UseCases.ProjectContext.Delete;

/// <summary>
/// Command to delete a project
/// </summary>
public record DeleteProjectCommand(Guid ProjectId) : IRequest<Result>;

/// <summary>
/// Handler for deleting a project
/// </summary>
public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, Result>
{
    private readonly IProjectRepository _repository;

    public DeleteProjectHandler(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
      DeleteProjectCommand request,
      CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
        {
            return Result.NotFound("Project not found");
        }

        await _repository.DeleteAsync(project, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
