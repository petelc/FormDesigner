using FormDesignerAPI.UseCases.ProjectContext;
using FormDesignerAPI.UseCases.ProjectContext.Create;
using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.Web.ProjectContext;

/// <summary>
/// Request to create a new project
/// </summary>
public record CreateProjectRequest
{
  public const string Route = "/api/projects";

  public string Name { get; init; } = string.Empty;
  public string Description { get; init; } = string.Empty;
  
  /// <summary>
  /// Optional filter configuration. If not provided, a default filter will be used.
  /// </summary>
  public ProjectFilterDTO? Filter { get; init; }
}

/// <summary>
/// Create a new project
/// </summary>
public class CreateProject(IMediator _mediator, IUser _currentUser)
  : Endpoint<CreateProjectRequest, ProjectDTO>
{
  public override void Configure()
  {
    Post(CreateProjectRequest.Route);
    
    // Just require authentication, no specific roles
    
    Summary(s =>
    {
      s.Summary = "Create a new project";
      s.Description = "Creates a new project with the specified configuration (requires authentication)";
      s.Responses[200] = "Project created successfully";
      s.Responses[400] = "Invalid request";
      s.Responses[401] = "Unauthorized - authentication required";
    });
  }

  public override async Task HandleAsync(
    CreateProjectRequest request,
    CancellationToken cancellationToken)
  {
    // Check if user is authenticated
    if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.Id))
    {
      await SendUnauthorizedAsync(cancellationToken);
      return;
    }

    // Use provided filter or create a default one
    var filter = request.Filter ?? new ProjectFilterDTO
    {
      ActiveOnly = true,
      Tags = new List<string>(),
      CustomFilters = new Dictionary<string, object>()
    };

    // Use the authenticated user's information
    var createdBy = _currentUser.UserName ?? _currentUser.Email ?? _currentUser.Id;

    var command = new CreateProjectCommand(
      request.Name,
      request.Description,
      filter,
      createdBy
    );

    var result = await _mediator.Send(command, cancellationToken);

    if (result.IsSuccess)
    {
      Response = result.Value!;
      await SendOkAsync(Response, cancellationToken);
    }
    else
    {
      await SendResultAsync(Results.BadRequest(result.Errors));
    }
  }
}
