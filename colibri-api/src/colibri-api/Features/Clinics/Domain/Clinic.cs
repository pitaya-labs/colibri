using colibri_api.Features.Tenants.Domain;

namespace colibri_api.Features.Clinics.Domain;

public enum ClinicStatus
{
    Active,
    Inactive
}

/// <summary>Physical practice location (consultorio).</summary>
public sealed class Clinic
{
    public const string DefaultTimeZone = "America/Mexico_City";

    private Clinic() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string? Contact { get; private set; }

    /// <summary>IANA time zone name (US-C3-03). Timestamps are stored in UTC; this zone is only used for display.</summary>
    public string TimeZone { get; private set; } = null!;
    public ClinicStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Clinic Create(
        Guid tenantId, string name, string address, string? contact, string? timeZone, DateTimeOffset now)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("A clinic requires a tenant.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);

        var zone = string.IsNullOrWhiteSpace(timeZone) ? DefaultTimeZone : timeZone.Trim();
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(zone, out _))
            throw new ArgumentException($"Unknown time zone: {zone}.", nameof(timeZone));

        return new Clinic
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Name = name.Trim(),
            Address = address.Trim(),
            Contact = string.IsNullOrWhiteSpace(contact) ? null : contact.Trim(),
            TimeZone = zone,
            Status = ClinicStatus.Active,
            CreatedAt = now
        };
    }

    public void Deactivate()
    {
        if (Status == ClinicStatus.Inactive)
            throw new InvalidOperationException("The clinic is already inactive.");
        Status = ClinicStatus.Inactive;
    }

    public void Activate()
    {
        if (Status == ClinicStatus.Active)
            throw new InvalidOperationException("The clinic is already active.");
        Status = ClinicStatus.Active;
    }
}
