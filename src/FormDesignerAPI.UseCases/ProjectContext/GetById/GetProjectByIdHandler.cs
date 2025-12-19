using FormDesignerAPI.Core.ProjectContext.Aggregates;
using FormDesignerAPI.Core.ProjectContext.Interfaces;

namespace FormDesignerAPI.UseCases.ProjectContext.GetById;

/// <summary>
/// Query to get a project by ID
/// </summary>
public record GetProjectByIdQuery(Guid ProjectId) : IRequest<Result<ProjectDTO>>;

/// <summary>
/// Handler for getting a project by ID
/// </summary>
public class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDTO>>
{
    private readonly IProjectRepository _repository;

    public GetProjectByIdHandler(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProjectDTO>> Handle(
      GetProjectByIdQuery request,
      CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
        {
            return Result.NotFound("Project not found");
        }

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
