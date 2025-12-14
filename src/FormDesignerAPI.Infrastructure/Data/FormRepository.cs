using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FormDesignerAPI.Infrastructure.Data;

/// <summary>
/// Repository implementation for Form aggregate
/// </summary>
public class FormRepository : EfRepository<Form>, IFormRepository
{
    private readonly AppDbContext _dbContext;

    public FormRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Get form with all its revisions eagerly loaded
    /// </summary>
    public async Task<Form?> GetByIdWithRevisionsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Forms
            .Include(f => f.Revisions)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    /// <summary>
    /// Get form by name
    /// </summary>
    public async Task<Form?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Forms
            .FirstOrDefaultAsync(f => f.Name == name, cancellationToken);
    }

    /// <summary>
    /// Check if a form with the given name already exists
    /// </summary>
    public async Task<bool> ExistsWithNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Forms
            .AnyAsync(f => f.Name == name, cancellationToken);
    }
}
