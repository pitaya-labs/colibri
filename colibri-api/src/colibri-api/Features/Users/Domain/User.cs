using colibri_api.Features.Doctors.Domain;
using colibri_api.Features.Tenants.Domain;

namespace colibri_api.Features.Users.Domain;

public enum UserType
{
    Doctor,
    Administrative,
    SystemAdmin,
    Support
}

public enum UserStatus
{
    Active,
    Suspended,
    Terminated
}

public sealed class User
{
    private User() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;
    public UserType Type { get; private set; }
    public Guid? DoctorId { get; private set; }
    public Doctor? Doctor { get; private set; }
    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public UserStatus Status { get; private set; }
    public bool MfaEnabled { get; private set; }
    public DateTimeOffset? LastAccessAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// A doctor-type user must have <paramref name="doctorId"/>; no other type may have it.
    /// </summary>
    public static User Create(
        Guid tenantId, UserType type, Guid? doctorId, string fullName, string email, DateTimeOffset now)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("A user requires a tenant.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        if (type == UserType.Doctor && doctorId is null)
            throw new InvalidOperationException("A doctor user requires a doctorId.");
        if (type != UserType.Doctor && doctorId is not null)
            throw new InvalidOperationException("Only a doctor user can have a doctorId.");

        return new User
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Type = type,
            DoctorId = doctorId,
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Status = UserStatus.Active,
            MfaEnabled = false,
            CreatedAt = now
        };
    }

    public void Suspend()
    {
        if (Status != UserStatus.Active)
            throw new InvalidOperationException("Only an active user can be suspended.");
        Status = UserStatus.Suspended;
    }

    public void Reactivate()
    {
        if (Status != UserStatus.Suspended)
            throw new InvalidOperationException("Only a suspended user can be reactivated.");
        Status = UserStatus.Active;
    }

    /// <summary>Termination is final: there is no transition out of it.</summary>
    public void Terminate()
    {
        if (Status == UserStatus.Terminated)
            throw new InvalidOperationException("The user is already terminated.");
        Status = UserStatus.Terminated;
    }

    public void EnableMfa() => MfaEnabled = true;

    public void RecordAccess(DateTimeOffset now)
    {
        if (Status != UserStatus.Active)
            throw new InvalidOperationException("A user that is not active cannot sign in.");
        LastAccessAt = now;
    }
}
