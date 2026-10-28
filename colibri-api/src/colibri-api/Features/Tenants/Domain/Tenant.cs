namespace colibri_api.Features.Tenants.Domain;

public enum TenantStatus
{
    Active,
    Suspended
}

/// <summary>
/// Legal entity responsible for the data and the data isolation boundary (DEC-P3-001):
/// two independent doctors never share a tenant.
/// </summary>
public sealed class Tenant
{
    private Tenant() { }

    public Guid Id { get; private set; }
    public string ResponsibleName { get; private set; } = null!;
    public string? Rfc { get; private set; }
    public TenantStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Tenant Create(string responsibleName, string? rfc, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(responsibleName);

        return new Tenant
        {
            Id = Guid.CreateVersion7(),
            ResponsibleName = responsibleName.Trim(),
            Rfc = string.IsNullOrWhiteSpace(rfc) ? null : rfc.Trim().ToUpperInvariant(),
            Status = TenantStatus.Active,
            CreatedAt = now
        };
    }

    public void Suspend()
    {
        if (Status == TenantStatus.Suspended)
            throw new InvalidOperationException("The tenant is already suspended.");
        Status = TenantStatus.Suspended;
    }

    public void Reactivate()
    {
        if (Status == TenantStatus.Active)
            throw new InvalidOperationException("The tenant is already active.");
        Status = TenantStatus.Active;
    }
}
