using FormDesignerAPI.Core.ProjectContext.Aggregates;
using FormDesignerAPI.Core.ProjectContext.Interfaces;

namespace FormDesignerAPI.UseCases.ProjectContext.Update;

/// <summary>
/// Command to update a project
/// </summary>
public record UpdateProjectCommand(
  Guid ProjectId,
  string? Name,
  string? Description,
  string UpdatedBy
) : IRequest<Result<ProjectDTO>>;

/// <summary>
/// Handler for updating a project
/// </summary>
public class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, Result<ProjectDTO>>
{
    private readonly IProjectRepository _repository;

    public UpdateProjectHandler(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProjectDTO>> Handle(
      UpdateProjectCommand request,
      CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
        {
            return Result.NotFound("Project not found");
        }

        // Use provided values or keep existing ones
        var name = !string.IsNullOrWhiteSpace(request.Name) ? request.Name : project.Name;
        var description = request.Description ?? project.Description;

        // Update the project
        project.Update(name, description, project.Filter, request.UpdatedBy);

        // Save changes
        await _repository.UpdateAsync(project, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = MapToDTO(project);
        return Result.Success(dto);
    }

    private static ProjectDTO MapToDTO(Project project)
    {
        return new ProjectDTO
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status.ToString(),
            Filter = new ProjectFilterDTO
            {
                FormType = project.Filter.FormType,
                Tags = project.Filter.Tags,
                ActiveOnly = project.Filter.ActiveOnly,
                FromDate = project.Filter.FromDate,
                ToDate = project.Filter.ToDate,
                CustomFilters = project.Filter.CustomFilters
            },
            FormIds = project.FormIds.ToList(),
            CodeGenerationJobIds = project.CodeGenerationJobIds.ToList(),
            CreatedBy = project.CreatedBy,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            UpdatedBy = project.UpdatedBy
        };
    }
}
