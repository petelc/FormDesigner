using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.Core.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;
using Shouldly;
using Xunit;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FormDesignerAPI.UnitTests.Core.Services;

public class FormDefinitionServiceTests
{
    private readonly ILogger<FormDefinitionService> _logger;
    private readonly IHtmlFormElementParser _htmlParser;
    private readonly FormDefinitionService _service;

    public FormDefinitionServiceTests()
    {
        _logger = Substitute.For<ILogger<FormDefinitionService>>();
        _htmlParser = Substitute.For<IHtmlFormElementParser>();
        _service = new FormDefinitionService(_logger, _htmlParser);
    }

    [Fact]
    public async Task GenerateFormDefinitionAsync_WithValidJson_ShouldProcessSuccessfully()
    {
        // Arrange
        var formId = 123;
        var json = @"{
            ""children"": [
                {
                    ""id"": ""1"",
                    ""content"": ""<input name='userName' type='text'>""
                }
            ]
        }";

        var expectedFormElements = new Dictionary<string, string>
        {
            ["userName"] = "NVARCHAR(255)"
        };

        _htmlParser.ExtractFormElements(Arg.Any<string>())
            .Returns(expectedFormElements);

        _htmlParser.GenerateCreateTableSql(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>())
        .Returns("CREATE TABLE [FormData] ([userName] NVARCHAR(255));");

        _htmlParser.GenerateCSharpModel(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>())
        .Returns("public class FormDataModel { public string UserName { get; set; } }");

        // Act
        var result = await _service.GenerateFormDefinitionAsync(formId, json, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify the HTML parser was called with the correct content
        _htmlParser.Received(1).ExtractFormElements("<input name='userName' type='text'>");

        // Verify the correct dictionary contents were passed
        _htmlParser.Received(1).GenerateCreateTableSql("FormData",
            Arg.Is<Dictionary<string, string>>(d =>
                d.Count == 1 &&
                d.ContainsKey("userName") &&
                d["userName"] == "NVARCHAR(255)"));

        _htmlParser.Received(1).GenerateCSharpModel("FormDataModel",
            Arg.Is<Dictionary<string, string>>(d =>
                d.Count == 1 &&
                d.ContainsKey("userName") &&
                d["userName"] == "NVARCHAR(255)"));
    }

    [Fact]
    public async Task GenerateFormDefinitionAsync_WithMultipleChildren_ShouldMergeFormElements()
    {
        // Arrange
        var formId = 123;
        var json = @"{
            ""children"": [
                {
                    ""id"": ""1"",
                    ""content"": ""<input name='userName' type='text'>""
                },
                {
                    ""id"": ""2"",
                    ""content"": ""<input name='age' type='number'>""
                }
            ]
        }";

        _htmlParser.ExtractFormElements(Arg.Any<string>())
            .Returns(new Dictionary<string, string> { ["userName"] = "NVARCHAR(255)" });

        _htmlParser.ExtractFormElements("<input name='userName' type='text'>")
            .Returns(new Dictionary<string, string> { ["userName"] = "NVARCHAR(255)" });

        _htmlParser.ExtractFormElements("<input name='age' type='number'>")
            .Returns(new Dictionary<string, string> { ["age"] = "INT" });

        var expectedMergedElements = new Dictionary<string, string>
        {
            ["userName"] = "NVARCHAR(255)",
            ["age"] = "INT"
        };

        _htmlParser.GenerateCreateTableSql("FormData", Arg.Is<Dictionary<string, string>>(
            d => d.ContainsKey("userName") && d.ContainsKey("age")))
            .Returns("CREATE TABLE [FormData] ([userName] NVARCHAR(255), [age] INT);");

        _htmlParser.GenerateCSharpModel("FormDataModel", Arg.Any<Dictionary<string, string>>())
            .Returns("public class FormDataModel { public string UserName { get; set; } public int Age { get; set; } }");

        // Act
        var result = await _service.GenerateFormDefinitionAsync(formId, json, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify both content strings were processed
        _htmlParser.Received(1).ExtractFormElements("<input name='userName' type='text'>");
        _htmlParser.Received(1).ExtractFormElements("<input name='age' type='number'>");

        // Verify SQL and C# generation was called once with merged results
        _htmlParser.Received(1).GenerateCreateTableSql("FormData", Arg.Any<Dictionary<string, string>>());
        _htmlParser.Received(1).GenerateCSharpModel("FormDataModel", Arg.Any<Dictionary<string, string>>());
    }

