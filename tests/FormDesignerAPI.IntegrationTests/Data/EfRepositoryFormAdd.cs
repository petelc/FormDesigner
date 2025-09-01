using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.IntegrationTests.Data;

public class EfRepositoryFormAdd : BaseEfRepoTestFixture
{
    [Fact]
    public async Task AddsFormAndSetsId()
    {
        var testFormName = "testForm";
        var repository = GetFormRepository();
        // TODO: add the required properties
        // TODO: add the set properties to the form aggregate
        var form = new Form(testFormName)
        {
            Division = "Test Division",
            Owner = new Owner("Test Owner", string.Empty),
            Version = "1.0",
            ConfigurationPath = "/path/to/config"
        };

        await repository.AddAsync(form);

        var newForm = (await repository.ListAsync())
                        .FirstOrDefault();

        newForm.ShouldNotBeNull();
        testFormName.ShouldBe(newForm.FormNumber);
        newForm.Id.ShouldBeGreaterThan(0);
    }
}
