using FormDesignerAPI.Core.ProjectContext.Aggregates;
using FormDesignerAPI.Core.ProjectContext.Interfaces;
using FormDesignerAPI.Core.ProjectContext.ValueObjects;

namespace FormDesignerAPI.UseCases.ProjectContext.List;

/// <summary>
/// Query to list projects with optional filtering
/// </summary>
public record ListProjectsQuery(
  int PageNumber = 1,
  int PageSize = 20,
  string? SearchTerm = null,
  string? Status = null
) : IRequest<Result<PagedProjectsResult>>;

/// <summary>
/// Paged result for projects
/// </summary>
public record PagedProjectsResult
{
  public List<ProjectDTO> Projects { get; init; } = new();
  public int TotalCount { get; init; }
  public int PageNumber { get; init; }
  public int PageSize { get; init; }
  public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
  public bool HasPrevious => PageNumber > 1;
  public bool HasNext => PageNumber < TotalPages;
}

/// <summary>
/// Handler for listing projects
/// </summary>
public class ListProjectsHandler : IRequestHandler<ListProjectsQuery, Result<PagedProjectsResult>>
{
  private readonly IProjectRepository _repository;

  public ListProjectsHandler(IProjectRepository repository)
  {
    _repository = repository;
  }

  public async Task<Result<PagedProjectsResult>> Handle(
    ListProjectsQuery request,
    CancellationToken cancellationToken)
  {
    // Parse status if provided
    ProjectStatus? status = null;
    if (!string.IsNullOrWhiteSpace(request.Status) &&
        Enum.TryParse<ProjectStatus>(request.Status, ignoreCase: true, out var parsedStatus))
    {
      status = parsedStatus;
    }

    // Get paged projects
    var (projects, totalCount) = await _repository.GetPagedAsync(
      request.PageNumber,
      request.PageSize,
      request.SearchTerm,
      status,
      cancellationToken
    );

    // Map to DTOs
    var dtos = projects.Select(MapToDTO).ToList();

    var result = new PagedProjectsResult
    {
      Projects = dtos,
      TotalCount = totalCount,
      PageNumber = request.PageNumber,
      PageSize = request.PageSize
    };

    return Result.Success(result);
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
