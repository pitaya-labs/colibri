using colibri_api.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace colibri_api.Features.Usuarios.Domain;

public sealed class SesionConfiguration : IEntityTypeConfiguration<Sesion>
{
    public void Configure(EntityTypeBuilder<Sesion> builder)
    {
        builder.ToTable("sesion", t =>
            t.HasCheckConstraint("ck_sesion_estado", SnakeCaseEnum.CheckIn<EstadoSesion>("estado")));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(x => x.Estado).HasColumnName("estado").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
        builder.Property(x => x.ExpiraEn).HasColumnName("expira_en").IsRequired();
        builder.Property(x => x.OrigenIp).HasColumnName("origen_ip").HasMaxLength(45);
        builder.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(512);

        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UsuarioId);
    }
}
