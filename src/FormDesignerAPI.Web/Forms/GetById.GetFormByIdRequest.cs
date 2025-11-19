using System;

namespace FormDesignerAPI.Web.Forms;

public class GetFormByIdRequest
{
    public const string Route = "/Forms/{FormId:Guid}";
    public static string BuildRoute(Guid formId) => Route.Replace("{FormId:Guid}", formId.ToString());

    public Guid FormId { get; set; }
}
