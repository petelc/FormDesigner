namespace FormDesignerAPI.Core.ImportContext.ValueObjects;

public class BatchStatus : IEquatable<BatchStatus>
{
    public string Value { get; }

    private BatchStatus(string value)
    {
        Value = value;
    }

    public static BatchStatus Pending => new("Pending");
    public static BatchStatus Processing => new("Processing");
    public static BatchStatus Completed => new("Completed");
    public static BatchStatus Failed => new("Failed");

    public static BatchStatus FromString(string value)
    {
        return value switch
        {
            "Pending" => Pending,
            "Processing" => Processing,
            "Completed" => Completed,
            "Failed" => Failed,
            _ => throw new ArgumentException($"Invalid batch status: {value}", nameof(value))
        };
    }

    public bool Equals(BatchStatus? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is BatchStatus other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(BatchStatus? left, BatchStatus? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BatchStatus? left, BatchStatus? right)
    {
        return !Equals(left, right);
    }
}