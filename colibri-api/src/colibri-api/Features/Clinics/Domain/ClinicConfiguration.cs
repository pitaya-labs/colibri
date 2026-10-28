using colibri_api.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace colibri_api.Features.Clinics.Domain;

public sealed class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.ToTable("clinic", t =>
            t.HasCheckConstraint("ck_clinic_status", SnakeCaseEnum.CheckIn<ClinicStatus>("status")));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Contact).HasColumnName("contact").HasMaxLength(300);
        builder.Property(x => x.TimeZone).HasColumnName("time_zone").HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TenantId);
    }
}
