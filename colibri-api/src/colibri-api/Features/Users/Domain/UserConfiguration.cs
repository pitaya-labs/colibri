using colibri_api.Common.Persistence;
using colibri_api.Features.Doctors.Domain;
using colibri_api.Features.Tenants.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace colibri_api.Features.Users.Domain;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // "user" is a reserved word in PostgreSQL, hence app_user.
        builder.ToTable("app_user", t =>
        {
            t.HasCheckConstraint("ck_app_user_type", SnakeCaseEnum.CheckIn<UserType>("type"));
            t.HasCheckConstraint("ck_app_user_status", SnakeCaseEnum.CheckIn<UserStatus>("status"));
            // Declarative backup of the domain rule that the original DDL enforced with a trigger.
            t.HasCheckConstraint("ck_app_user_doctor_id", "(type = 'doctor') = (doctor_id IS NOT NULL)");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.DoctorId).HasColumnName("doctor_id");
        builder.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.MfaEnabled).HasColumnName("mfa_enabled").IsRequired();
        builder.Property(x => x.LastAccessAt).HasColumnName("last_access_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
    }
}
