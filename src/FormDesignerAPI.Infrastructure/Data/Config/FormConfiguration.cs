using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.Infrastructure.Data.Config;

public class FormConfiguration : IEntityTypeConfiguration<Form>
{
    public void Configure(EntityTypeBuilder<Form> builder)
    {
        builder.Property(f => f.FormNumber)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DEFAULT_FORM_NUMBER_LENGTH);

        builder.Property(f => f.FormTitle)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH);

        builder.Property(f => f.Division)
            .HasMaxLength(DataSchemaConstants.DEFAULT_DIVISION_LENGTH);

        // Owner
        builder.OwnsOne(builder => builder.Owner, o =>
        {
            o.Property(p => p.Name)
                .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH)
                .IsRequired();

            o.Property(p => p.Email)
                .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH)
                .IsRequired();
        });

        // Each form has one current revision (without loading the navigation - use FK only)
        // builder.HasOne(f => f.CurrentRevision)
        //     .WithOne()
        //     .HasForeignKey<Core.FormAggregate.Form>(f => f.CurrentRevisionId)
        //     .IsRequired();

        // builder.Property(f => f.GetCurrentRevision()!)
        //     .HasMaxLength(DataSchemaConstants.DEFAULT_FORM_NUMBER_LENGTH);

        // each form can have many previous revisions
        builder.HasMany(f => f.Revisions)
            .WithOne()
            .HasForeignKey(r => r.FormId)
            .IsRequired();

        builder.Property(f => f.CreatedDate);

        builder.Property(f => f.RevisedDate);

        // Shadow property for storing revision string from database
        builder.Property<string>("Revision")
            .HasMaxLength(DataSchemaConstants.DEFAULT_FORM_NUMBER_LENGTH)
            .IsRequired(false);

        builder.Property(f => f.Status)
            .HasConversion(
                x => x.Value,
                x => FormStatus.FromValue(x))
            .HasDefaultValue(FormStatus.NotSet)
            .IsRequired();
    }
}
