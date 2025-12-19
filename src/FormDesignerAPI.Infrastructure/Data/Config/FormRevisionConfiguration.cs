using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for FormRevision entity
/// </summary>
public class FormRevisionConfiguration : IEntityTypeConfiguration<FormRevision>
{
    public void Configure(EntityTypeBuilder<FormRevision> builder)
    {
        builder.ToTable("FormRevisions");

        // Primary key - SQL Server uses uniqueidentifier natively
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .IsRequired();

        // FormId - foreign key using SQL Server uniqueidentifier
        builder.Property(r => r.FormId)
            .IsRequired();

        // Version number
        builder.Property(r => r.Version)
            .IsRequired();

        // FormDefinition - store as JSON and recreate with Fields when loading
        builder.Property(r => r.Definition)
            .HasConversion(
                // To database: serialize the Schema property
                definition => definition.Schema,
                // From database: parse schema and populate Fields
                schema => FormDefinition.From(schema))
            .HasColumnName("DefinitionSchema")
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        // Notes
        builder.Property(r => r.Notes)
            .HasMaxLength(1000);

        // CreatedBy
        builder.Property(r => r.CreatedBy)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH);

        // CreatedAt
        builder.Property(r => r.CreatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(r => r.FormId);
        builder.HasIndex(r => r.Version);
        builder.HasIndex(r => new { r.FormId, r.Version }).IsUnique();
    }
}
