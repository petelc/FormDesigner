using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace FormDesignerAPI.IntegrationTests.Services;

public class FormDefinitionServiceIntegrationTests
{
    private readonly ServiceProvider _serviceProvider;

    public FormDefinitionServiceIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IHtmlFormElementParser, HtmlFormElementParser>();
        services.AddScoped<IFormDefinitionService, FormDefinitionService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task GenerateFormDefinitionAsync_EndToEnd_ShouldWorkCorrectly()
    {
        // Arrange
        var service = _serviceProvider.GetRequiredService<IFormDefinitionService>();
        var json = @"{
            ""children"": [
                {
                    ""id"": ""1"",
                    ""content"": ""<input name='firstName' type='text'><input name='age' type='number'><textarea name='comments'></textarea>""
                }
            ]
        }";

        // Act
        var result = await service.GenerateFormDefinitionAsync(1, json, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}