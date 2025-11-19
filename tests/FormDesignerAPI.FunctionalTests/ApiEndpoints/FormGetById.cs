using FormDesignerAPI.Infrastructure.Data;
using FormDesignerAPI.UseCases.Forms;
using FormDesignerAPI.Web.Forms;

namespace FormDesignerAPI.FunctionalTests.ApiEndpoints;

/// <summary>
/// Functional tests for the Get Form by ID endpoint.
/// </summary>
public class FormGetById(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ReturnsSeedFormGivenForm1Id()
    {
        // Arrange
        var formId = SeedData.Form1.FormId;
        var route = GetFormByIdRequest.BuildRoute(formId);

        // Act
        var result = await _client.GetAndDeserializeAsync<FormDTO>(route);

        // Assert
        result.ShouldNotBeNull();
        result.FormNumber.ShouldBe(SeedData.Form1.FormNumber);
        result.FormTitle.ShouldBe(SeedData.Form1.FormTitle);
        result.Division.ShouldBe(SeedData.Form1.Division);
    }

    [Fact]
    public async Task ReturnsNotFoundGivenNonExistentFormId()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var route = GetFormByIdRequest.BuildRoute(nonExistentId);

        // Act & Assert
        _ = await _client.GetAndEnsureNotFoundAsync(route);
    }

    [Fact]
    public async Task ReturnsFormWithOwnerInfo()
    {
        // Arrange
        var formId = SeedData.Form1.FormId;
        var route = GetFormByIdRequest.BuildRoute(formId);

        // Act
        var result = await _client.GetAndDeserializeAsync<FormDTO>(route);

        // Assert
        result.ShouldNotBeNull();
        result.Owner.ShouldBe(SeedData.Form1.Owner?.Name);
    }
}

