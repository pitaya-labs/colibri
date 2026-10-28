using colibri_api.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace colibri_api.Features.Adscripciones.Domain;

public sealed class AdscripcionConfiguration : IEntityTypeConfiguration<Adscripcion>
{
    public void Configure(EntityTypeBuilder<Adscripcion> builder)
    {
        builder.ToTable("adscripcion", t =>
        {
            t.HasCheckConstraint("ck_adscripcion_rol", SnakeCaseEnum.CheckIn<RolAdscripcion>("rol"));
            t.HasCheckConstraint("ck_adscripcion_estado", SnakeCaseEnum.CheckIn<EstadoAdscripcion>("estado"));
            t.HasCheckConstraint("ck_adscripcion_vigencia",
                "vigencia_hasta IS NULL OR vigencia_hasta >= vigencia_desde");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(x => x.ConsultorioId).HasColumnName("consultorio_id").IsRequired();
        builder.Property(x => x.Rol).HasColumnName("rol").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.VigenciaDesde).HasColumnName("vigencia_desde").IsRequired();
        builder.Property(x => x.VigenciaHasta).HasColumnName("vigencia_hasta");
        builder.Property(x => x.Estado).HasColumnName("estado").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();

        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Consultorio).WithMany().HasForeignKey(x => x.ConsultorioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UsuarioId, x.ConsultorioId, x.VigenciaDesde }).IsUnique();
        builder.HasIndex(x => new { x.UsuarioId, x.ConsultorioId })
            .HasDatabaseName("idx_adscripcion_usuario_activa")
            .HasFilter("estado = 'activa'");
    }
}
