using FormDesignerAPI.Core.ProjectContext.Aggregates;
using FormDesignerAPI.Core.ProjectContext.Interfaces;
using FormDesignerAPI.Core.ProjectContext.ValueObjects;
using Ardalis.GuardClauses;

namespace FormDesignerAPI.UseCases.ProjectContext.Create;

/// <summary>
/// Handler for creating a new project
/// </summary>
public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, Result<ProjectDTO>>
{
  private readonly IProjectRepository _repository;

  public CreateProjectHandler(IProjectRepository repository)
  {
    _repository = repository;
  }

  public async Task<Result<ProjectDTO>> Handle(
    CreateProjectCommand request,
    CancellationToken cancellationToken)
  {
    // Validate inputs
    Guard.Against.NullOrWhiteSpace(request.Name, nameof(request.Name));
    Guard.Against.Null(request.Filter, nameof(request.Filter));

    // Map DTO to value object with null coalescing for safety
    var filter = new ProjectFilter
    {
      FormType = request.Filter.FormType,
      Tags = request.Filter.Tags ?? new List<string>(),
      ActiveOnly = request.Filter.ActiveOnly,
      FromDate = request.Filter.FromDate,
      ToDate = request.Filter.ToDate,
      CustomFilters = request.Filter.CustomFilters ?? new Dictionary<string, object>()
    };

    // Create project using factory method
    var project = Project.Create(
      request.Name,
      request.Description,
      filter,
      request.CreatedBy
    );

    // Save to repository
    await _repository.AddAsync(project, cancellationToken);
    await _repository.SaveChangesAsync(cancellationToken);

    // Map to DTO
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
