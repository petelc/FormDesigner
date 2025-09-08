using Projects;

namespace FormDesignerAPI.AspireTests.Tests;

public class AspireIntegrationTests
{
    // Follow the link below to write you tests with Aspire
    // https://learn.microsoft.com/en-us/dotnet/aspire/testing/write-your-first-test?pivots=xunit
    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.FormDesignerAPI_AspireHost>();

        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        // To capture logs from your tests, see the "Capture logs from tests" section
        // in the documentation or refer to LoggingTest.cs for a complete example

        await using var app = await builder.BuildAsync();

        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("web");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await app.ResourceNotifications.WaitForResourceHealthyAsync(
            "web",
            cts.Token);

        var response = await httpClient.GetAsync("/Contributors", cts.Token);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
