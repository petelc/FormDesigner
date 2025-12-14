namespace FormDesignerAPI.UseCases.Forms;

/// <summary>
/// Data Transfer Object for Form
/// </summary>
public class FormDTO
{
    public int Id { get; set; }
    public string FormNumber { get; set; } = string.Empty;
    public string FormTitle { get; set; } = string.Empty;
    public string? Division { get; set; }
    public string? Owner { get; set; }
    public string? Version { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime RevisedDate { get; set; }
    public string? ConfigurationPath { get; set; }
}
