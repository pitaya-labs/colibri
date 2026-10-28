namespace colibri_api.Features.Usuarios.Domain;

public enum TipoMfa
{
    Totp
}

public enum EstadoMfaCredencial
{
    Activo,
    Revocado
}

/// <summary>
/// Credencial MFA separada de <see cref="Usuario"/>; el secreto nunca se guarda en texto plano
/// ni se registra en logs (US-C1-01).
/// </summary>
public sealed class MfaCredencial
{
    private MfaCredencial() { }

    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public TipoMfa Tipo { get; private set; }
    public byte[] SecretoCifrado { get; private set; } = null!;
    public EstadoMfaCredencial Estado { get; private set; }
    public DateTimeOffset CreadoEn { get; private set; }

    /// <param name="secretoCifrado">Secreto ya cifrado por la capa de aplicación antes de llegar aquí.</param>
    public static MfaCredencial Crear(Guid usuarioId, byte[] secretoCifrado, DateTimeOffset ahora)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("La credencial requiere un usuario.", nameof(usuarioId));
        ArgumentNullException.ThrowIfNull(secretoCifrado);
        if (secretoCifrado.Length == 0)
            throw new ArgumentException("El secreto cifrado no puede estar vacío.", nameof(secretoCifrado));

        return new MfaCredencial
        {
            Id = Guid.CreateVersion7(),
            UsuarioId = usuarioId,
            Tipo = TipoMfa.Totp,
            SecretoCifrado = secretoCifrado,
            Estado = EstadoMfaCredencial.Activo,
            CreadoEn = ahora
        };
    }

    public void Revocar()
    {
        if (Estado == EstadoMfaCredencial.Revocado)
            throw new InvalidOperationException("La credencial MFA ya está revocada.");
        Estado = EstadoMfaCredencial.Revocado;
    }
}
