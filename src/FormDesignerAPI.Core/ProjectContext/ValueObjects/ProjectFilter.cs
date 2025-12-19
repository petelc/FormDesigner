namespace FormDesignerAPI.Core.ProjectContext.ValueObjects;

/// <summary>
/// Project filter for selecting forms and generation options
/// </summary>
public record ProjectFilter
{
  /// <summary>
  /// Filter by form type (optional)
  /// </summary>
  public string? FormType { get; init; }

  /// <summary>
  /// Filter by tags (optional)
  /// </summary>
  public List<string> Tags { get; init; } = new();

  /// <summary>
  /// Include only active forms
  /// </summary>
  public bool ActiveOnly { get; init; } = true;

  /// <summary>
  /// Date range filter (optional)
  /// </summary>
  public DateTime? FromDate { get; init; }
  public DateTime? ToDate { get; init; }

  /// <summary>
  /// Custom filter criteria
  /// </summary>
  public Dictionary<string, object> CustomFilters { get; init; } = new();

  /// <summary>
  /// Create default filter
  /// </summary>
  public static ProjectFilter Default()
  {
    return new ProjectFilter
    {
      ActiveOnly = true
    };
  }

  /// <summary>
  /// Create filter by date range
  /// </summary>
  public static ProjectFilter ByDateRange(DateTime from, DateTime to)
  {
    return new ProjectFilter
    {
      FromDate = from,
      ToDate = to,
      ActiveOnly = true
    };
  }

  /// <summary>
  /// Create filter by tags
  /// </summary>
  public static ProjectFilter ByTags(params string[] tags)
  {
    return new ProjectFilter
    {
      Tags = tags.ToList(),
      ActiveOnly = true
    };
  }
}
