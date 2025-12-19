using FormDesignerAPI.UseCases.ProjectContext;
using FormDesignerAPI.UseCases.ProjectContext.GetById;
using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.Web.ProjectContext;

/// <summary>
/// Request to get a project by ID
/// </summary>
public record GetProjectRequest
{
    public const string Route = "/api/projects/{id}";

    [BindFrom("id")]
    public Guid Id { get; init; }
}

/// <summary>
/// Get a single project by ID
/// </summary>
public class GetProject(IMediator _mediator, IUser _currentUser)
  : Endpoint<GetProjectRequest, ProjectDTO>
{
    public override void Configure()
    {
        Get(GetProjectRequest.Route);

        // Just require authentication, no specific roles

        Summary(s =>
        {
            s.Summary = "Get a project by ID";
            s.Description = "Retrieves a single project by its unique identifier (requires authentication)";
            s.Responses[200] = "Project found";
            s.Responses[401] = "Unauthorized - authentication required";
            s.Responses[404] = "Project not found";
        });
    }

    public override async Task HandleAsync(
      GetProjectRequest request,
      CancellationToken cancellationToken)
    {
        // Check if user is authenticated
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.Id))
        {
            await SendUnauthorizedAsync(cancellationToken);
            return;
        }

        var query = new GetProjectByIdQuery(request.Id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            Response = result.Value!;
            await SendOkAsync(Response, cancellationToken);
        }
        else
        {
            await SendNotFoundAsync(cancellationToken);
        }
    }
}
