using Traxs.SharedKernel;
using FormDesignerAPI.Core.FormContext.Aggregates;

namespace FormDesignerAPI.Core.FormContext.Interfaces;

/// <summary>
/// Repository interface for Form aggregate
/// Inherits from Traxs.SharedKernel IRepository which uses Ardalis.Specification
/// </summary>
public interface IFormRepository : IRepository<Form>
{
    /// <summary>
    /// Get form with all its revisions eagerly loaded
    /// </summary>
    Task<Form?> GetByIdWithRevisionsAsync(
      Guid id,
      CancellationToken cancellationToken = default);

    /// <summary>
    /// Get form by name
    /// </summary>
    Task<Form?> GetByNameAsync(
      string name,
      CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a form with the given name already exists
    /// </summary>
    Task<bool> ExistsWithNameAsync(
      string name,
      CancellationToken cancellationToken = default);
}