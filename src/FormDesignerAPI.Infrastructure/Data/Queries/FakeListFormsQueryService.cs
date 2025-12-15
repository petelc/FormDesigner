using Azure.Core;
using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.UseCases.Forms;
using FormDesignerAPI.UseCases.Forms.List;

namespace FormDesignerAPI.Infrastructure.Data.Queries;

public class FakeListFormsQueryService : IListFormsQueryService
{
    public Task<IEnumerable<FormDTO>> ListFormsAsync()
    {
        IEnumerable<FormDTO> forms =
        [
            new FormDTO
            {
                Id = 1,
                FormNumber = "FORM-001",
                FormTitle = "Sample Form 1",
                Division = "Division 1",
                Owner = "Owner 1",
                Version = "1.0",
                CreatedDate = DateTime.UtcNow,
                RevisedDate = DateTime.UtcNow,
                ConfigurationPath = "path/to/config1.json"
            },
            new FormDTO
            {
                Id = 2,
                FormNumber = "FORM-002",
                FormTitle = "Sample Form 2",
                Division = "Division 2",
                Owner = "Owner 2",
                Version = "1.0",
                CreatedDate = DateTime.UtcNow,
                RevisedDate = DateTime.UtcNow,
                ConfigurationPath = "path/to/config2.json"
            }
        ];
        return Task.FromResult(forms);
    }
}
