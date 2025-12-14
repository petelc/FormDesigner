// Save to: src/FormDesignerAPI.Core/CodeGenerationContext/ValueObjects/GenerationVersion.cs

using System.Text.RegularExpressions;

namespace FormDesignerAPI.Core.CodeGenerationContext.ValueObjects;

/// <summary>
/// Semantic version for code generation
/// Format: Major.Minor.Patch (e.g., 1.0.0)
/// </summary>
public record GenerationVersion
{
  public int Major { get; init; }
  public int Minor { get; init; }
  public int Patch { get; init; }

  public GenerationVersion(int major, int minor, int patch)
  {
    Major = major;
    Minor = minor;
    Patch = patch;
  }

  /// <summary>
  /// Parse version string (e.g., "1.0.0")
  /// </summary>
  public static GenerationVersion Parse(string version)
  {
    var match = Regex.Match(version, @"^(\d+)\.(\d+)\.(\d+)$");
    if (!match.Success)
      throw new ArgumentException($"Invalid version format: {version}");

    return new GenerationVersion(
      int.Parse(match.Groups[1].Value),
      int.Parse(match.Groups[2].Value),
      int.Parse(match.Groups[3].Value)
    );
  }

  /// <summary>
  /// Increment patch version
  /// </summary>
  public GenerationVersion Increment()
  {
    return new GenerationVersion(Major, Minor, Patch + 1);
  }

  /// <summary>
  /// Increment minor version (resets patch to 0)
  /// </summary>
  public GenerationVersion IncrementMinor()
  {
    return new GenerationVersion(Major, Minor + 1, 0);
  }

  /// <summary>
  /// Increment major version (resets minor and patch to 0)
  /// </summary>
  public GenerationVersion IncrementMajor()
  {
    return new GenerationVersion(Major + 1, 0, 0);
  }

  public override string ToString() => $"{Major}.{Minor}.{Patch}";
}