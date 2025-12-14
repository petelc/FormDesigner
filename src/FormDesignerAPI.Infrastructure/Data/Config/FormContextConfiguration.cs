using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.Infrastructure.Data.Config;

/// <summary>
/// EF Core configuration for FormContext.Form aggregate
/// </summary>
public class FormContextConfiguration : IEntityTypeConfiguration<Form>
{
    public void Configure(EntityTypeBuilder<Form> builder)
    {
        builder.ToTable("Forms");

        // Primary key - Guid stored as TEXT in SQLite
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasConversion(
                id => id.ToString(),
                id => Guid.Parse(id))
            .IsRequired();

        // Name
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH);

        // FormDefinition - stored as JSON
        builder.OwnsOne(f => f.Definition, definition =>
        {
            definition.Property(d => d.Schema)
                .HasColumnName("DefinitionSchema")
                .HasColumnType("TEXT")
                .IsRequired();

            // Fields are serialized as part of the Schema JSON
            definition.Ignore(d => d.Fields);
        });

        // OriginMetadata - owned entity
        builder.OwnsOne(f => f.Origin, origin =>
        {
            origin.Property(o => o.Type)
                .HasColumnName("Origin_Type")
                .HasConversion<int>()
                .IsRequired();

            origin.Property(o => o.CreatedBy)
                .HasColumnName("Origin_CreatedBy")
                .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH)
                .IsRequired();

            origin.Property(o => o.CreatedAt)
                .HasColumnName("Origin_CreatedAt")
                .IsRequired();

            origin.Property(o => o.ReferenceId)
                .HasColumnName("Origin_ReferenceId")
                .HasMaxLength(500);
        });

        // IsActive
        builder.Property(f => f.IsActive)
            .IsRequired();

        // Computed properties (not stored)
        builder.Ignore(f => f.CurrentRevision);
        builder.Ignore(f => f.CurrentVersion);
        builder.Ignore(f => f.FieldCount);

        // Revisions - ignore for now (will be handled separately)
        builder.Ignore(f => f.Revisions);

        // Indexes
        builder.HasIndex(f => f.Name);
        builder.HasIndex(f => f.IsActive);
    }
}
