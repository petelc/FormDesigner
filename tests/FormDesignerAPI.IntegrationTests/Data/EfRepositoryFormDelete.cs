using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.IntegrationTests.Data;

/// <summary>
/// Integration tests for deleting forms via the EF repository.
/// </summary>
public class EfRepositoryFormDelete : BaseEfRepoTestFixture
{
    [Fact]
    public async Task DeletesFormAfterAddingIt()
    {
        // Arrange
        var testFormNumber = "XXX1000";
        var testFormTitle = "Test Form";
        var repository = GetFormRepository();

        var form = Form.CreateBuilder(testFormNumber)
            .WithTitle(testFormTitle)
            .Build();
        await repository.AddAsync(form);

        // Act
        await repository.DeleteAsync(form);

        // Assert
        (await repository.ListAsync()).ShouldNotContain(f => f.FormNumber == testFormNumber);
    }
}
