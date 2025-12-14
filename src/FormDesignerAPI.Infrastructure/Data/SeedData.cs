using FormDesignerAPI.Core.FormContext.Aggregates;

namespace FormDesignerAPI.Infrastructure.Data;

public static class SeedData
{


  // public static readonly Form Form1 = new Form("FD-001", "Form Designer Initial Form")
  //   .UpdateDivision("Engineering")
  //   .UpdateVersion("1.0")
  //   .SetOwner("John Doe", "john.doe@example.com");

  // public static readonly Form Form2 = new Form("FD-002", "Form Designer Second Form")
  //   .UpdateDivision("Marketing")
  //   .UpdateVersion("1.0")
  //   .SetOwner("Jane Smith", "jane.smith@example.com");

  public static async Task InitializeAsync(AppDbContext dbContext)
  {
    if (await dbContext.Forms.AnyAsync()) return; // DB has been seeded

    await PopulateTestDataAsync(dbContext);
  }

  public static async Task PopulateTestDataAsync(AppDbContext dbContext)
  {
    // dbContext.Forms.AddRange(new[] { Form1, Form2 });
    await dbContext.SaveChangesAsync();
  }
}
