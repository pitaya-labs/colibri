using colibri_api.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace colibri_api.Features.Tenants.Domain;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenant", t =>
            t.HasCheckConstraint("ck_tenant_estado", SnakeCaseEnum.CheckIn<TenantEstado>("estado")));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.NombreResponsable).HasColumnName("nombre_responsable").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Rfc).HasColumnName("rfc").HasMaxLength(13);
        builder.Property(x => x.Estado).HasColumnName("estado").HasSnakeCaseConversion().IsRequired();
        builder.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
    }
}
