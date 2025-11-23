using FormDesignerAPI.Core.ContributorAggregate;
using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.Infrastructure.Data;

public static class SeedData
{
  public static readonly Contributor Contributor1 = new Contributor("Ardalis");
  public static readonly Contributor Contributor2 = new Contributor("Snowfrog");

  private static Form? _form1;
  private static Form? _form2;

  public static Form Form1
  {
    get
    {
      if (_form1 == null)
      {
        var formDef1 = new FormDefinition("/forms/fd-001/form-definition.json");
        var version1 = FormDesignerAPI.Core.FormAggregate.Version.Create(1, 1, 1, formDef1);
        var formDef3 = new FormDefinition("/forms/fd-001/form-definition-v2.json");
        var version2 = FormDesignerAPI.Core.FormAggregate.Version.Create(2, 1, 1, formDef3);

        _form1 = Form.CreateBuilder("FD-001")
          .WithTitle("Form Designer Initial Form")
          .WithDivision("Engineering")
          .WithOwner("John Doe", "john.doe@example.com")
          .WithCreatedDate(DateTime.UtcNow.AddDays(-30))
          .WithRevisedDate(DateTime.UtcNow.AddDays(-30))
          .WithVersions(version1, version2)
          .Build();
      }
      return _form1;
    }
  }

  public static Form Form2
  {
    get
    {
      if (_form2 == null)
      {
        var formDef2 = new FormDefinition("/forms/fd-002/form-definition.json");
        var version1 = FormDesignerAPI.Core.FormAggregate.Version.Create(1, 1, 1, formDef2);

        _form2 = Form.CreateBuilder("FD-002")
          .WithTitle("Form Designer Second Form")
          .WithDivision("Marketing")
          .WithOwner("Jane Smith", "jane.smith@example.com")
          .WithCreatedDate(DateTime.UtcNow.AddDays(-15))
          .WithRevisedDate(DateTime.UtcNow.AddDays(-15))
          .WithVersion(version1)
          .Build();
      }
      return _form2;
    }
  }

  public static async Task InitializeAsync(AppDbContext dbContext)
  {
    if (await dbContext.Contributors.AnyAsync()) return; // DB has been seeded
    if (await dbContext.Forms.AnyAsync()) return; // DB has been seeded

    await PopulateTestDataAsync(dbContext);
  }

  public static async Task PopulateTestDataAsync(AppDbContext dbContext)
  {
    // Add contributors
    dbContext.Contributors.AddRange(new[] { Contributor1, Contributor2 });

    // Add forms with their versions using the cached static properties
    dbContext.Forms.AddRange(new[] { Form1, Form2 });

    // Save contributors and forms with their versions first
    await dbContext.SaveChangesAsync();
  }
}

