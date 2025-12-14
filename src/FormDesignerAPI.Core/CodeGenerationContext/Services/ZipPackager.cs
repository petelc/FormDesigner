// Save to: src/FormDesignerAPI.Core/CodeGenerationContext/Services/ZipPackager.cs

using System.IO.Compression;

namespace FormDesignerAPI.Core.CodeGenerationContext.Services;

/// <summary>
/// Creates ZIP archives of generated code
/// </summary>
public class ZipPackager
{
  /// <summary>
  /// Create ZIP file from a folder
  /// </summary>
  public async Task<string> CreateZipFile(string folderPath, string outputPath)
  {
    if (!Directory.Exists(folderPath))
    {
      throw new DirectoryNotFoundException($"Folder not found: {folderPath}");
    }

    var zipFileName = $"{Path.GetFileName(folderPath)}_{DateTime.UtcNow:yyyyMMddHHmmss}.zip";
    var zipFilePath = Path.Combine(outputPath, zipFileName);

    // Ensure output directory exists
    Directory.CreateDirectory(outputPath);

    // Delete existing file if present
    if (File.Exists(zipFilePath))
    {
      File.Delete(zipFilePath);
    }

    // Create zip file
    await Task.Run(() =>
    {
      ZipFile.CreateFromDirectory(
        folderPath,
        zipFilePath,
        CompressionLevel.Optimal,
        includeBaseDirectory: false);
    });

    return zipFilePath;
  }

  /// <summary>
  /// Create ZIP archive in memory (returns byte array)
  /// </summary>
  public async Task<byte[]> CreateZipArchive(string folderPath)
  {
    if (!Directory.Exists(folderPath))
    {
      throw new DirectoryNotFoundException($"Folder not found: {folderPath}");
    }

    using var memoryStream = new MemoryStream();
    
    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
    {
      await AddDirectoryToZip(archive, folderPath, "");
    }

    return memoryStream.ToArray();
  }

  /// <summary>
  /// Create ZIP from specific files
  /// </summary>
  public async Task<string> CreateZipFromFiles(
    Dictionary<string, string> files, // Key = filename, Value = content
    string outputPath,
    string zipFileName)
  {
    Directory.CreateDirectory(outputPath);
    
    var zipFilePath = Path.Combine(outputPath, zipFileName);
    
    if (File.Exists(zipFilePath))
    {
      File.Delete(zipFilePath);
    }

    using var fileStream = new FileStream(zipFilePath, FileMode.Create);
    using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);

    foreach (var file in files)
    {
      var entry = archive.CreateEntry(file.Key, CompressionLevel.Optimal);
      
      using var entryStream = entry.Open();
      using var writer = new StreamWriter(entryStream);
      await writer.WriteAsync(file.Value);
    }

    return zipFilePath;
  }

  /// <summary>
  /// Recursively add directory contents to ZIP archive
  /// </summary>
  private async Task AddDirectoryToZip(
    ZipArchive archive,
    string sourceDir,
    string entryPrefix)
  {
    var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);

    foreach (var file in files)
    {
      var relativePath = Path.GetRelativePath(sourceDir, file);
      var entryName = string.IsNullOrEmpty(entryPrefix)
        ? relativePath
        : Path.Combine(entryPrefix, relativePath);

      // Normalize path separators for zip (always use forward slash)
      entryName = entryName.Replace(Path.DirectorySeparatorChar, '/');

      var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);

      using var entryStream = entry.Open();
      using var fileStream = File.OpenRead(file);
      await fileStream.CopyToAsync(entryStream);
    }
  }

  /// <summary>
  /// Extract ZIP file to a directory
  /// </summary>
  public async Task ExtractZipFile(string zipFilePath, string destinationPath)
  {
    if (!File.Exists(zipFilePath))
    {
      throw new FileNotFoundException($"ZIP file not found: {zipFilePath}");
    }

    Directory.CreateDirectory(destinationPath);

    await Task.Run(() =>
    {
      ZipFile.ExtractToDirectory(zipFilePath, destinationPath, overwriteFiles: true);
    });
  }

  /// <summary>
  /// Get list of files in a ZIP archive
  /// </summary>
  public List<string> GetZipContents(string zipFilePath)
  {
    if (!File.Exists(zipFilePath))
    {
      throw new FileNotFoundException($"ZIP file not found: {zipFilePath}");
    }

    using var archive = ZipFile.OpenRead(zipFilePath);
    return archive.Entries.Select(e => e.FullName).ToList();
  }

  /// <summary>
  /// Get size of ZIP file
  /// </summary>
  public long GetZipFileSize(string zipFilePath)
  {
    if (!File.Exists(zipFilePath))
    {
      throw new FileNotFoundException($"ZIP file not found: {zipFilePath}");
    }

    return new FileInfo(zipFilePath).Length;
  }
}