namespace FormDesignerAPI.Core.ImportContext.Events;

public class FormCandidateApprovedEvent
{
    public Guid CandidateId { get; set; }
    public Guid BatchId { get; set; }
    public string? extractedJson { get; set; }
    public string ApprovedBy { get; set; } = string.Empty; // This needs to set to the identity user id.
}