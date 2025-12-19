using FormDesignerAPI.Core.ProjectContext.Aggregates;
using FormDesignerAPI.Core.ProjectContext.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace FormDesignerAPI.Infrastructure.Data.Config;

/// <summary>
/// Entity Framework configuration for Project aggregate
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
  public void Configure(EntityTypeBuilder<Project> builder)
  {
    builder.ToTable("Projects");

    builder.HasKey(p => p.Id);

    builder.Property(p => p.Name)
      .IsRequired()
      .HasMaxLength(200);

    builder.Property(p => p.Description)
      .HasMaxLength(2000);

    builder.Property(p => p.Status)
      .IsRequired()
      .HasConversion<string>()
      .HasMaxLength(50);

    builder.Property(p => p.CreatedBy)
      .IsRequired()
      .HasMaxLength(256);

    builder.Property(p => p.UpdatedBy)
      .HasMaxLength(256);

    builder.Property(p => p.CreatedAt)
      .IsRequired();

    builder.Property(p => p.UpdatedAt)
      .IsRequired();

    // Configure ProjectFilter as owned entity WITHOUT ToJson
    builder.OwnsOne(p => p.Filter, filter =>
    {
      // Map each property to its own column with prefix
      filter.Property(f => f.FormType)
        .HasColumnName("Filter_FormType")
        .HasMaxLength(100);

      filter.Property(f => f.ActiveOnly)
        .HasColumnName("Filter_ActiveOnly");

      filter.Property(f => f.FromDate)
        .HasColumnName("Filter_FromDate");

      filter.Property(f => f.ToDate)
        .HasColumnName("Filter_ToDate");

      // Store Tags as JSON string
      filter.Property(f => f.Tags)
        .HasColumnName("Filter_Tags")
        .HasConversion(
          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
        )
        .HasColumnType("nvarchar(max)");

      // Store CustomFilters as JSON string
      filter.Property(f => f.CustomFilters)
        .HasColumnName("Filter_CustomFilters")
        .HasConversion(
          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
          v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
        )
        .HasColumnType("nvarchar(max)");
    });

    // Configure FormIds as JSON column
    builder.Property<List<Guid>>("_formIds")
      .HasColumnName("FormIds")
      .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>()
      )
      .HasColumnType("nvarchar(max)");

    // Configure CodeGenerationJobIds as JSON column
    builder.Property<List<Guid>>("_codeGenerationJobIds")
      .HasColumnName("CodeGenerationJobIds")
      .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>()
      )
      .HasColumnType("nvarchar(max)");

    // Indexes
    builder.HasIndex(p => p.Name);
    builder.HasIndex(p => p.Status);
    builder.HasIndex(p => p.CreatedBy);
    builder.HasIndex(p => p.CreatedAt);
    builder.HasIndex(p => p.UpdatedAt);
  }
}
