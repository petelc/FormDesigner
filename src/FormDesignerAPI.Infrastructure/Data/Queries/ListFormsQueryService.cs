using FormDesignerAPI.UseCases.Forms;
using FormDesignerAPI.UseCases.Forms.List;

namespace FormDesignerAPI.Infrastructure.Data.Queries;

public class ListFormsQueryService(AppDbContext _db) : IListFormsQueryService
{
    public async Task<IEnumerable<FormDTO>> ListFormsAsync()
    {
        // Get raw data from database using a simple DTO
        var rawResults = await _db.Database.SqlQuery<FormQueryResult>(
            $"SELECT Id, FormNumber, FormTitle, Division, Owner_Name AS Owner, Revision, CreatedDate, RevisedDate FROM Forms"
        ).ToListAsync();

        // Convert to FormDTO, creating stable Guids from integer IDs
        return rawResults.Select(r => new FormDTO(
            ConvertIntToGuid(r.Id),
            r.FormNumber,
            r.FormTitle,
            r.Division,
            r.Owner,
            r.Revision,
            r.CreatedDate,
            r.RevisedDate
        )).ToList();
    }

    private static Guid ConvertIntToGuid(int id)
    {
        // Create a stable Guid from an integer ID
        var bytes = new byte[16];
        BitConverter.GetBytes(id).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}

/// <summary>
/// Intermediate DTO for raw SQL query results
/// </summary>
internal class FormQueryResult
{
    public int Id { get; set; }
    public string FormNumber { get; set; } = null!;
    public string FormTitle { get; set; } = null!;
    public string? Division { get; set; }
    public string? Owner { get; set; }
    public string? Revision { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime RevisedDate { get; set; }
}

