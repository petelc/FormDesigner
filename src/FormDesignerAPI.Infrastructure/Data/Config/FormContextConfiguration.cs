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

        // Primary key - SQL Server uses uniqueidentifier natively
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .IsRequired();

        // Name
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH);

        // FormDefinition - store as JSON and recreate with Fields when loading
        builder.Property(f => f.Definition)
            .HasConversion(
                // To database: serialize the Schema property
                definition => definition.Schema,
                // From database: parse schema and populate Fields
                schema => FormDefinition.From(schema))
            .HasColumnName("DefinitionSchema")
            .HasColumnType("nvarchar(max)")
            .IsRequired();

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

        // Configure the one-to-many relationship with FormRevisions
        builder.HasMany(f => f.Revisions)
            .WithOne()
            .HasForeignKey(r => r.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(f => f.Name);
        builder.HasIndex(f => f.IsActive);
    }
}
