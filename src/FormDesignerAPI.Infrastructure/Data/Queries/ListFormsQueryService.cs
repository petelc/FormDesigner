using FormDesignerAPI.UseCases.Forms;
using FormDesignerAPI.UseCases.Forms.List;

namespace FormDesignerAPI.Infrastructure.Data.Queries;

public class ListFormsQueryService(AppDbContext _db) : IListFormsQueryService
{
    public async Task<IEnumerable<FormDTO>> ListFormsAsync()
    {
        var result = await _db.Database.SqlQuery<FormDTO>(
            $"SELECT Id, FormNumber, FormTitle, Division, Owner_Name AS Owner, Version, CreatedDate, RevisedDate, ConfigurationPath FROM Forms"
        ).ToListAsync();

        return result;
    }
}
