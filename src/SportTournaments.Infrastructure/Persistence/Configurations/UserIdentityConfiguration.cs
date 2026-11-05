using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportTournaments.Domain.Common;

public sealed class UserIdentityConfiguration: IEntityTypeConfiguration<UserIdentity>
{
    public void Configure(EntityTypeBuilder<UserIdentity> builder)
    {
        builder.ToTable("user_identities");

        builder.HasKey(identity => identity.Id);

        builder.Property(identity => identity.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(identity => identity.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(identity => identity.Issuer).HasColumnName("issuer").HasMaxLength(500).IsRequired();
        builder.Property(identity => identity.Subject).HasColumnName("subject").HasMaxLength(255).IsRequired();

        builder.HasOne<User>().WithMany().HasForeignKey(identity => identity.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(identity => new
        {
            identity.Issuer,
            identity.Subject
        }).IsUnique();
    }
}