// Save to: src/FormDesignerAPI.Core/CodeGenerationContext/ValueObjects/GeneratedArtifact.cs

using System.Security.Cryptography;
using System.Text;

namespace FormDesignerAPI.Core.CodeGenerationContext.ValueObjects;

/// <summary>
/// Represents a single generated code file with content and metadata
/// </summary>
public record GeneratedArtifact
{
  public ArtifactType Type { get; init; }
  public string FilePath { get; init; } = string.Empty;
  public string Content { get; init; } = string.Empty;
  public string ContentHash { get; init; } = string.Empty;
  public int SizeBytes { get; init; }
  public DateTime GeneratedAt { get; init; }

  public GeneratedArtifact(ArtifactType type, string filePath, string content)
  {
    Type = type;
    FilePath = filePath;
    Content = content;
    ContentHash = ComputeHash(content);
    SizeBytes = Encoding.UTF8.GetByteCount(content);
    GeneratedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Compute SHA256 hash of content for integrity checking
  /// </summary>
  private static string ComputeHash(string content)
  {
    using var sha256 = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(content);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToHexString(hash);
  }

  /// <summary>
  /// Get file extension from path
  /// </summary>
  public string GetFileExtension()
  {
    return Path.GetExtension(FilePath);
  }

  /// <summary>
  /// Get filename without path
  /// </summary>
  public string GetFileName()
  {
    return Path.GetFileName(FilePath);
  }
}