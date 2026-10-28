using colibri_api.Features.Adscripciones.Domain;
using colibri_api.Features.Consultorios.Domain;
using colibri_api.Features.Usuarios.Domain;

namespace colibri_api.Tests.Features.IdentidadYAcceso;

public class DominioTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid TenantId = Guid.CreateVersion7();

    private static Usuario UsuarioDe(TipoUsuario tipo) =>
        Usuario.Crear(TenantId, tipo, tipo == TipoUsuario.Medico ? Guid.CreateVersion7() : null,
            "Nombre", "  Correo@Ejemplo.MX ", Ahora);

    private static Consultorio ConsultorioDePrueba() =>
        Consultorio.Crear(TenantId, "Consultorio", "Calle 1", null, null, Ahora);

    [Fact]
    public void Usuario_medico_requiere_medicoId()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Usuario.Crear(TenantId, TipoUsuario.Medico, null, "N", "a@b.mx", Ahora));
    }

    [Theory]
    [InlineData(TipoUsuario.Administrativo)]
    [InlineData(TipoUsuario.AdministradorSistema)]
    [InlineData(TipoUsuario.Soporte)]
    public void Usuario_no_medico_no_puede_tener_medicoId(TipoUsuario tipo)
    {
        Assert.Throws<InvalidOperationException>(() =>
            Usuario.Crear(TenantId, tipo, Guid.CreateVersion7(), "N", "a@b.mx", Ahora));
    }

    [Fact]
    public void Usuario_normaliza_el_correo()
    {
        Assert.Equal("correo@ejemplo.mx", UsuarioDe(TipoUsuario.Administrativo).Correo);
    }

    [Fact]
    public void Usuario_dado_de_baja_no_puede_reactivarse_ni_acceder()
    {
        var usuario = UsuarioDe(TipoUsuario.Administrativo);
        usuario.DarDeBaja();

        Assert.Throws<InvalidOperationException>(usuario.Reactivar);
        Assert.Throws<InvalidOperationException>(() => usuario.RegistrarAcceso(Ahora));
    }

    [Fact]
    public void Consultorio_usa_zona_horaria_por_defecto_y_rechaza_zonas_desconocidas()
    {
        Assert.Equal(Consultorio.ZonaHorariaPorDefecto, ConsultorioDePrueba().ZonaHoraria);
        Assert.Throws<ArgumentException>(() =>
            Consultorio.Crear(TenantId, "C", "D", null, "Marte/Olympus", Ahora));
    }

    [Fact]
    public void Adscripcion_deriva_el_rol_del_tipo_de_usuario()
    {
        var desde = new DateOnly(2026, 11, 1);

        var medico = Adscripcion.Crear(TenantId, UsuarioDe(TipoUsuario.Medico), ConsultorioDePrueba(), desde, null, Ahora);
        var admin = Adscripcion.Crear(TenantId, UsuarioDe(TipoUsuario.Administrativo), ConsultorioDePrueba(), desde, null, Ahora);

        Assert.Equal(RolAdscripcion.Medico, medico.Rol);
        Assert.Equal(RolAdscripcion.Administrativo, admin.Rol);
    }

    [Fact]
    public void Adscripcion_rechaza_tipos_sin_rol_y_vigencias_invertidas()
    {
        var desde = new DateOnly(2026, 11, 1);

        Assert.Throws<InvalidOperationException>(() =>
            Adscripcion.Crear(TenantId, UsuarioDe(TipoUsuario.Soporte), ConsultorioDePrueba(), desde, null, Ahora));
        Assert.Throws<ArgumentException>(() =>
            Adscripcion.Crear(TenantId, UsuarioDe(TipoUsuario.Medico), ConsultorioDePrueba(), desde, desde.AddDays(-1), Ahora));
    }

    [Fact]
    public void Adscripcion_requiere_usuario_activo()
    {
        var usuario = UsuarioDe(TipoUsuario.Administrativo);
        usuario.Suspender();

        Assert.Throws<InvalidOperationException>(() =>
            Adscripcion.Crear(TenantId, usuario, ConsultorioDePrueba(), new DateOnly(2026, 11, 1), null, Ahora));
    }

    [Fact]
    public void Sesion_debe_expirar_despues_de_crearse()
    {
        Assert.Throws<ArgumentException>(() =>
            Sesion.Crear(Guid.CreateVersion7(), Ahora, Ahora, null, null));
    }

    [Fact]
    public void Sesion_revocada_deja_de_estar_vigente_y_no_se_revoca_dos_veces()
    {
        var sesion = Sesion.Crear(Guid.CreateVersion7(), Ahora, Ahora.AddHours(1), "127.0.0.1", "test");
        Assert.True(sesion.EstaVigente(Ahora));

        sesion.Revocar();

        Assert.False(sesion.EstaVigente(Ahora));
        Assert.Throws<InvalidOperationException>(sesion.Revocar);
    }

    [Fact]
    public void Sesion_no_puede_expirar_antes_de_su_fecha()
    {
        var sesion = Sesion.Crear(Guid.CreateVersion7(), Ahora, Ahora.AddHours(1), null, null);

        Assert.Throws<InvalidOperationException>(() => sesion.Expirar(Ahora));
        sesion.Expirar(Ahora.AddHours(2));
        Assert.Equal(EstadoSesion.Expirada, sesion.Estado);
    }

    [Fact]
    public void MfaCredencial_exige_secreto_y_se_revoca_una_vez()
    {
        Assert.Throws<ArgumentException>(() => MfaCredencial.Crear(Guid.CreateVersion7(), [], Ahora));

        var credencial = MfaCredencial.Crear(Guid.CreateVersion7(), [1, 2, 3], Ahora);
        credencial.Revocar();

        Assert.Throws<InvalidOperationException>(credencial.Revocar);
    }
}
