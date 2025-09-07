using System.ComponentModel.DataAnnotations;

namespace FormDesignerAPI.Web.Forms;

public class UpdateFormRequest
{
    public const string Route = "/Forms/{FormId:Int}";

    public static string BuildRoute(int formId) => Route.Replace("{FormId:Int}", formId.ToString());

    public int FormId { get; set; }

    public int Id { get; set; }

    [Required]
    public string? FormNumber { get; set; }
    public string? FormTitle { get; set; }
    public string? Division { get; set; }
    public string? Owner { get; set; }
    public string? Version { get; set; }
    public string? Status { get; set; } // I don't know if this will work
    public DateTime? CreatedDate { get; set; }
    public DateTime? RevisedDate { get; set; } = DateTime.UtcNow;
    public string? ConfigurationPath { get; set; }
}
