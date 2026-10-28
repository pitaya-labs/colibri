using colibri_api.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace colibri_api.Features.Medicos.Domain;

public sealed class MedicoConfiguration : IEntityTypeConfiguration<Medico>
{
    public void Configure(EntityTypeBuilder<Medico> builder)
    {
        builder.ToTable("medico", t =>
            t.HasCheckConstraint("ck_medico_estado_verificacion",
                SnakeCaseEnum.CheckIn<EstadoVerificacionMedico>("estado_verificacion")));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(300).IsRequired();
        builder.Property(x => x.CedulaProfesional).HasColumnName("cedula_profesional").HasMaxLength(20).IsRequired();
        builder.Property(x => x.CedulaEspecialidad).HasColumnName("cedula_especialidad").HasMaxLength(20);
        builder.Property(x => x.EstadoVerificacion).HasColumnName("estado_verificacion")
            .HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.CedulaProfesional }).IsUnique();
    }
}
