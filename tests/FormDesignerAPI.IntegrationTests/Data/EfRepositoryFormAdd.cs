using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.IntegrationTests.Data;

/// <summary>
/// Integration tests for adding forms via the EF repository.
/// </summary>
public class EfRepositoryFormAdd : BaseEfRepoTestFixture
{
    [Fact]
    public async Task AddsFormAndSetsId()
    {
        // Arrange
        var testFormNumber = "XXX1000";
        var testFormTitle = "Test Form";
        var repository = GetFormRepository();

        var form = Form.CreateBuilder(testFormNumber)
            .WithTitle(testFormTitle)
            .Build();

        // Act
        await repository.AddAsync(form);

        // Assert
        var newForm = (await repository.ListAsync())
                        .FirstOrDefault();

        newForm.ShouldNotBeNull();
        testFormNumber.ShouldBe(newForm.FormNumber);
        newForm.FormId.ShouldNotBe(Guid.Empty);
    }
}
