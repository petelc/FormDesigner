using FormDesignerAPI.Core.ContributorAggregate;
using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.Infrastructure.Data;

public static class SeedData
{
  public static readonly Contributor Contributor1 = new Contributor("Ardalis");
  public static readonly Contributor Contributor2 = new Contributor("Snowfrog");

  public static readonly Form Form1 = Form.CreateBuilder("FD-001")
    .WithTitle("Form Designer Initial Form")
    .WithDivision("Engineering")
    .WithOwner("John Doe", "john.doe@example.com")
    .WithCreatedDate(DateTime.UtcNow)
    .WithRevisedDate(DateTime.UtcNow)
    .Build();

  public static readonly Form Form2 = Form.CreateBuilder("FD-002")
    .WithTitle("Form Designer Second Form")
    .WithDivision("Marketing")
    .WithOwner("Jane Smith", "jane.smith@example.com")
    .WithCreatedDate(DateTime.UtcNow)
    .WithRevisedDate(DateTime.UtcNow)
    .Build();

  public static async Task InitializeAsync(AppDbContext dbContext)
  {
    if (await dbContext.Contributors.AnyAsync()) return; // DB has been seeded
    if (await dbContext.Forms.AnyAsync()) return; // DB has been seeded

    await PopulateTestDataAsync(dbContext);
  }

  public static async Task PopulateTestDataAsync(AppDbContext dbContext)
  {
    dbContext.Contributors.AddRange(new[] { Contributor1, Contributor2 });
    dbContext.Forms.AddRange(new[] { Form1, Form2 });
    await dbContext.SaveChangesAsync();
  }
}

