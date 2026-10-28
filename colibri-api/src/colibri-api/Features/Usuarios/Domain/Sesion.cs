namespace colibri_api.Features.Usuarios.Domain;

public enum EstadoSesion
{
    Activa,
    Revocada,
    Expirada
}

/// <summary>
/// Sesión persistida como fila (no solo JWT) para poder revocarla de inmediato (US-C1-01).
/// </summary>
public sealed class Sesion
{
    private Sesion() { }

    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public EstadoSesion Estado { get; private set; }
    public DateTimeOffset CreadaEn { get; private set; }
    public DateTimeOffset ExpiraEn { get; private set; }

    /// <summary>
    /// IP de origen como texto. El diseño la define como <c>inet</c>; se guarda como cadena para no
    /// atar el modelo a un proveedor (los tests corren sobre SQLite).
    /// </summary>
    public string? OrigenIp { get; private set; }
    public string? UserAgent { get; private set; }

    public static Sesion Crear(
        Guid usuarioId, DateTimeOffset ahora, DateTimeOffset expiraEn, string? origenIp, string? userAgent)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("La sesión requiere un usuario.", nameof(usuarioId));
        if (expiraEn <= ahora)
            throw new ArgumentException("La sesión debe expirar después de su creación.", nameof(expiraEn));

        return new Sesion
        {
            Id = Guid.CreateVersion7(),
            UsuarioId = usuarioId,
            Estado = EstadoSesion.Activa,
            CreadaEn = ahora,
            ExpiraEn = expiraEn,
            OrigenIp = origenIp,
            UserAgent = userAgent
        };
    }

    public bool EstaVigente(DateTimeOffset ahora) => Estado == EstadoSesion.Activa && ahora < ExpiraEn;

    public void Revocar()
    {
        if (Estado != EstadoSesion.Activa)
            throw new InvalidOperationException("Solo una sesión activa puede revocarse.");
        Estado = EstadoSesion.Revocada;
    }

    public void Expirar(DateTimeOffset ahora)
    {
        if (Estado != EstadoSesion.Activa)
            throw new InvalidOperationException("Solo una sesión activa puede expirar.");
        if (ahora < ExpiraEn)
            throw new InvalidOperationException("La sesión aún no llega a su fecha de expiración.");
        Estado = EstadoSesion.Expirada;
    }
}
