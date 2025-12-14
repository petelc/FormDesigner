// Save to: src/FormDesignerAPI.Core/CodeGenerationContext/Services/CodeArtifactOrganizer.cs

using FormDesignerAPI.Core.CodeGenerationContext.ValueObjects;
using System.Text;

namespace FormDesignerAPI.Core.CodeGenerationContext.Services;

/// <summary>
/// Organizes generated artifacts into a folder structure
/// </summary>
public class CodeArtifactOrganizer
{
  private readonly string _baseOutputPath;

  public CodeArtifactOrganizer(string baseOutputPath)
  {
    _baseOutputPath = baseOutputPath;
  }

  /// <summary>
  /// Organize artifacts into folder structure and write to disk
  /// </summary>
  public async Task<OrganizedArtifacts> OrganizeArtifacts(
    List<GeneratedArtifact> artifacts,
    string projectName)
  {
    var rootPath = Path.Combine(_baseOutputPath, SanitizeFolderName(projectName));
    
    // Define folder structure for each artifact type
    var folders = GetFolderMapping(rootPath);

    // Create all directories
    foreach (var folder in folders.Values.Distinct())
    {
      Directory.CreateDirectory(folder);
    }

    var organizedFiles = new List<OrganizedFile>();

    // Write artifacts to their respective folders
    foreach (var artifact in artifacts)
    {
      if (!folders.TryGetValue(artifact.Type, out var targetFolder))
      {
        // Skip unknown artifact types
        continue;
      }

      var fileName = Path.GetFileName(artifact.FilePath);
      var fullPath = Path.Combine(targetFolder, fileName);

      await File.WriteAllTextAsync(fullPath, artifact.Content);

      organizedFiles.Add(new OrganizedFile(
        artifact.Type,
        fullPath,
        fileName,
        artifact.SizeBytes
      ));
    }

    // Create README
    await CreateReadmeFile(rootPath, organizedFiles, projectName);

    // Create .gitignore
    await CreateGitIgnoreFile(rootPath);

    return new OrganizedArtifacts(rootPath, organizedFiles);
  }

  /// <summary>
  /// Get folder mapping for each artifact type
  /// </summary>
  private Dictionary<ArtifactType, string> GetFolderMapping(string rootPath)
  {
    return new Dictionary<ArtifactType, string>
    {
      // C# Files - Organized by Clean Architecture layers
      { 
        ArtifactType.CSharpEntity, 
        Path.Combine(rootPath, "CSharp", "Domain", "Entities") 
      },
      { 
        ArtifactType.CSharpInterface, 
        Path.Combine(rootPath, "CSharp", "Domain", "Interfaces") 
      },
      { 
        ArtifactType.CSharpRepository, 
        Path.Combine(rootPath, "CSharp", "Infrastructure", "Repositories") 
      },
      { 
        ArtifactType.CSharpController, 
        Path.Combine(rootPath, "CSharp", "Web", "Controllers") 
      },
      { 
        ArtifactType.CSharpDto, 
        Path.Combine(rootPath, "CSharp", "Application", "DTOs") 
      },
      { 
        ArtifactType.CSharpAutoMapper, 
        Path.Combine(rootPath, "CSharp", "Application", "Mappings") 
      },
      { 
        ArtifactType.CSharpValidation, 
        Path.Combine(rootPath, "CSharp", "Application", "Validators") 
      },
      { 
        ArtifactType.CSharpUnitTests, 
        Path.Combine(rootPath, "Tests", "UnitTests") 
      },
      { 
        ArtifactType.CSharpIntegrationTests, 
        Path.Combine(rootPath, "Tests", "IntegrationTests") 
      },
      
      // SQL Files
      { 
        ArtifactType.SqlCreateTable, 
        Path.Combine(rootPath, "SQL", "Tables") 
      },
      { 
        ArtifactType.SqlStoredProcedures, 
        Path.Combine(rootPath, "SQL", "StoredProcedures") 
      },
      
      // React Files
      { 
        ArtifactType.ReactComponent, 
        Path.Combine(rootPath, "React", "Components") 
      },
      { 
        ArtifactType.ReactValidation, 
        Path.Combine(rootPath, "React", "Validation") 
      },
      
      // CI/CD Files
      { 
        ArtifactType.GitHubActions, 
        Path.Combine(rootPath, ".github", "workflows") 
      },
      { 
        ArtifactType.AzurePipeline, 
        Path.Combine(rootPath, "Pipelines") 
      },
      { 
        ArtifactType.Dockerfile, 
        rootPath // Dockerfile goes in root
      }
    };
  }

