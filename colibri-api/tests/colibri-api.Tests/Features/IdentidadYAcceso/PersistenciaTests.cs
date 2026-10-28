using colibri_api.Common.Persistence;
using colibri_api.Features.Medicos.Domain;
using colibri_api.Features.Tenants.Domain;
using colibri_api.Features.Usuarios.Domain;
using colibri_api.Tests.Common;

namespace colibri_api.Tests.Features.IdentidadYAcceso;

public class PersistenciaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Guarda_y_recupera_un_medico_con_su_usuario()
    {
        await using var db = TestDb.Create();
        var tenant = Tenant.Crear("Dra. Prueba", "xaxx010101000", Ahora);
        var medico = Medico.Crear(tenant.Id, "Dra. Prueba", "12345678", null, Ahora);
        var usuario = Usuario.Crear(tenant.Id, TipoUsuario.Medico, medico.Id, "Dra. Prueba", "dra@prueba.mx", Ahora);
        db.AddRange(tenant, medico, usuario);

        Assert.Equal(SaveResult.Success, await db.TrySaveChangesAsync());

        db.ChangeTracker.Clear();
        var leido = await db.Usuarios.FindAsync(usuario.Id);
        Assert.Equal(TipoUsuario.Medico, leido!.Tipo);
        Assert.Equal(medico.Id, leido.MedicoId);
    }

    [Fact]
    public async Task Rechaza_cedula_duplicada_en_el_mismo_tenant()
    {
        await using var db = TestDb.Create();
        var tenant = Tenant.Crear("T", null, Ahora);
        db.AddRange(tenant, Medico.Crear(tenant.Id, "A", "111", null, Ahora));
        await db.TrySaveChangesAsync();

        db.Add(Medico.Crear(tenant.Id, "B", "111", null, Ahora));

        Assert.Equal(SaveResult.DuplicateKey, await db.TrySaveChangesAsync());
    }

    [Fact]
    public async Task Rechaza_correo_duplicado_en_el_mismo_tenant_pero_no_entre_tenants()
    {
        await using var db = TestDb.Create();
        var t1 = Tenant.Crear("T1", null, Ahora);
        var t2 = Tenant.Crear("T2", null, Ahora);
        db.AddRange(t1, t2, Usuario.Crear(t1.Id, TipoUsuario.Administrativo, null, "A", "x@y.mx", Ahora));
        await db.TrySaveChangesAsync();

        db.Add(Usuario.Crear(t2.Id, TipoUsuario.Administrativo, null, "B", "x@y.mx", Ahora));
        Assert.Equal(SaveResult.Success, await db.TrySaveChangesAsync());

        db.Add(Usuario.Crear(t1.Id, TipoUsuario.Soporte, null, "C", "X@Y.mx", Ahora));
        Assert.Equal(SaveResult.DuplicateKey, await db.TrySaveChangesAsync());
    }
}
