using colibri_api.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace colibri_api.Features.Usuarios.Domain;

public sealed class MfaCredencialConfiguration : IEntityTypeConfiguration<MfaCredencial>
{
    public void Configure(EntityTypeBuilder<MfaCredencial> builder)
    {
        builder.ToTable("mfa_credencial", t =>
        {
            t.HasCheckConstraint("ck_mfa_credencial_tipo", SnakeCaseEnum.CheckIn<TipoMfa>("tipo"));
            t.HasCheckConstraint("ck_mfa_credencial_estado", SnakeCaseEnum.CheckIn<EstadoMfaCredencial>("estado"));
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(x => x.Tipo).HasColumnName("tipo").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.SecretoCifrado).HasColumnName("secreto_cifrado").IsRequired();
        builder.Property(x => x.Estado).HasColumnName("estado").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UsuarioId);
    }
}
