namespace FormDesignerAPI.Web.ProjectContext;

public class GenerateProjectCodeResponse
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int FormsGenerated { get; set; }
    public GeneratedFilesStructure Files { get; set; } = new();
    public List<string> DownloadUrls { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class GeneratedFilesStructure
{
    public List<GeneratedFile> Frontend { get; set; } = new();
    public List<GeneratedFile> Backend { get; set; } = new();
    public List<GeneratedFile> Sql { get; set; } = new();
    public List<GeneratedFile> Tests { get; set; } = new();
    public List<GeneratedFile> Docs { get; set; } = new();
}

public class GeneratedFile
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty; // e.g., "csharp", "sql", "typescript", "markdown"
}
