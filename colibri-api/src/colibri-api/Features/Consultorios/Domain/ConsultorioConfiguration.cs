using colibri_api.Common.Persistence;
using colibri_api.Features.Tenants.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace colibri_api.Features.Consultorios.Domain;

public sealed class ConsultorioConfiguration : IEntityTypeConfiguration<Consultorio>
{
    public void Configure(EntityTypeBuilder<Consultorio> builder)
    {
        builder.ToTable("consultorio", t =>
            t.HasCheckConstraint("ck_consultorio_estado", SnakeCaseEnum.CheckIn<EstadoConsultorio>("estado")));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Domicilio).HasColumnName("domicilio").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Contacto).HasColumnName("contacto").HasMaxLength(300);
        builder.Property(x => x.ZonaHoraria).HasColumnName("zona_horaria").HasMaxLength(64).IsRequired();
        builder.Property(x => x.Estado).HasColumnName("estado").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TenantId);
    }
}
