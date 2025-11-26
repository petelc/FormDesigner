using FormDesignerAPI.Core.FormAggregate;


namespace FormDesignerAPI.Infrastructure.Data.Config;

public class RevisionConfiguration : IEntityTypeConfiguration<Core.FormAggregate.Revision>
{
    public void Configure(EntityTypeBuilder<Core.FormAggregate.Revision> builder)
    {
        builder.Property(r => r.Major)
            .IsRequired();

        builder.Property(r => r.Minor)
            .IsRequired();

        builder.Property(r => r.Patch)
            .IsRequired();

        builder.Property(r => r.RevisionDate);
        builder.Property(r => r.ReleasedDate);

        builder.Property(r => r.Status)
            .HasConversion(
                x => x.Value,
                x => FormStatus.FromValue(x))
            .HasDefaultValue(FormStatus.NotSet)
            .IsRequired();

        // Each revision has one form definition
        builder.HasOne(r => r.FormDefinition)
            .WithOne()
            .HasForeignKey<Core.FormAggregate.Revision>(r => r.RevisionId)
            .IsRequired();
    }
}
