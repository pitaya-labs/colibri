using colibri_api.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace colibri_api.Features.Assignments.Domain;

public sealed class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("assignment", t =>
        {
            t.HasCheckConstraint("ck_assignment_role", SnakeCaseEnum.CheckIn<AssignmentRole>("role"));
            t.HasCheckConstraint("ck_assignment_status", SnakeCaseEnum.CheckIn<AssignmentStatus>("status"));
            t.HasCheckConstraint("ck_assignment_validity", "valid_to IS NULL OR valid_to >= valid_from");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.ClinicId).HasColumnName("clinic_id").IsRequired();
        builder.Property(x => x.Role).HasColumnName("role").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.ValidFrom).HasColumnName("valid_from").IsRequired();
        builder.Property(x => x.ValidTo).HasColumnName("valid_to");
        builder.Property(x => x.Status).HasColumnName("status").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Clinic).WithMany().HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.ClinicId, x.ValidFrom }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.ClinicId })
            .HasDatabaseName("idx_assignment_active_user")
            .HasFilter("status = 'active'");
    }
}
