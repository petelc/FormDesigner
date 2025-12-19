using FormDesignerAPI.Core.CodeGenerationContext.Aggregates;
using FormDesignerAPI.Core.CodeGenerationContext.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FormDesignerAPI.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for CodeGenerationJob aggregate
/// </summary>
public class CodeGenerationJobConfiguration : IEntityTypeConfiguration<CodeGenerationJob>
{
    public void Configure(EntityTypeBuilder<CodeGenerationJob> builder)
    {
        builder.ToTable("CodeGenerationJobs");

        // Primary key - SQL Server uses uniqueidentifier natively
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id)
            .IsRequired();

        // Foreign keys - SQL Server uses uniqueidentifier natively
        builder.Property(j => j.FormDefinitionId)
            .IsRequired();

        builder.Property(j => j.FormRevisionId)
            .IsRequired();

        // Status enum
        builder.Property(j => j.Status)
            .HasConversion<int>()
            .IsRequired();

        // GenerationVersion - owned entity
        builder.OwnsOne(j => j.Version, version =>
        {
            version.Property(v => v.Major).HasColumnName("Version_Major").IsRequired();
            version.Property(v => v.Minor).HasColumnName("Version_Minor").IsRequired();
            version.Property(v => v.Patch).HasColumnName("Version_Patch").IsRequired();
        });

        // GenerationOptions - owned entity with JSON serialization for complex properties
        builder.OwnsOne(j => j.Options, options =>
        {
            options.Property(o => o.IncludeCSharpModels).HasColumnName("Options_IncludeCSharpModels");
            options.Property(o => o.IncludeSqlSchema).HasColumnName("Options_IncludeSqlSchema");
            options.Property(o => o.IncludeReactComponents).HasColumnName("Options_IncludeReactComponents");
            options.Property(o => o.IncludeTests).HasColumnName("Options_IncludeTests");
            options.Property(o => o.GenerateIntegrationTests).HasColumnName("Options_GenerateIntegrationTests");

            options.Property(o => o.Namespace).HasColumnName("Options_Namespace");
            options.Property(o => o.ProjectName).HasColumnName("Options_ProjectName");
            options.Property(o => o.Author).HasColumnName("Options_Author");

            options.Property(o => o.TestFramework).HasColumnName("Options_TestFramework");
            options.Property(o => o.UseFluentAssertions).HasColumnName("Options_UseFluentAssertions");
            options.Property(o => o.DatabaseType).HasColumnName("Options_DatabaseType");

            // Serialize Dictionaries as JSON - use nvarchar(max) for SQL Server
            options.Property(o => o.AdditionalImports)
                .HasColumnName("Options_AdditionalImports")
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null!) ?? new Dictionary<string, string>());

            options.Property(o => o.CustomSettings)
                .HasColumnName("Options_CustomSettings")
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!) ?? new Dictionary<string, object>());
        });

        // Simple properties
        builder.Property(j => j.OutputFolderPath).HasMaxLength(500);
        builder.Property(j => j.ZipFilePath).HasMaxLength(500);
        builder.Property(j => j.ZipFileSizeBytes);
        builder.Property(j => j.ArtifactCount).IsRequired();

        builder.Property(j => j.RequestedAt).IsRequired();
        builder.Property(j => j.RequestedBy).IsRequired().HasMaxLength(256);
        builder.Property(j => j.CompletedAt);
        builder.Property(j => j.ProcessingDuration);
        builder.Property(j => j.ErrorMessage).HasMaxLength(2000);

        // Artifacts collection - ignore for now (would need separate table)
        builder.Ignore(j => j.Artifacts);
    }
}
