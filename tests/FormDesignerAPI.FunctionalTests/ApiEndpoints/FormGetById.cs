using FormDesignerAPI.Infrastructure.Data;
using FormDesignerAPI.Web.Forms;

namespace FormDesignerAPI.FunctionalTests.ApiEndpoints;

public class FormGetById(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ReturnsSeedFormGivenId1()
    {
        var result = await _client.GetAndDeserializeAsync<FormRecord>(GetFormByIdRequest.BuildRoute(1));

        result.Id.ShouldBe(1);
        result.FormNumber.ShouldBe(SeedData.Form1.FormNumber);
    }

    [Fact]
    public async Task ReturnsNotFoundGivenId1000()
    {
        string route = GetFormByIdRequest.BuildRoute(1000);
        _ = await _client.GetAndEnsureNotFoundAsync(route);
    }
}
