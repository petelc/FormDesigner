using FormDesignerAPI.UseCases.ProjectContext.Delete;
using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.Web.ProjectContext;

/// <summary>
/// Request to delete a project
/// </summary>
public record DeleteProjectRequest
{
    public const string Route = "/api/projects/{id}";

    [BindFrom("id")]
    public Guid Id { get; init; }
}

/// <summary>
/// Response for delete operation
/// </summary>
public record DeleteProjectResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Delete a project
/// </summary>
public class DeleteProject(IMediator _mediator, IUser _currentUser)
  : Endpoint<DeleteProjectRequest, DeleteProjectResponse>
{
    public override void Configure()
    {
        Delete(DeleteProjectRequest.Route);

        // Just require authentication, no specific roles

        Summary(s =>
        {
            s.Summary = "Delete a project";
            s.Description = "Deletes an existing project (requires authentication)";
            s.Responses[200] = "Project deleted successfully";
            s.Responses[401] = "Unauthorized - authentication required";
            s.Responses[404] = "Project not found";
        });
    }

    public override async Task HandleAsync(
      DeleteProjectRequest request,
      CancellationToken cancellationToken)
    {
        // Check if user is authenticated
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.Id))
        {
            await SendUnauthorizedAsync(cancellationToken);
            return;
        }

        var command = new DeleteProjectCommand(request.Id);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            Response = new DeleteProjectResponse
            {
                Success = true,
                Message = "Project deleted successfully"
            };
            await SendOkAsync(Response, cancellationToken);
        }
        else
        {
            await SendNotFoundAsync(cancellationToken);
        }
    }
}
