using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.Web.Forms;

public class CreateFormResponse(Guid id, string formNumber, string formTitle, string? division, Owner? owner, Core.FormAggregate.Revision? revision)
{
    public Guid Id { get; } = id;
    public string FormNumber { get; } = formNumber;
    public string FormTitle { get; } = formTitle;
    public string? Division { get; } = division;
    public Owner? Owner { get; } = owner;
    public Core.FormAggregate.Revision? Revision { get; } = revision;

}
