using FormDesignerAPI.UseCases.ProjectContext;
using FormDesignerAPI.UseCases.ProjectContext.Update;
using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.Web.ProjectContext;

/// <summary>
/// Request to update a project
/// </summary>
public record UpdateProjectRequest
{
    public const string Route = "/api/projects/{id}";

    [BindFrom("id")]
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// Update an existing project
/// </summary>
public class UpdateProject(IMediator _mediator, IUser _currentUser)
  : Endpoint<UpdateProjectRequest, ProjectDTO>
{
    public override void Configure()
    {
        Put(UpdateProjectRequest.Route);

        // Just require authentication, no specific roles

        Summary(s =>
        {
            s.Summary = "Update a project";
            s.Description = "Updates an existing project's details (requires authentication)";
            s.Responses[200] = "Project updated successfully";
            s.Responses[401] = "Unauthorized - authentication required";
            s.Responses[404] = "Project not found";
        });
    }

    public override async Task HandleAsync(
      UpdateProjectRequest request,
      CancellationToken cancellationToken)
    {
        // Check if user is authenticated
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.Id))
        {
            await SendUnauthorizedAsync(cancellationToken);
            return;
        }

        var command = new UpdateProjectCommand(
            request.Id,
            request.Name,
            request.Description,
            _currentUser.Id
        );

        var result = await _mediator.Send(command, cancellationToken);

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
