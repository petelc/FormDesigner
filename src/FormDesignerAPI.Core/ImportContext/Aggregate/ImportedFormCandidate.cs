using System;
using FormDesignerAPI.Core.ImportContext.ValueObjects;

namespace FormDesignerAPI.Core.ImportContext.Aggregate;

public class ImportedFormCandidate : EntityBase<Guid>
{
    public Guid CandidateId { get; set; }
    public Guid BatchId { get; set; }
    public string? originalFileName { get; set; }
    public string? extractedJson { get; set; }
    ExtractionStatus extractionStatus { get; set; } = ExtractionStatus.Pending;
    ApprovalStatus approvalStatus { get; set; } = ApprovalStatus.Pending;
    List<string> validationErrors { get; set; } = new();

    public void Approve(string approvedBy, string extractedJson)
    {
        this.approvalStatus = ApprovalStatus.Approved;
        this.extractedJson = extractedJson;
        // You might want to log approvedBy and the timestamp here as well.
    }

    public void Reject(string approvedBy, List<string> errors)
    {
        this.approvalStatus = ApprovalStatus.Rejected;
        this.validationErrors = errors;
        // You might want to log approvedBy and the timestamp here as well.
    }
}
