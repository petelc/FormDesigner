using System.ComponentModel.DataAnnotations;

namespace FormDesignerAPI.Web.Forms;

/// <summary>
/// Request model for updating an existing form.
/// Version parameters (Major, Minor, Patch) and FormDefinitionPath are optional.
/// If not provided, the current version values will be retained.
/// </summary>
public class UpdateFormRequest
{
    public const string Route = "/Forms/{FormId:Guid}";

    public static string BuildRoute(Guid formId) => Route.Replace("{FormId:Guid}", formId.ToString());

    public Guid FormId { get; set; }

    [Required]
    public string? FormNumber { get; set; }

    [Required]
    public string? FormTitle { get; set; }

    [Required]
    public string? Division { get; set; }

    [Required]
    public string? Owner { get; set; }

    [Required]
    public string? OwnerEmail { get; set; }

    /// <summary>
    /// Optional version parameters. If provided, these create or update the version.
    /// If not provided, the current version values are retained.
    /// </summary>
    public int? VersionMajor { get; set; }
    public int? VersionMinor { get; set; }
    public int? VersionPatch { get; set; }

    /// <summary>
    /// Optional path to the form definition configuration.
    /// If not provided, the current form definition is retained.
    /// </summary>
    public string? FormDefinitionPath { get; set; }

    public DateTime? RevisedDate { get; set; }
}
