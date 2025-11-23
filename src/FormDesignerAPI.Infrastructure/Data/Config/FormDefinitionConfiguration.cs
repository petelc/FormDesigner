using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.Infrastructure.Data.Config;

public class FormDefinitionConfiguration : IEntityTypeConfiguration<FormDefinition>
{
    public void Configure(EntityTypeBuilder<FormDefinition> builder)
    {
        builder.Property(fd => fd.ConfigurationPath)
            .IsRequired()
            .HasMaxLength(DataSchemaConstants.DEFAULT_DESCRIPTION_LENGTH);

    }
}
