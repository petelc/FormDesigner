using FormDesignerAPI.UseCases.ProjectContext;

namespace FormDesignerAPI.UseCases.ProjectContext.Create;

/// <summary>
/// Command to create a new project
/// </summary>
public record CreateProjectCommand(
  string Name,
  string Description,
  ProjectFilterDTO Filter,
  string CreatedBy
) : IRequest<Result<ProjectDTO>>;
