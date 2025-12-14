using FormDesignerAPI.Core.FormContext.Aggregates;

namespace FormDesignerAPI.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for FormRevision entity
/// </summary>
public class FormRevisionConfiguration : IEntityTypeConfiguration<FormRevision>
{
    public void Configure(EntityTypeBuilder<FormRevision> builder)
    {
        builder.ToTable("FormRevisions");

        // Primary key - Guid stored as TEXT in SQLite
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(
                id => id.ToString(),
                id => Guid.Parse(id))
            .IsRequired();

        // FormId - foreign key stored as TEXT
        builder.Property<Guid>("FormId")
            .HasConversion(
                id => id.ToString(),
                id => Guid.Parse(id))
            .IsRequired();

        // Version number
        builder.Property(r => r.Version)
            .IsRequired();

        // FormDefinition - stored as JSON
        builder.OwnsOne(r => r.Definition, definition =>
        {
            definition.Property(d => d.Schema)
                .HasColumnName("DefinitionSchema")
                .HasColumnType("TEXT")
                .IsRequired();

            // Fields are serialized as part of the Schema JSON
            definition.Ignore(d => d.Fields);
        });

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
        builder.HasIndex("FormId");
        builder.HasIndex(r => r.Version);
        builder.HasIndex("FormId", "Version").IsUnique();
    }
}
