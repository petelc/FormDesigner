namespace FormDesignerAPI.Core.ImportContext.ValueObjects;

public class ExtractionStatus : IEquatable<ExtractionStatus>
{
    public static readonly ExtractionStatus Pending = new("Pending");
    public static readonly ExtractionStatus InProgress = new("InProgress");
    public static readonly ExtractionStatus Completed = new("Completed");
    public static readonly ExtractionStatus Failed = new("Failed");

    public string Value { get; }

    private ExtractionStatus(string value)
    {
        Value = value;
    }

    public static ExtractionStatus FromString(string value)
    {
        return value switch
        {
            "Pending" => Pending,
            "InProgress" => InProgress,
            "Completed" => Completed,
            "Failed" => Failed,
            _ => throw new ArgumentException($"Invalid extraction status: {value}", nameof(value))
        };
    }

    public bool Equals(ExtractionStatus? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is ExtractionStatus other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(ExtractionStatus? left, ExtractionStatus? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ExtractionStatus? left, ExtractionStatus? right)
    {
        return !Equals(left, right);
    }
}