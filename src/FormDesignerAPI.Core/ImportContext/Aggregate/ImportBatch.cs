using FormDesignerAPI.Core.ImportContext.ValueObjects;

namespace FormDesignerAPI.Core.ImportContext.Aggregate;

public class ImportBatch : EntityBase<Guid>, IAggregateRoot
{
    public Guid BatchId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    List<string> uploadedFiles { get; set; } = new();
    BatchStatus batchStatus { get; set; } = BatchStatus.Pending;
    List<ImportedFormCandidate> candidates { get; set; } = new();

    public void AddCandidate(ImportedFormCandidate candidate)
    {
        candidates.Add(candidate);
    }

    public ImportBatch Create()
    {
        this.BatchId = Guid.NewGuid();
        this.CreatedAt = DateTime.UtcNow;
        this.batchStatus = BatchStatus.Pending;
        return this;
    }

    public void Process()
    {
        this.batchStatus = BatchStatus.Processing;
    }
}
