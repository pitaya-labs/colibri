using colibri_api.Features.Clinics.Domain;
using colibri_api.Features.Tenants.Domain;
using colibri_api.Features.Users.Domain;

namespace colibri_api.Features.Assignments.Domain;

public enum AssignmentRole
{
    Doctor,
    Administrative
}

public enum AssignmentStatus
{
    Active,
    Inactive
}

/// <summary>
/// Links a user (doctor or administrative staff) to a clinic for a validity period (adscripción).
/// It defines the scope of administrative staff, who work across several doctors.
/// </summary>
public sealed class Assignment
{
    private Assignment() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid ClinicId { get; private set; }
    public Clinic Clinic { get; private set; } = null!;
    public AssignmentRole Role { get; private set; }
    public DateOnly ValidFrom { get; private set; }
    public DateOnly? ValidTo { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>The role is derived from the user type: only doctors and administrative staff can be assigned.</summary>
    public static Assignment Create(
        Guid tenantId, User user, Clinic clinic, DateOnly validFrom, DateOnly? validTo, DateTimeOffset now)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("An assignment requires a tenant.", nameof(tenantId));
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(clinic);

        if (user.Status != UserStatus.Active)
            throw new InvalidOperationException("Only an active user can be assigned to a clinic.");
        if (validTo is not null && validTo < validFrom)
            throw new ArgumentException("The validity period cannot end before it starts.", nameof(validTo));

        var role = user.Type switch
        {
            UserType.Doctor => AssignmentRole.Doctor,
            UserType.Administrative => AssignmentRole.Administrative,
            _ => throw new InvalidOperationException(
                "Only a doctor or administrative user can be assigned to a clinic.")
        };

        return new Assignment
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            UserId = user.Id,
            ClinicId = clinic.Id,
            Role = role,
            ValidFrom = validFrom,
            ValidTo = validTo,
            Status = AssignmentStatus.Active,
            CreatedAt = now
        };
    }

    public void Deactivate(DateOnly until)
    {
        if (Status == AssignmentStatus.Inactive)
            throw new InvalidOperationException("The assignment is already inactive.");
        if (until < ValidFrom)
            throw new ArgumentException("The validity period cannot end before it starts.", nameof(until));

        ValidTo = until;
        Status = AssignmentStatus.Inactive;
    }
}
