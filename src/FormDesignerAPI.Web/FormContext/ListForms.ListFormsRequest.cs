namespace FormDesignerAPI.Web.FormContext;

public class ListFormsRequest
{
    public const string Route = "/api/forms";
    public bool? ActiveOnly { get; set; }
    public string? SearchTerm { get; set; }
}
