using colibri_api.Features.Consultorios.Domain;
using colibri_api.Features.Tenants.Domain;
using colibri_api.Features.Usuarios.Domain;

namespace colibri_api.Features.Adscripciones.Domain;

public enum RolAdscripcion
{
    Medico,
    Administrativo
}

public enum EstadoAdscripcion
{
    Activa,
    Inactiva
}

/// <summary>
/// Vincula a un usuario (médico o administrativo) con un consultorio durante un periodo de vigencia.
/// Es lo que define el alcance del Administrativo, que opera sobre varios médicos.
/// </summary>
public sealed class Adscripcion
{
    private Adscripcion() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public Guid ConsultorioId { get; private set; }
    public Consultorio Consultorio { get; private set; } = null!;
    public RolAdscripcion Rol { get; private set; }
    public DateOnly VigenciaDesde { get; private set; }
    public DateOnly? VigenciaHasta { get; private set; }
    public EstadoAdscripcion Estado { get; private set; }
    public DateTimeOffset CreadaEn { get; private set; }

    /// <summary>El rol se deriva del tipo de usuario: solo médicos y administrativos pueden adscribirse.</summary>
    public static Adscripcion Crear(
        Guid tenantId, Usuario usuario, Consultorio consultorio, DateOnly vigenciaDesde, DateOnly? vigenciaHasta,
        DateTimeOffset ahora)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("La adscripción requiere un tenant.", nameof(tenantId));
        ArgumentNullException.ThrowIfNull(usuario);
        ArgumentNullException.ThrowIfNull(consultorio);

        if (usuario.Estado != EstadoUsuario.Activo)
            throw new InvalidOperationException("Solo un usuario activo puede adscribirse a un consultorio.");
        if (vigenciaHasta is not null && vigenciaHasta < vigenciaDesde)
            throw new ArgumentException("La vigencia no puede terminar antes de empezar.", nameof(vigenciaHasta));

        var rol = usuario.Tipo switch
        {
            TipoUsuario.Medico => RolAdscripcion.Medico,
            TipoUsuario.Administrativo => RolAdscripcion.Administrativo,
            _ => throw new InvalidOperationException(
                "Solo un usuario médico o administrativo puede adscribirse a un consultorio.")
        };

        return new Adscripcion
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            UsuarioId = usuario.Id,
            ConsultorioId = consultorio.Id,
            Rol = rol,
            VigenciaDesde = vigenciaDesde,
            VigenciaHasta = vigenciaHasta,
            Estado = EstadoAdscripcion.Activa,
            CreadaEn = ahora
        };
    }

    public void Desactivar(DateOnly hasta)
    {
        if (Estado == EstadoAdscripcion.Inactiva)
            throw new InvalidOperationException("La adscripción ya está inactiva.");
        if (hasta < VigenciaDesde)
            throw new ArgumentException("La vigencia no puede terminar antes de empezar.", nameof(hasta));

        VigenciaHasta = hasta;
        Estado = EstadoAdscripcion.Inactiva;
    }
}
