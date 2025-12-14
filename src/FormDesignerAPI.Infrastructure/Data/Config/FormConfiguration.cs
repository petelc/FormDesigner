using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.Infrastructure.Data.Config;

public class FormConfiguration : IEntityTypeConfiguration<Form>
{
    public void Configure(EntityTypeBuilder<Form> builder)
    {
        builder.ToTable("Forms");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FormNumber)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DEFAULT_FORM_NUMBER_LENGTH);

        builder.Property(f => f.FormTitle)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH);

        builder.Property(f => f.Division)
            .HasMaxLength(DataSchemaConstants.DEFAULT_DIVISION_LENGTH);

        // Owner as owned entity
        builder.OwnsOne(f => f.Owner, o =>
        {
            o.Property(p => p.Name)
                .HasColumnName("Owner_Name")
                .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH)
                .IsRequired();

            o.Property(p => p.Email)
                .HasColumnName("Owner_Email")
                .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH)
                .IsRequired();
        });

        builder.Property(f => f.Version)
            .HasMaxLength(50);

        builder.Property(f => f.CreatedDate)
            .IsRequired();

        builder.Property(f => f.RevisedDate)
            .IsRequired();

        builder.Property(f => f.ConfigurationPath)
            .HasMaxLength(500);

        builder.Property(f => f.Status)
            .IsRequired()
            .HasConversion<int>();
    }
}
