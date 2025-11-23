using FormDesignerAPI.Core.FormAggregate;


namespace FormDesignerAPI.Infrastructure.Data.Config;

public class VersionConfiguration : IEntityTypeConfiguration<Core.FormAggregate.Version>
{
    public void Configure(EntityTypeBuilder<Core.FormAggregate.Version> builder)
    {
        builder.Property(v => v.Major)
            .IsRequired();

        builder.Property(v => v.Minor)
            .IsRequired();

        builder.Property(v => v.Patch)
            .IsRequired();

        builder.Property(v => v.VersionDate);
        builder.Property(v => v.ReleasedDate);

        builder.Property(v => v.Status)
            .HasConversion(
                x => x.Value,
                x => FormStatus.FromValue(x))
            .HasDefaultValue(FormStatus.NotSet)
            .IsRequired();

        // Each version has one form definition
        builder.HasOne(v => v.FormDefinition)
            .WithOne()
            .HasForeignKey<Core.FormAggregate.Version>(v => v.VersionId)
            .IsRequired();
    }
}