namespace colibri_api.Features.Users.Domain;

public enum SessionStatus
{
    Active,
    Revoked,
    Expired
}

/// <summary>
/// Session persisted as a row (not just a JWT) so it can be revoked immediately (US-C1-01).
/// </summary>
public sealed class Session
{
    private Session() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public SessionStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>
    /// Origin IP as text. The design defines it as <c>inet</c>; it is kept as a string so the model
    /// is not tied to one provider (tests run on SQLite).
    /// </summary>
    public string? OriginIp { get; private set; }
    public string? UserAgent { get; private set; }

    public static Session Create(
        Guid userId, DateTimeOffset now, DateTimeOffset expiresAt, string? originIp, string? userAgent)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("A session requires a user.", nameof(userId));
        if (expiresAt <= now)
            throw new ArgumentException("A session must expire after it is created.", nameof(expiresAt));

        return new Session
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Status = SessionStatus.Active,
            CreatedAt = now,
            ExpiresAt = expiresAt,
            OriginIp = originIp,
            UserAgent = userAgent
        };
    }

    public bool IsActive(DateTimeOffset now) => Status == SessionStatus.Active && now < ExpiresAt;

    public void Revoke()
    {
        if (Status != SessionStatus.Active)
            throw new InvalidOperationException("Only an active session can be revoked.");
        Status = SessionStatus.Revoked;
    }

    public void Expire(DateTimeOffset now)
    {
        if (Status != SessionStatus.Active)
            throw new InvalidOperationException("Only an active session can expire.");
        if (now < ExpiresAt)
            throw new InvalidOperationException("The session has not reached its expiration time yet.");
        Status = SessionStatus.Expired;
    }
}
