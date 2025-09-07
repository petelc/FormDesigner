namespace FormDesignerAPI.Web.Forms;

public class UpdateFormResponse(FormRecord form)
{
    public FormRecord Form { get; set; } = form;
}
