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
                "Status 1",
                DateTime.UtcNow,
                DateTime.UtcNow,
                null
            ),
            new FormDTO(
                2,
                "Form 2",
                "Description 2",
                "Category 2",
                "Owner 2",
                "Status 2",
                DateTime.UtcNow,
                DateTime.UtcNow,
                null
            )];
        return Task.FromResult(forms);
    }
}
