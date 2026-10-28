using colibri_api.Features.Medicos.Domain;
using colibri_api.Features.Tenants.Domain;

namespace colibri_api.Features.Usuarios.Domain;

public enum TipoUsuario
{
    Medico,
    Administrativo,
    AdministradorSistema,
    Soporte
}

public enum EstadoUsuario
{
    Activo,
    Suspendido,
    Baja
}

public sealed class Usuario
{
    private Usuario() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;
    public TipoUsuario Tipo { get; private set; }
    public Guid? MedicoId { get; private set; }
    public Medico? Medico { get; private set; }
    public string NombreCompleto { get; private set; } = null!;
    public string Correo { get; private set; } = null!;
    public EstadoUsuario Estado { get; private set; }
    public bool MfaHabilitado { get; private set; }
    public DateTimeOffset? UltimoAcceso { get; private set; }
    public DateTimeOffset CreadoEn { get; private set; }

    /// <summary>
    /// Un usuario tipo médico debe tener <paramref name="medicoId"/>; ningún otro tipo puede tenerlo.
    /// </summary>
    public static Usuario Crear(
        Guid tenantId, TipoUsuario tipo, Guid? medicoId, string nombreCompleto, string correo,
        DateTimeOffset ahora)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("El usuario requiere un tenant.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(nombreCompleto);
        ArgumentException.ThrowIfNullOrWhiteSpace(correo);

        if (tipo == TipoUsuario.Medico && medicoId is null)
            throw new InvalidOperationException("Un usuario tipo médico requiere medicoId.");
        if (tipo != TipoUsuario.Medico && medicoId is not null)
            throw new InvalidOperationException("Solo un usuario tipo médico puede tener medicoId.");

        return new Usuario
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Tipo = tipo,
            MedicoId = medicoId,
            NombreCompleto = nombreCompleto.Trim(),
            Correo = correo.Trim().ToLowerInvariant(),
            Estado = EstadoUsuario.Activo,
            MfaHabilitado = false,
            CreadoEn = ahora
        };
    }

    public void Suspender()
    {
        if (Estado != EstadoUsuario.Activo)
            throw new InvalidOperationException("Solo un usuario activo puede suspenderse.");
        Estado = EstadoUsuario.Suspendido;
    }

    public void Reactivar()
    {
        if (Estado != EstadoUsuario.Suspendido)
            throw new InvalidOperationException("Solo un usuario suspendido puede reactivarse.");
        Estado = EstadoUsuario.Activo;
    }

    /// <summary>La baja es terminal: no existe transición de salida.</summary>
    public void DarDeBaja()
    {
        if (Estado == EstadoUsuario.Baja)
            throw new InvalidOperationException("El usuario ya está dado de baja.");
        Estado = EstadoUsuario.Baja;
    }

    public void HabilitarMfa() => MfaHabilitado = true;

    public void RegistrarAcceso(DateTimeOffset ahora)
    {
        if (Estado != EstadoUsuario.Activo)
            throw new InvalidOperationException("Un usuario no activo no puede acceder.");
        UltimoAcceso = ahora;
    }
}
