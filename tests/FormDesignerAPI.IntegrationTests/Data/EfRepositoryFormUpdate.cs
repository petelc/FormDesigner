using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.IntegrationTests.Data;

public class EfRepositoryFormUpdate : BaseEfRepoTestFixture
{
    [Fact]
    public async Task UpdatesFormAfterAddingIt()
    {
        var testFormNumber = "XXX1000";
        var testFormTitle = "Test Form";
        var repository = GetFormRepository();

        var Form = new Form(testFormNumber, testFormTitle);
        await repository.AddAsync(Form);

        // detach the item so we get a different instance
        _dbContext.Entry(Form).State = EntityState.Detached;

        // fetch the form and updates its form title
        var newForm = (await repository.ListAsync())
            .FirstOrDefault(f => f.FormNumber == testFormNumber);
        newForm.ShouldNotBeNull();

        Form.ShouldNotBeSameAs(newForm);
        var newFormNumber = "XXX2000";
        var newFormTitle = "Updated Test Form";
        newForm.UpdateFormNumber(newFormNumber);
        newForm.UpdateFormTitle(newFormTitle);

        // Update the form
        await repository.UpdateAsync(newForm);



        var updatedForm = (await repository.ListAsync())
            .FirstOrDefault(Form => Form.FormNumber == newFormNumber);
        updatedForm.ShouldNotBeNull();


        Form.FormNumber.ShouldNotBe(updatedForm.FormNumber);
        Form.Status.ShouldBe(updatedForm.Status);
        newForm.Id.ShouldBe(updatedForm.Id);
    }

}