  /// <summary>
  /// Create README file with generation summary
  /// </summary>
  private async Task CreateReadmeFile(
    string rootPath,
    List<OrganizedFile> files,
    string projectName)
  {
    var readme = new StringBuilder();
    
    readme.AppendLine($"# Generated Code for {projectName}");
    readme.AppendLine();
    readme.AppendLine($"**Generated on:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
    readme.AppendLine($"**Total Files:** {files.Count}");
    readme.AppendLine($"**Total Size:** {FormatBytes(files.Sum(f => f.SizeBytes))}");
    readme.AppendLine();
    
    readme.AppendLine("## 📁 File Structure");
    readme.AppendLine();
    readme.AppendLine("```");
    readme.AppendLine($"{projectName}/");
    readme.AppendLine("├── CSharp/");
    readme.AppendLine("│   ├── Domain/          (Entities, Interfaces)");
    readme.AppendLine("│   ├── Application/     (DTOs, Validators)");
    readme.AppendLine("│   ├── Infrastructure/  (Repositories)");
    readme.AppendLine("│   └── Web/            (Controllers)");
    readme.AppendLine("├── SQL/");
    readme.AppendLine("│   ├── Tables/");
    readme.AppendLine("│   └── StoredProcedures/");
    readme.AppendLine("├── React/");
    readme.AppendLine("│   ├── Components/");
    readme.AppendLine("│   └── Validation/");
    readme.AppendLine("└── Tests/");
    readme.AppendLine("```");
    readme.AppendLine();
    
    readme.AppendLine("## 📄 Files Generated");
    readme.AppendLine();

    // Group files by category
    var grouped = files
      .GroupBy(f => GetCategory(f.Type))
      .OrderBy(g => g.Key);

    foreach (var group in grouped)
    {
      readme.AppendLine($"### {group.Key}");
      readme.AppendLine();
      
      foreach (var file in group.OrderBy(f => f.FileName))
      {
        readme.AppendLine($"- **{file.FileName}** ({FormatBytes(file.SizeBytes)})");
      }
      
      readme.AppendLine();
    }

    readme.AppendLine("## 🚀 Next Steps");
    readme.AppendLine();
    readme.AppendLine("1. Review the generated code");
    readme.AppendLine("2. Adjust namespaces if needed");
    readme.AppendLine("3. Add to your solution");
    readme.AppendLine("4. Run and test");
    readme.AppendLine();
    
    readme.AppendLine("## ⚠️ Important Notes");
    readme.AppendLine();
    readme.AppendLine("- This code was generated from a form definition");
    readme.AppendLine("- Review for your specific requirements");
    readme.AppendLine("- Add error handling as needed");
    readme.AppendLine("- Customize validation rules");
    readme.AppendLine("- Add authentication/authorization");
    readme.AppendLine();

    var readmePath = Path.Combine(rootPath, "README.md");
    await File.WriteAllTextAsync(readmePath, readme.ToString());
  }

  /// <summary>
  /// Create .gitignore file
  /// </summary>
  private async Task CreateGitIgnoreFile(string rootPath)
  {
    var gitignore = @"# Build results
bin/
obj/
*.user
*.suo

# IDE
.vs/
.vscode/
.idea/

# Node modules (for React)
node_modules/
";

    var gitignorePath = Path.Combine(rootPath, ".gitignore");
    await File.WriteAllTextAsync(gitignorePath, gitignore);
  }

  /// <summary>
  /// Get category name for artifact type
  /// </summary>
  private string GetCategory(ArtifactType type)
  {
    return type switch
    {
      ArtifactType.CSharpEntity or
      ArtifactType.CSharpInterface or
      ArtifactType.CSharpRepository or
      ArtifactType.CSharpController or
      ArtifactType.CSharpDto or
      ArtifactType.CSharpAutoMapper or
      ArtifactType.CSharpValidation => "C# Files",
      
      ArtifactType.CSharpUnitTests or
      ArtifactType.CSharpIntegrationTests => "Test Files",
      
      ArtifactType.SqlCreateTable or
      ArtifactType.SqlStoredProcedures => "SQL Scripts",
      
      ArtifactType.ReactComponent or
      ArtifactType.ReactValidation => "React Files",
      
      ArtifactType.GitHubActions or
      ArtifactType.AzurePipeline or
      ArtifactType.Dockerfile => "CI/CD Files",
      
      _ => "Other Files"
    };
  }

  /// <summary>
  /// Sanitize folder name (remove invalid characters)
  /// </summary>
  private string SanitizeFolderName(string name)
  {
    var invalid = Path.GetInvalidFileNameChars();
    return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
  }

  /// <summary>
  /// Format bytes to human-readable string
  /// </summary>
  private string FormatBytes(long bytes)
  {
    string[] sizes = { "B", "KB", "MB", "GB" };
    double len = bytes;
    int order = 0;
    
    while (len >= 1024 && order < sizes.Length - 1)
    {
      order++;
      len = len / 1024;
    }

    return $"{len:0.##} {sizes[order]}";
  }
}

/// <summary>
/// Result of organizing artifacts
/// </summary>
public record OrganizedArtifacts(
  string RootPath,
  List<OrganizedFile> Files
);

/// <summary>
/// Information about an organized file
/// </summary>
public record OrganizedFile(
  ArtifactType Type,
  string FullPath,
  string FileName,
  int SizeBytes
);