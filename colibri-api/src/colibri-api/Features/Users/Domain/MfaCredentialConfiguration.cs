using colibri_api.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace colibri_api.Features.Users.Domain;

public sealed class MfaCredentialConfiguration : IEntityTypeConfiguration<MfaCredential>
{
    public void Configure(EntityTypeBuilder<MfaCredential> builder)
    {
        builder.ToTable("mfa_credential", t =>
        {
            t.HasCheckConstraint("ck_mfa_credential_type", SnakeCaseEnum.CheckIn<MfaType>("type"));
            t.HasCheckConstraint("ck_mfa_credential_status", SnakeCaseEnum.CheckIn<MfaCredentialStatus>("status"));
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.EncryptedSecret).HasColumnName("encrypted_secret").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId);
    }
}
