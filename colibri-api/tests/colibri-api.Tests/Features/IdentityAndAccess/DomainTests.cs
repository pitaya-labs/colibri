using colibri_api.Features.Assignments.Domain;
using colibri_api.Features.Clinics.Domain;
using colibri_api.Features.Users.Domain;

namespace colibri_api.Tests.Features.IdentityAndAccess;

public class DomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid TenantId = Guid.CreateVersion7();

    private static User UserOf(UserType type) =>
        User.Create(TenantId, type, type == UserType.Doctor ? Guid.CreateVersion7() : null,
            "Name", "  Email@Example.MX ", Now);

    private static Clinic SampleClinic() =>
        Clinic.Create(TenantId, "Clinic", "Street 1", null, null, Now);

    [Fact]
    public void Doctor_user_requires_doctorId()
    {
        Assert.Throws<InvalidOperationException>(() =>
            User.Create(TenantId, UserType.Doctor, null, "N", "a@b.mx", Now));
    }

    [Theory]
    [InlineData(UserType.Administrative)]
    [InlineData(UserType.SystemAdmin)]
    [InlineData(UserType.Support)]
    public void Non_doctor_user_cannot_have_doctorId(UserType type)
    {
        Assert.Throws<InvalidOperationException>(() =>
            User.Create(TenantId, type, Guid.CreateVersion7(), "N", "a@b.mx", Now));
    }

    [Fact]
    public void User_normalizes_the_email()
    {
        Assert.Equal("email@example.mx", UserOf(UserType.Administrative).Email);
    }

    [Fact]
    public void Terminated_user_cannot_be_reactivated_or_sign_in()
    {
        var user = UserOf(UserType.Administrative);
        user.Terminate();

        Assert.Throws<InvalidOperationException>(user.Reactivate);
        Assert.Throws<InvalidOperationException>(() => user.RecordAccess(Now));
    }

    [Fact]
    public void Clinic_uses_the_default_time_zone_and_rejects_unknown_zones()
    {
        Assert.Equal(Clinic.DefaultTimeZone, SampleClinic().TimeZone);
        Assert.Throws<ArgumentException>(() =>
            Clinic.Create(TenantId, "C", "A", null, "Mars/Olympus", Now));
    }

    [Fact]
    public void Assignment_derives_the_role_from_the_user_type()
    {
        var from = new DateOnly(2026, 11, 1);

        var doctor = Assignment.Create(TenantId, UserOf(UserType.Doctor), SampleClinic(), from, null, Now);
        var admin = Assignment.Create(TenantId, UserOf(UserType.Administrative), SampleClinic(), from, null, Now);

        Assert.Equal(AssignmentRole.Doctor, doctor.Role);
        Assert.Equal(AssignmentRole.Administrative, admin.Role);
    }

    [Fact]
    public void Assignment_rejects_types_without_a_role_and_inverted_validity()
    {
        var from = new DateOnly(2026, 11, 1);

        Assert.Throws<InvalidOperationException>(() =>
            Assignment.Create(TenantId, UserOf(UserType.Support), SampleClinic(), from, null, Now));
        Assert.Throws<ArgumentException>(() =>
            Assignment.Create(TenantId, UserOf(UserType.Doctor), SampleClinic(), from, from.AddDays(-1), Now));
    }

    [Fact]
    public void Assignment_requires_an_active_user()
    {
        var user = UserOf(UserType.Administrative);
        user.Suspend();

        Assert.Throws<InvalidOperationException>(() =>
            Assignment.Create(TenantId, user, SampleClinic(), new DateOnly(2026, 11, 1), null, Now));
    }

    [Fact]
    public void Session_must_expire_after_it_is_created()
    {
        Assert.Throws<ArgumentException>(() =>
            Session.Create(Guid.CreateVersion7(), Now, Now, null, null));
    }

    [Fact]
    public void Revoked_session_is_no_longer_active_and_cannot_be_revoked_twice()
    {
        var session = Session.Create(Guid.CreateVersion7(), Now, Now.AddHours(1), "127.0.0.1", "test");
        Assert.True(session.IsActive(Now));

        session.Revoke();

        Assert.False(session.IsActive(Now));
        Assert.Throws<InvalidOperationException>(session.Revoke);
    }

    [Fact]
    public void Session_cannot_expire_before_its_expiration_time()
    {
        var session = Session.Create(Guid.CreateVersion7(), Now, Now.AddHours(1), null, null);

        Assert.Throws<InvalidOperationException>(() => session.Expire(Now));
        session.Expire(Now.AddHours(2));
        Assert.Equal(SessionStatus.Expired, session.Status);
    }

    [Fact]
    public void MfaCredential_requires_a_secret_and_is_revoked_once()
    {
        Assert.Throws<ArgumentException>(() => MfaCredential.Create(Guid.CreateVersion7(), [], Now));

        var credential = MfaCredential.Create(Guid.CreateVersion7(), [1, 2, 3], Now);
        credential.Revoke();

        Assert.Throws<InvalidOperationException>(credential.Revoke);
    }
}
