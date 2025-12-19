using FormDesignerAPI.Core.ProjectContext.Aggregates;
using FormDesignerAPI.Core.ProjectContext.ValueObjects;

namespace FormDesignerAPI.Core.ProjectContext.Interfaces;

/// <summary>
/// Repository interface for Project aggregate
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
  /// <summary>
  /// Get project with all related forms
  /// </summary>
  Task<Project?> GetByIdWithFormsAsync(Guid id, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get project with all code generation jobs
  /// </summary>
  Task<Project?> GetByIdWithJobsAsync(Guid id, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get project with complete details (forms + jobs)
  /// </summary>
  Task<Project?> GetByIdCompleteAsync(Guid id, CancellationToken cancellationToken = default);

  /// <summary>
  /// Find projects by status
  /// </summary>
  Task<IEnumerable<Project>> FindByStatusAsync(
    ProjectStatus status,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Find projects created by user
  /// </summary>
  Task<IEnumerable<Project>> FindByCreatedByAsync(
    string createdBy,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Search projects by name
  /// </summary>
  Task<IEnumerable<Project>> SearchByNameAsync(
    string searchTerm,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Get projects with pagination
  /// </summary>
  Task<(IEnumerable<Project> Projects, int TotalCount)> GetPagedAsync(
    int pageNumber,
    int pageSize,
    string? searchTerm = null,
    ProjectStatus? status = null,
    CancellationToken cancellationToken = default);
}
