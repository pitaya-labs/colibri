using colibri_api.Common.Persistence;
using colibri_api.Features.Medicos.Domain;
using colibri_api.Features.Tenants.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace colibri_api.Features.Usuarios.Domain;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario", t =>
        {
            t.HasCheckConstraint("ck_usuario_tipo", SnakeCaseEnum.CheckIn<TipoUsuario>("tipo"));
            t.HasCheckConstraint("ck_usuario_estado", SnakeCaseEnum.CheckIn<EstadoUsuario>("estado"));
            // Respaldo declarativo de la regla de dominio que en el DDL era el trigger fn_validar_usuario_medico.
            t.HasCheckConstraint("ck_usuario_medico_id", "(tipo = 'medico') = (medico_id IS NOT NULL)");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.Tipo).HasColumnName("tipo").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.MedicoId).HasColumnName("medico_id");
        builder.Property(x => x.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Correo).HasColumnName("correo").HasMaxLength(320).IsRequired();
        builder.Property(x => x.Estado).HasColumnName("estado").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.MfaHabilitado).HasColumnName("mfa_habilitado").IsRequired();
        builder.Property(x => x.UltimoAcceso).HasColumnName("ultimo_acceso");
        builder.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Medico).WithMany().HasForeignKey(x => x.MedicoId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.Correo }).IsUnique();
    }
}
