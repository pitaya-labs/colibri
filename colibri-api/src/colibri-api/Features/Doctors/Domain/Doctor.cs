using colibri_api.Features.Tenants.Domain;

namespace colibri_api.Features.Doctors.Domain;

public enum DoctorVerificationStatus
{
    Pending,
    Verified,
    Rejected
}

public sealed class Doctor
{
    private Doctor() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;
    public string FullName { get; private set; } = null!;

    /// <summary>Professional license number (cédula profesional).</summary>
    public string ProfessionalLicense { get; private set; } = null!;

    /// <summary>Specialty license number (cédula de especialidad).</summary>
    public string? SpecialtyLicense { get; private set; }
    public DoctorVerificationStatus VerificationStatus { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Doctor Create(
        Guid tenantId, string fullName, string professionalLicense, string? specialtyLicense,
        DateTimeOffset now)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("A doctor requires a tenant.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(professionalLicense);

        return new Doctor
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            FullName = fullName.Trim(),
            ProfessionalLicense = professionalLicense.Trim(),
            SpecialtyLicense = string.IsNullOrWhiteSpace(specialtyLicense) ? null : specialtyLicense.Trim(),
            VerificationStatus = DoctorVerificationStatus.Pending,
            CreatedAt = now
        };
    }

    public void Verify()
    {
        if (VerificationStatus != DoctorVerificationStatus.Pending)
            throw new InvalidOperationException("Only a pending doctor can be verified.");
        VerificationStatus = DoctorVerificationStatus.Verified;
    }

    public void Reject()
    {
        if (VerificationStatus != DoctorVerificationStatus.Pending)
            throw new InvalidOperationException("Only a pending doctor can be rejected.");
        VerificationStatus = DoctorVerificationStatus.Rejected;
    }
}
