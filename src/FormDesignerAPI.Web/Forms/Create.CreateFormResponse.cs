using System;

namespace FormDesignerAPI.Web.Forms;

public class CreateFormResponse(int id, string formNumber, string formTitle)
{
    public int Id { get; } = id;
    public string FormNumber { get; } = formNumber;
    public string FormTitle { get; } = formTitle;
}
