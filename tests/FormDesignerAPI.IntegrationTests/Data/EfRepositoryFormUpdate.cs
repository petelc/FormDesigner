using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.IntegrationTests.Data;

/// <summary>
/// Integration tests for updating forms via the EF repository.
/// </summary>
public class EfRepositoryFormUpdate : BaseEfRepoTestFixture
{
    [Fact]
    public async Task UpdatesFormAfterAddingIt()
    {
        // Arrange
        var testFormNumber = "XXX1000";
        var testFormTitle = "Test Form";
        var repository = GetFormRepository();

        var form = Form.CreateBuilder(testFormNumber)
            .WithTitle(testFormTitle)
            .Build();

        await repository.AddAsync(form);

        // detach the item so we get a different instance
        _dbContext.Entry(form).State = EntityState.Detached;

        // Act - fetch the form and update its properties
        var newForm = (await repository.ListAsync())
            .FirstOrDefault(f => f.FormNumber == testFormNumber);
        newForm.ShouldNotBeNull();

        form.ShouldNotBeSameAs(newForm);
        var newFormNumber = "XXX2000";
        var newFormTitle = "Updated Test Form";
        newForm.UpdateFormNumber(newFormNumber);
        newForm.UpdateFormTitle(newFormTitle);

        // Update the form
        await repository.UpdateAsync(newForm);

        // Assert
        var updatedForm = (await repository.ListAsync())
            .FirstOrDefault(f => f.FormNumber == newFormNumber);
        updatedForm.ShouldNotBeNull();

        form.FormNumber.ShouldNotBe(updatedForm.FormNumber);
        form.Status.ShouldBe(updatedForm.Status);
        newForm.FormId.ShouldBe(updatedForm.FormId);
    }
}
