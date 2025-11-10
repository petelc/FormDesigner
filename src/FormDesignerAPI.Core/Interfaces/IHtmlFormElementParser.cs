namespace FormDesignerAPI.Core.Interfaces;

public interface IHtmlFormElementParser
{
    Dictionary<string, string> ExtractFormElements(string htmlContent);
    string GenerateCreateTableSql(string tableName, Dictionary<string, string> columns);
    string GenerateCSharpModel(string className, Dictionary<string, string> columns);
}
