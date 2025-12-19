using FormDesignerAPI.Core.ProjectContext.Aggregates;
using FormDesignerAPI.Core.ProjectContext.Interfaces;
using FormDesignerAPI.Core.ProjectContext.ValueObjects;
using FormDesignerAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FormDesignerAPI.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Project aggregate
/// </summary>
public class ProjectRepository : EfRepository<Project>, IProjectRepository
{
  private readonly AppDbContext _dbContext;

  public ProjectRepository(AppDbContext dbContext) : base(dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<Project?> GetByIdWithFormsAsync(
    Guid id,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Projects
      .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
  }

  public async Task<Project?> GetByIdWithJobsAsync(
    Guid id,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Projects
      .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
  }

  public async Task<Project?> GetByIdCompleteAsync(
    Guid id,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Projects
      .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
  }

  public async Task<IEnumerable<Project>> FindByStatusAsync(
    ProjectStatus status,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Projects
      .Where(p => p.Status == status)
      .OrderByDescending(p => p.UpdatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<Project>> FindByCreatedByAsync(
    string createdBy,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Projects
      .Where(p => p.CreatedBy == createdBy)
      .OrderByDescending(p => p.CreatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<Project>> SearchByNameAsync(
    string searchTerm,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Projects
      .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
      .OrderByDescending(p => p.UpdatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<(IEnumerable<Project> Projects, int TotalCount)> GetPagedAsync(
    int pageNumber,
    int pageSize,
    string? searchTerm = null,
    ProjectStatus? status = null,
    CancellationToken cancellationToken = default)
  {
    var query = _dbContext.Projects.AsQueryable();

    // Apply filters
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
      query = query.Where(p =>
        p.Name.Contains(searchTerm) ||
        p.Description.Contains(searchTerm));
    }

    if (status.HasValue)
    {
      query = query.Where(p => p.Status == status.Value);
    }

    // Get total count
    var totalCount = await query.CountAsync(cancellationToken);

    // Get paged results
    var projects = await query
      .OrderByDescending(p => p.UpdatedAt)
      .Skip((pageNumber - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);

    return (projects, totalCount);
  }
}
