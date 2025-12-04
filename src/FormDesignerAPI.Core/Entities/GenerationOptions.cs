namespace FormDesignerAPI.Core.Entities;

public class GenerationOptions
{
    public bool IncludeCSharpModels { get; set; } = true;
    public bool IncludeSqlSchema { get; set; } = true;
    public bool IncludeReactComponents { get; set; } = true;

    public string Namespace { get; set; } = "YourApp";
    public string ProjectName { get; set; } = "YourProject";
    // public string Author { get; set; } // This needs to be set as the AppUser's name

    public Dictionary<string, string> AdditionalImports { get; set; } = new();

    // Customization options
    public bool UseEntityFramework { get; set; } = true;
    public bool UseStoredProcedures { get; set; } = true;
    public bool UseSoftDelete { get; set; } = true;
    public bool IncludeAuditFields { get; set; } = true;

    // React-specific options
    public string ReactStyleFramework { get; set; } = "Bootstrap"; // or "MaterialUI", "TailwindCSS"
    public bool UseTypeScript { get; set; } = true;
    public bool UseReactHooks { get; set; } = true;
}
