using FormDesignerAPI.UseCases.ProjectContext.List;
using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.Web.ProjectContext;

/// <summary>
/// Request to list projects
/// </summary>
public record ListProjectsRequest
{
  public const string Route = "/api/projects";

  [BindFrom("page")]
  public int PageNumber { get; init; } = 1;
  
  [BindFrom("pageSize")]
  public int PageSize { get; init; } = 20;
  
  [BindFrom("search")]
  public string? SearchTerm { get; init; }
  
  [BindFrom("status")]
  public string? Status { get; init; }
}

/// <summary>
/// List projects with pagination
/// </summary>
public class ListProjects(IMediator _mediator, IUser _currentUser)
  : Endpoint<ListProjectsRequest, PagedProjectsResult>
{
  public override void Configure()
  {
    Get(ListProjectsRequest.Route);
    
    // Just require authentication, no specific roles
    
    Summary(s =>
    {
      s.Summary = "List all projects";
      s.Description = "Retrieves projects with pagination and optional filtering (requires authentication)";
      s.Responses[200] = "List of projects";
      s.Responses[401] = "Unauthorized - authentication required";
      s.Params["page"] = "Page number (default: 1)";
      s.Params["pageSize"] = "Items per page (default: 20)";
      s.Params["search"] = "Search term to filter by name or description";
      s.Params["status"] = "Filter by project status (DRAFT, PDF_UPLOADED, etc.)";
    });
  }

  public override async Task HandleAsync(
    ListProjectsRequest request,
    CancellationToken cancellationToken)
  {
    // Check if user is authenticated
    if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.Id))
    {
      await SendUnauthorizedAsync(cancellationToken);
      return;
    }

    var query = new ListProjectsQuery(
      request.PageNumber,
      request.PageSize,
      request.SearchTerm,
      request.Status
    );

    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
      Response = result.Value!;
      await SendOkAsync(Response, cancellationToken);
    }
    else
    {
      await SendNoContentAsync(cancellationToken);
    }
  }
}