    [Fact]
    public async Task GenerateFormDefinitionAsync_WithDuplicateFieldNames_ShouldNotOverwrite()
    {
        // Arrange
        var formId = 123;
        var json = @"{
            ""children"": [
                {
                    ""id"": ""1"",
                    ""content"": ""<input name='userName' type='text'>""
                },
                {
                    ""id"": ""2"",
                    ""content"": ""<input name='userName' type='email'>""
                }
            ]
        }";

        _htmlParser.ExtractFormElements("<input name='userName' type='text'>")
            .Returns(new Dictionary<string, string> { ["userName"] = "NVARCHAR(255)" });

        _htmlParser.ExtractFormElements("<input name='userName' type='email'>")
            .Returns(new Dictionary<string, string> { ["userName"] = "NVARCHAR(255)" });

        // Act
        var result = await _service.GenerateFormDefinitionAsync(formId, json, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify the merged dictionary was passed with only one userName entry
        _htmlParser.Received(1).GenerateCreateTableSql("FormData",
            Arg.Is<Dictionary<string, string>>(d => d.Count == 1 && d.ContainsKey("userName")));
    }

    [Fact]
    public async Task GenerateFormDefinitionAsync_WithNoChildren_ShouldSucceedWithoutProcessing()
    {
        // Arrange
        var formId = 123;
        var json = @"{ ""someOtherProperty"": ""value"" }";

        // Act
        var result = await _service.GenerateFormDefinitionAsync(formId, json, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify no HTML parsing was called
        _htmlParser.DidNotReceive().ExtractFormElements(Arg.Any<string>());
        _htmlParser.DidNotReceive().GenerateCreateTableSql(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>());
        _htmlParser.DidNotReceive().GenerateCSharpModel(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>());
    }

    [Fact]
    public async Task GenerateFormDefinitionAsync_WithChildrenButNoContent_ShouldSucceedWithoutProcessing()
    {
        // Arrange
        var formId = 123;
        var json = @"{
            ""children"": [
                {
                    ""id"": ""1"",
                    ""x"": 0,
                    ""y"": 0
                }
            ]
        }";

        // Act
        var result = await _service.GenerateFormDefinitionAsync(formId, json, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify no HTML parsing was called since there's no content property
        _htmlParser.DidNotReceive().ExtractFormElements(Arg.Any<string>());
    }

    [Fact]
    public async Task GenerateFormDefinitionAsync_WithInvalidJson_ShouldReturnError()
    {
        // Arrange
        var formId = 123;
        var invalidJson = "{ invalid json }";

        // Act
        var result = await _service.GenerateFormDefinitionAsync(formId, invalidJson, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();

        // Verify error was logged - Use the correct logging verification pattern
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Error generating form definition for Form")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GenerateFormDefinitionAsync_WithRealWorldExample_ShouldProcess()
    {
        // Arrange
        var formId = 123;
        var json = @"{
            ""minRow"": 1,
            ""float"": true,
            ""cellHeight"": 80,
            ""children"": [
                {
                    ""x"": 2,
                    ""y"": 0,
                    ""w"": 4,
                    ""h"": 3,
                    ""content"": ""<textarea name=\""textArea\"" type=\""text\""></textarea><input name=\""inputField\"" type=\""text\""><div contenteditable=\""true\"">Editable Div</div>"",
                    ""id"": ""1""
                }
            ]
        }";

        var expectedFormElements = new Dictionary<string, string>
        {
            ["textArea"] = "NVARCHAR(MAX)",
            ["inputField"] = "NVARCHAR(255)"
        };

        _htmlParser.ExtractFormElements(Arg.Any<string>())
            .Returns(expectedFormElements);

        // Act
        var result = await _service.GenerateFormDefinitionAsync(formId, json, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _htmlParser.Received(1).ExtractFormElements(Arg.Any<string>());

        _htmlParser.Received(1).GenerateCreateTableSql("FormData",
            Arg.Any<Dictionary<string, string>>());


        // _htmlParser.Received(1).GenerateCreateTableSql("FormData", expectedFormElements);
        _htmlParser.Received(1).GenerateCSharpModel("FormDataModel", Arg.Any<Dictionary<string, string>>());
    }
}