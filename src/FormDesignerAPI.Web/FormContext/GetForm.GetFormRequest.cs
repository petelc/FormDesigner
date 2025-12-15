namespace FormDesignerAPI.Web.FormContext;

public class GetFormRequest
{
    public const string Route = "/api/forms/{Id}";
    public Guid Id { get; set; }
}
