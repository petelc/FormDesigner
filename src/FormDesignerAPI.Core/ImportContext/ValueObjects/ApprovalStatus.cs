namespace FormDesignerAPI.Core.ImportContext.ValueObjects;

public sealed class ApprovalStatus : IEquatable<ApprovalStatus>
{
    public string Value { get; }

    private ApprovalStatus(string value)
    {
        Value = value;
    }

    public static ApprovalStatus Pending => new("Pending");
    public static ApprovalStatus Approved => new("Approved");
    public static ApprovalStatus Rejected => new("Rejected");

    public static ApprovalStatus Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Approval status cannot be null or empty.", nameof(value));

        return value.ToLowerInvariant() switch
        {
            "pending" => Pending,
            "approved" => Approved,
            "rejected" => Rejected,
            _ => throw new ArgumentException($"Invalid approval status: {value}", nameof(value))
        };
    }

    public bool Equals(ApprovalStatus? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => obj is ApprovalStatus other && Equals(other);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(ApprovalStatus? left, ApprovalStatus? right) => Equals(left, right);

    public static bool operator !=(ApprovalStatus? left, ApprovalStatus? right) => !Equals(left, right);
}