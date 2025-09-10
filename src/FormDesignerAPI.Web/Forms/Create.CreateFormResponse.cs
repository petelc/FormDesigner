using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.Web.Forms;

public class CreateFormResponse(int id, string formNumber, string formTitle, string? division, Owner? owner, string? version, string? configurationPath)
{
    public int Id { get; } = id;
    public string FormNumber { get; } = formNumber;
    public string FormTitle { get; } = formTitle;
    public string? Division { get; } = division;
    public Owner? Owner { get; } = owner;
    public string? Version { get; } = version;
    public string? ConfigurationPath { get; } = configurationPath;
}
