using FormDesignerAPI.Infrastructure.Data;
using FormDesignerAPI.Web.Forms;

namespace FormDesignerAPI.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class FormList(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ReturnsTwoForms()
    {
        var result = await _client.GetAndDeserializeAsync<FormListResponse>("/Forms");

        result.Forms.Count.ShouldBe(2);
        result.Forms.ShouldContain(form => form.FormNumber == SeedData.Form1.FormNumber);
        result.Forms.ShouldContain(form => form.FormNumber == SeedData.Form2.FormNumber);

    }


}
