using colibri_api.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace colibri_api.Features.Doctors.Domain;

public sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("doctor", t =>
            t.HasCheckConstraint("ck_doctor_verification_status",
                SnakeCaseEnum.CheckIn<DoctorVerificationStatus>("verification_status")));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(300).IsRequired();
        builder.Property(x => x.ProfessionalLicense).HasColumnName("professional_license").HasMaxLength(20).IsRequired();
        builder.Property(x => x.SpecialtyLicense).HasColumnName("specialty_license").HasMaxLength(20);
        builder.Property(x => x.VerificationStatus).HasColumnName("verification_status")
            .HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.ProfessionalLicense }).IsUnique();
    }
}
