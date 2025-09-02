using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.IntegrationTests.Data;

public class EfRepositoryFormDelete : BaseEfRepoTestFixture
{
    [Fact]
    public async Task DeletesFormAfterAddingIt()
    {
        var testFormNumber = "XXX1000";
        var testFormTitle = "Test Form";
        var repository = GetFormRepository();

        var form = new Form(testFormNumber, testFormTitle);
        await repository.AddAsync(form);

        await repository.DeleteAsync(form);

        (await repository.ListAsync()).ShouldNotContain(Form => Form.FormNumber == testFormNumber);
    }
}
