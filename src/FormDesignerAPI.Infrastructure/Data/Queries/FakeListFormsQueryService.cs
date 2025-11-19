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
                new Guid(),
                "Form 1",
                "Description 1",
                "Category 1",
                "Owner 1",
                Core.FormAggregate.Version.Create(1,0,0, new FormDefinition("/path/to/config"))!,
                DateTime.UtcNow,
                DateTime.UtcNow
            ),
            new FormDTO(
                new Guid(),
                "Form 2",
                "Description 2",
                "Category 2",
                "Owner 2",
                Core.FormAggregate.Version.Create(1,0,1, new FormDefinition("/path/to/config"))!,
                DateTime.UtcNow,
                DateTime.UtcNow
            )];
        return Task.FromResult(forms);
    }
}
