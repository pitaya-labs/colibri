using colibri_api.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace colibri_api.Features.Tenants.Domain;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenant", t =>
            t.HasCheckConstraint("ck_tenant_status", SnakeCaseEnum.CheckIn<TenantStatus>("status")));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.ResponsibleName).HasColumnName("responsible_name").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Rfc).HasColumnName("rfc").HasMaxLength(13);
        builder.Property(x => x.Status).HasColumnName("status").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
    }
}
