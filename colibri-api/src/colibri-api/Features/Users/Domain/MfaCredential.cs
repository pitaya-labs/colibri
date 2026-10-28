namespace colibri_api.Features.Users.Domain;

public enum MfaType
{
    Totp
}

public enum MfaCredentialStatus
{
    Active,
    Revoked
}

/// <summary>
/// MFA credential kept apart from <see cref="User"/>; the secret is never stored in plain text
/// nor written to logs (US-C1-01).
/// </summary>
public sealed class MfaCredential
{
    private MfaCredential() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public MfaType Type { get; private set; }
    public byte[] EncryptedSecret { get; private set; } = null!;
    public MfaCredentialStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    /// <param name="encryptedSecret">Secret already encrypted by the application layer before reaching this point.</param>
    public static MfaCredential Create(Guid userId, byte[] encryptedSecret, DateTimeOffset now)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("A credential requires a user.", nameof(userId));
        ArgumentNullException.ThrowIfNull(encryptedSecret);
        if (encryptedSecret.Length == 0)
            throw new ArgumentException("The encrypted secret cannot be empty.", nameof(encryptedSecret));

        return new MfaCredential
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Type = MfaType.Totp,
            EncryptedSecret = encryptedSecret,
            Status = MfaCredentialStatus.Active,
            CreatedAt = now
        };
    }

    public void Revoke()
    {
        if (Status == MfaCredentialStatus.Revoked)
            throw new InvalidOperationException("The MFA credential is already revoked.");
        Status = MfaCredentialStatus.Revoked;
    }
}
