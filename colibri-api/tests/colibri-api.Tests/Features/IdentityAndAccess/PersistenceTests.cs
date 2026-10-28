using colibri_api.Common.Persistence;
using colibri_api.Features.Doctors.Domain;
using colibri_api.Features.Tenants.Domain;
using colibri_api.Features.Users.Domain;
using colibri_api.Tests.Common;

namespace colibri_api.Tests.Features.IdentityAndAccess;

public class PersistenceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Saves_and_reads_back_a_doctor_with_its_user()
    {
        await using var db = TestDb.Create();
        var tenant = Tenant.Create("Dr. Test", "xaxx010101000", Now);
        var doctor = Doctor.Create(tenant.Id, "Dr. Test", "12345678", null, Now);
        var user = User.Create(tenant.Id, UserType.Doctor, doctor.Id, "Dr. Test", "dr@test.mx", Now);
        db.AddRange(tenant, doctor, user);

        Assert.Equal(SaveResult.Success, await db.TrySaveChangesAsync());

        db.ChangeTracker.Clear();
        var loaded = await db.Users.FindAsync(user.Id);
        Assert.Equal(UserType.Doctor, loaded!.Type);
        Assert.Equal(doctor.Id, loaded.DoctorId);
    }

    [Fact]
    public async Task Rejects_a_duplicate_license_within_the_same_tenant()
    {
        await using var db = TestDb.Create();
        var tenant = Tenant.Create("T", null, Now);
        db.AddRange(tenant, Doctor.Create(tenant.Id, "A", "111", null, Now));
        await db.TrySaveChangesAsync();

        db.Add(Doctor.Create(tenant.Id, "B", "111", null, Now));

        Assert.Equal(SaveResult.DuplicateKey, await db.TrySaveChangesAsync());
    }

    [Fact]
    public async Task Rejects_a_duplicate_email_within_a_tenant_but_not_across_tenants()
    {
        await using var db = TestDb.Create();
        var t1 = Tenant.Create("T1", null, Now);
        var t2 = Tenant.Create("T2", null, Now);
        db.AddRange(t1, t2, User.Create(t1.Id, UserType.Administrative, null, "A", "x@y.mx", Now));
        await db.TrySaveChangesAsync();

        db.Add(User.Create(t2.Id, UserType.Administrative, null, "B", "x@y.mx", Now));
        Assert.Equal(SaveResult.Success, await db.TrySaveChangesAsync());

        db.Add(User.Create(t1.Id, UserType.Support, null, "C", "X@Y.mx", Now));
        Assert.Equal(SaveResult.DuplicateKey, await db.TrySaveChangesAsync());
    }
}
