using colibri_api.Features.Tenants.Domain;

namespace colibri_api.Features.Medicos.Domain;

public enum EstadoVerificacionMedico
{
    Pendiente,
    Verificado,
    Rechazado
}

public sealed class Medico
{
    private Medico() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;
    public string NombreCompleto { get; private set; } = null!;
    public string CedulaProfesional { get; private set; } = null!;
    public string? CedulaEspecialidad { get; private set; }
    public EstadoVerificacionMedico EstadoVerificacion { get; private set; }
    public DateTimeOffset CreadoEn { get; private set; }

    public static Medico Crear(
        Guid tenantId, string nombreCompleto, string cedulaProfesional, string? cedulaEspecialidad,
        DateTimeOffset ahora)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("El médico requiere un tenant.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(nombreCompleto);
        ArgumentException.ThrowIfNullOrWhiteSpace(cedulaProfesional);

        return new Medico
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            NombreCompleto = nombreCompleto.Trim(),
            CedulaProfesional = cedulaProfesional.Trim(),
            CedulaEspecialidad = string.IsNullOrWhiteSpace(cedulaEspecialidad) ? null : cedulaEspecialidad.Trim(),
            EstadoVerificacion = EstadoVerificacionMedico.Pendiente,
            CreadoEn = ahora
        };
    }

    public void Verificar()
    {
        if (EstadoVerificacion != EstadoVerificacionMedico.Pendiente)
            throw new InvalidOperationException("Solo un médico pendiente puede verificarse.");
        EstadoVerificacion = EstadoVerificacionMedico.Verificado;
    }

    public void Rechazar()
    {
        if (EstadoVerificacion != EstadoVerificacionMedico.Pendiente)
            throw new InvalidOperationException("Solo un médico pendiente puede rechazarse.");
        EstadoVerificacion = EstadoVerificacionMedico.Rechazado;
    }
}
