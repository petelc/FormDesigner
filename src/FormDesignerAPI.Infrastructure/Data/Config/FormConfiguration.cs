using FormDesignerAPI.Core.FormContext.Aggregates;

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

        builder.Property(f => f.Revisions);

        builder.Property(f => f.CreatedDate);

        builder.Property(f => f.RevisedDate);

        builder.Property(f => f.Status)
            .HasConversion(
                x => x.Value,
                x => FormStatus.FromValue(x));
    }
}
