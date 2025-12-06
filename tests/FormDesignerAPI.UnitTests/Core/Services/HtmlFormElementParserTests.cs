using FormDesignerAPI.Core.Services;
using Shouldly;
using Xunit;

namespace FormDesignerAPI.UnitTests.Core.Services;

public class HtmlFormElementParserTests
{
    private readonly HtmlFormElementParser _parser;

    public HtmlFormElementParserTests()
    {
        _parser = new HtmlFormElementParser();
    }

    [Fact]
    public void ExtractFormElements_WithInputFields_ShouldExtractCorrectly()
    {
        // Arrange
        var htmlContent = @"<input name=""userName"" type=""text""><input name=""age"" type=""number"">";

        // Act
        var result = _parser.ExtractFormElements(htmlContent);

        // Assert
        result.ShouldNotBeEmpty();
        result.ShouldContainKeyAndValue("userName", "NVARCHAR(255)");
        result.ShouldContainKeyAndValue("age", "INT");
    }

    [Fact]
    public void ExtractFormElements_WithTextarea_ShouldExtractCorrectly()
    {
        // Arrange
        var htmlContent = @"<textarea name=""description""></textarea>";

        // Act
        var result = _parser.ExtractFormElements(htmlContent);

        // Assert
        result.ShouldContainKeyAndValue("description", "NVARCHAR(MAX)");
    }

    [Fact]
    public void ExtractFormElements_WithSelect_ShouldExtractCorrectly()
    {
        // Arrange
        var htmlContent = @"<select name=""country""><option>USA</option></select>";

        // Act
        var result = _parser.ExtractFormElements(htmlContent);

        // Assert
        result.ShouldContainKeyAndValue("country", "NVARCHAR(MAX)");
    }

    [Fact]
    public void ExtractFormElements_WithMixedElements_ShouldExtractAll()
    {
        // Arrange
        var htmlContent = @"
            <input name=""email"" type=""email"">
            <textarea name=""comments""></textarea>
            <input name=""birthDate"" type=""date"">
            <input name=""isActive"" type=""checkbox"">
        ";

        // Act
        var result = _parser.ExtractFormElements(htmlContent);

        // Assert
        result.Count.ShouldBe(4);
        result.ShouldContainKeyAndValue("email", "NVARCHAR(255)");
        result.ShouldContainKeyAndValue("comments", "NVARCHAR(MAX)");
        result.ShouldContainKeyAndValue("birthDate", "DATE");
        result.ShouldContainKeyAndValue("isActive", "BIT");
    }

    [Fact]
    public void ExtractFormElements_WithRealWorldExample_ShouldExtractCorrectly()
    {
        // Arrange
        var htmlContent = @"<button onclick=""removeWidget(this.parentElement.parentElement)"">X</button><br> 1<br> <button onclick=""alert('clicked!')"">Press me</button><div>text area</div><div><textarea name=""textArea"" type=""text""></textarea></div><div>Input Field</div><input name=""inputField"" type=""text""><div contenteditable=""true"">Editable Div</div><div class=""no-drag"">no drag</div>";

        // Act
        var result = _parser.ExtractFormElements(htmlContent);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContainKeyAndValue("textArea", "NVARCHAR(MAX)");
        result.ShouldContainKeyAndValue("inputField", "NVARCHAR(255)");
    }

    [Fact]
    public void ExtractFormElements_WithNoFormElements_ShouldReturnEmpty()
    {
        // Arrange
        var htmlContent = @"<div>Just a div</div><p>Just a paragraph</p>";

        // Act
        var result = _parser.ExtractFormElements(htmlContent);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void ExtractFormElements_WithDuplicateNames_ShouldNotOverwrite()
    {
        // Arrange
        var htmlContent = @"<input name=""userName"" type=""text""><input name=""userName"" type=""email"">";

        // Act
        var result = _parser.ExtractFormElements(htmlContent);

        // Assert
        result.Count.ShouldBe(1);
        result.ShouldContainKeyAndValue("userName", "NVARCHAR(255)"); // First one wins
    }

    [Theory]
    [InlineData("text", "NVARCHAR(255)")]
    [InlineData("email", "NVARCHAR(255)")]
    [InlineData("password", "NVARCHAR(255)")]
    [InlineData("number", "INT")]
    [InlineData("decimal", "DECIMAL(18,2)")]
    [InlineData("date", "DATE")]
    [InlineData("datetime-local", "DATETIME2")]
    [InlineData("time", "TIME")]
    [InlineData("checkbox", "BIT")]
    [InlineData("radio", "NVARCHAR(100)")]
    [InlineData("unknown", "NVARCHAR(255)")]
    public void ExtractFormElements_WithDifferentInputTypes_ShouldMapCorrectly(string inputType, string expectedSqlType)
    {
        // Arrange
        var htmlContent = $@"<input name=""testField"" type=""{inputType}"">";

        // Act
        var result = _parser.ExtractFormElements(htmlContent);

        // Assert
        result.ShouldContainKeyAndValue("testField", expectedSqlType);
    }

    [Fact]
    public void GenerateCreateTableSql_WithValidColumns_ShouldGenerateCorrectSql()
    {
        // Arrange
        var columns = new Dictionary<string, string>
        {
            ["userName"] = "NVARCHAR(255)",
            ["age"] = "INT",
            ["isActive"] = "BIT"
        };

        // Act
        var result = _parser.GenerateCreateTableSql("TestTable", columns);

        // Assert
        result.ShouldBe("CREATE TABLE [TestTable] ([userName] NVARCHAR(255), [age] INT, [isActive] BIT);");
    }

    [Fact]
    public void GenerateCreateTableSql_WithEmptyColumns_ShouldReturnEmpty()
    {
        // Arrange
        var columns = new Dictionary<string, string>();

        // Act
        var result = _parser.GenerateCreateTableSql("TestTable", columns);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GenerateCSharpModel_WithValidColumns_ShouldGenerateCorrectModel()
    {
        // Arrange
        var columns = new Dictionary<string, string>
        {
            ["userName"] = "NVARCHAR(255)",
            ["age"] = "INT",
            ["isActive"] = "BIT"
        };

        // Act
        var result = _parser.GenerateCSharpModel("TestModel", columns);

        // Assert
        var expectedModel = @"public class TestModel
{
    public string UserName { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
}";
        result.Trim().ShouldBe(expectedModel);
    }

    [Fact]
    public void GenerateCSharpModel_WithEmptyColumns_ShouldReturnEmpty()
    {
        // Arrange
        var columns = new Dictionary<string, string>();

        // Act
        var result = _parser.GenerateCSharpModel("TestModel", columns);

        // Assert
        result.ShouldBeEmpty();
    }
}