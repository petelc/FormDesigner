using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.Examples;

/// <summary>
/// Example demonstrating how form versioning works with the business rule 
/// that only one version can be published at a time.
/// </summary>
public class FormVersioningExample
{
    public void DemonstrateFormVersioning()
    {
        // Create form definitions
        var definition1 = new FormDefinition("/configs/employee-reg-v1.json");
        var definition2 = new FormDefinition("/configs/employee-reg-v2.json");
        var definition3 = new FormDefinition("/configs/employee-reg-v3.json");

        // Create versions
        var version1 = Version.Create(1, 0, 0, definition1);
        var version2 = Version.Create(1, 1, 0, definition2);
        var version3 = Version.Create(2, 0, 0, definition3);

        // Create a new form using the Builder pattern - much cleaner!
        var form = Form.CreateBuilder("FORM-001")
            .WithTitle("Employee Registration Form")
            .WithDivision("HR")
            .WithOwner("John Doe", "john.doe@company.com")
            .WithCreatedDate(DateTime.UtcNow.AddDays(-30))
            .WithVersions(version1, version2, version3)
            .Build();

        Console.WriteLine($"Form has {form.Versions.Count} versions");
        Console.WriteLine($"Published version: {form.GetPublishedVersion()?.ToString() ?? "None"}");

        // Publish version 1.0.0
        form.PublishVersion(version1);
        Console.WriteLine($"After publishing v1.0.0:");
        Console.WriteLine($"  Published version: {form.GetPublishedVersion()?.ToString()}");
        Console.WriteLine($"  Form status: {form.Status}");

        // Try to publish version 2.0.0 - this should archive v1.0.0
        form.PublishVersion(version3);
        Console.WriteLine($"After publishing v2.0.0:");
        Console.WriteLine($"  Published version: {form.GetPublishedVersion()?.ToString()}");
        Console.WriteLine($"  v1.0.0 status: {version1.Status}");
        Console.WriteLine($"  v2.0.0 status: {version3.Status}");

        // Check version statuses
        var draftVersions = form.GetVersionsByStatus(FormStatus.Draft);
        var archivedVersions = form.GetVersionsByStatus(FormStatus.Archived);

        Console.WriteLine($"Draft versions: {string.Join(", ", draftVersions.Select(v => v.ToString()))}");
        Console.WriteLine($"Archived versions: {string.Join(", ", archivedVersions.Select(v => v.ToString()))}");
    }

    public void DemonstrateMinimalFormCreation()
    {
        // Simple form creation - just the essentials
        var simpleForm = Form.CreateBuilder("FORM-002")
            .WithTitle("Simple Contact Form")
            .Build();

        Console.WriteLine($"Created simple form: {simpleForm.FormNumber} - {simpleForm.FormTitle}");
    }

    public void DemonstrateComplexFormCreation()
    {
        // Complex form creation with all properties
        var owner = new Owner("Jane Smith", "jane.smith@company.com");
        var formDef = new FormDefinition("/configs/complex-form.json");
        var initialVersion = Version.Create(1, 0, 0, formDef);

        var complexForm = Form.CreateBuilder("FORM-003")
            .WithTitle("Complex Application Form")
            .WithDivision("Legal")
            .WithOwner(owner)  // Using Owner value object
            .WithCreatedDate(DateTime.UtcNow.AddDays(-10))
            .WithRevisedDate(DateTime.UtcNow.AddDays(-5))
            .WithVersion(initialVersion)
            .Build();

        Console.WriteLine($"Created complex form: {complexForm.FormNumber}");
        Console.WriteLine($"  Division: {complexForm.Division}");
        Console.WriteLine($"  Owner: {complexForm.Owner?.Name}");
        Console.WriteLine($"  Versions: {complexForm.Versions.Count}");
    }
}