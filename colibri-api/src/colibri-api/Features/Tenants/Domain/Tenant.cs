namespace colibri_api.Features.Tenants.Domain;

public enum TenantEstado
{
    Activo,
    Suspendido
}

/// <summary>
/// Responsable jurídico y límite de aislamiento de datos (DEC-P3-001): dos médicos independientes
/// nunca comparten tenant.
/// </summary>
public sealed class Tenant
{
    private Tenant() { }

    public Guid Id { get; private set; }
    public string NombreResponsable { get; private set; } = null!;
    public string? Rfc { get; private set; }
    public TenantEstado Estado { get; private set; }
    public DateTimeOffset CreadoEn { get; private set; }

    public static Tenant Crear(string nombreResponsable, string? rfc, DateTimeOffset ahora)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombreResponsable);

        return new Tenant
        {
            Id = Guid.CreateVersion7(),
            NombreResponsable = nombreResponsable.Trim(),
            Rfc = string.IsNullOrWhiteSpace(rfc) ? null : rfc.Trim().ToUpperInvariant(),
            Estado = TenantEstado.Activo,
            CreadoEn = ahora
        };
    }

    public void Suspender()
    {
        if (Estado == TenantEstado.Suspendido)
            throw new InvalidOperationException("El tenant ya está suspendido.");
        Estado = TenantEstado.Suspendido;
    }

    public void Reactivar()
    {
        if (Estado == TenantEstado.Activo)
            throw new InvalidOperationException("El tenant ya está activo.");
        Estado = TenantEstado.Activo;
    }
}
