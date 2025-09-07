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
            new FormDTO(
                1,
                "Form 1",
                "Description 1",
                "Category 1",
                "Owner 1",
                "Version 1",
                Status: FormStatus.Draft,
                DateTime.UtcNow,
                DateTime.UtcNow,
                "path/to/config1.json"
            ),
            new FormDTO(
                2,
                "Form 2",
                "Description 2",
                "Category 2",
                "Owner 2",
                "Version 2",
                Status: FormStatus.Draft,
                DateTime.UtcNow,
                DateTime.UtcNow,
                "path/to/config2.json"
            )];
        return Task.FromResult(forms);
    }
}
