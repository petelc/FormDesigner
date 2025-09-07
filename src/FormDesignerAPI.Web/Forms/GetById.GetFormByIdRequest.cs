using System;

namespace FormDesignerAPI.Web.Forms;

public class GetFormByIdRequest
{
    public const string Route = "/Forms/{FormId:int}";
    public static string BuildRoute(int formId) => Route.Replace("{FormId:int}", formId.ToString());

    public int FormId { get; set; }
}
