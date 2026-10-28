using colibri_api.Features.Tenants.Domain;

namespace colibri_api.Features.Consultorios.Domain;

public enum EstadoConsultorio
{
    Activo,
    Inactivo
}

public sealed class Consultorio
{
    public const string ZonaHorariaPorDefecto = "America/Mexico_City";

    private Consultorio() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public string Domicilio { get; private set; } = null!;
    public string? Contacto { get; private set; }

    /// <summary>Nombre de zona IANA (US-C3-03). Los timestamps se guardan en UTC; esta zona solo se usa para presentar.</summary>
    public string ZonaHoraria { get; private set; } = null!;
    public EstadoConsultorio Estado { get; private set; }
    public DateTimeOffset CreadoEn { get; private set; }

    public static Consultorio Crear(
        Guid tenantId, string nombre, string domicilio, string? contacto, string? zonaHoraria,
        DateTimeOffset ahora)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("El consultorio requiere un tenant.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(domicilio);

        var zona = string.IsNullOrWhiteSpace(zonaHoraria) ? ZonaHorariaPorDefecto : zonaHoraria.Trim();
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(zona, out _))
            throw new ArgumentException($"Zona horaria desconocida: {zona}.", nameof(zonaHoraria));

        return new Consultorio
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Nombre = nombre.Trim(),
            Domicilio = domicilio.Trim(),
            Contacto = string.IsNullOrWhiteSpace(contacto) ? null : contacto.Trim(),
            ZonaHoraria = zona,
            Estado = EstadoConsultorio.Activo,
            CreadoEn = ahora
        };
    }

    public void Desactivar()
    {
        if (Estado == EstadoConsultorio.Inactivo)
            throw new InvalidOperationException("El consultorio ya está inactivo.");
        Estado = EstadoConsultorio.Inactivo;
    }

    public void Activar()
    {
        if (Estado == EstadoConsultorio.Activo)
            throw new InvalidOperationException("El consultorio ya está activo.");
        Estado = EstadoConsultorio.Activo;
    }
}
