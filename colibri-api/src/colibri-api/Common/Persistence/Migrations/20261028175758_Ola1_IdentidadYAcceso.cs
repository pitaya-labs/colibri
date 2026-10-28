using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace colibriapi.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Ola1_IdentidadYAcceso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_responsable = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    rfc = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant", x => x.id);
                    table.CheckConstraint("ck_tenant_estado", "estado IN ('activo', 'suspendido')");
                });

            migrationBuilder.CreateTable(
                name: "consultorio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    domicilio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    contacto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    zona_horaria = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultorio", x => x.id);
                    table.CheckConstraint("ck_consultorio_estado", "estado IN ('activo', 'inactivo')");
                    table.ForeignKey(
                        name: "FK_consultorio_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medico",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_completo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    cedula_profesional = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cedula_especialidad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    estado_verificacion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medico", x => x.id);
                    table.CheckConstraint("ck_medico_estado_verificacion", "estado_verificacion IN ('pendiente', 'verificado', 'rechazado')");
                    table.ForeignKey(
                        name: "FK_medico_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    medico_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nombre_completo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    correo = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    mfa_habilitado = table.Column<bool>(type: "boolean", nullable: false),
                    ultimo_acceso = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id);
                    table.CheckConstraint("ck_usuario_estado", "estado IN ('activo', 'suspendido', 'baja')");
                    table.CheckConstraint("ck_usuario_medico_id", "(tipo = 'medico') = (medico_id IS NOT NULL)");
                    table.CheckConstraint("ck_usuario_tipo", "tipo IN ('medico', 'administrativo', 'administrador_sistema', 'soporte')");
                    table.ForeignKey(
                        name: "FK_usuario_medico_medico_id",
                        column: x => x.medico_id,
                        principalTable: "medico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "adscripcion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consultorio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    vigencia_desde = table.Column<DateOnly>(type: "date", nullable: false),
                    vigencia_hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adscripcion", x => x.id);
                    table.CheckConstraint("ck_adscripcion_estado", "estado IN ('activa', 'inactiva')");
                    table.CheckConstraint("ck_adscripcion_rol", "rol IN ('medico', 'administrativo')");
                    table.CheckConstraint("ck_adscripcion_vigencia", "vigencia_hasta IS NULL OR vigencia_hasta >= vigencia_desde");
                    table.ForeignKey(
                        name: "FK_adscripcion_consultorio_consultorio_id",
                        column: x => x.consultorio_id,
                        principalTable: "consultorio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_adscripcion_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_adscripcion_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mfa_credencial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    secreto_cifrado = table.Column<byte[]>(type: "bytea", nullable: false),
                    estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mfa_credencial", x => x.id);
                    table.CheckConstraint("ck_mfa_credencial_estado", "estado IN ('activo', 'revocado')");
                    table.CheckConstraint("ck_mfa_credencial_tipo", "tipo IN ('totp')");
                    table.ForeignKey(
                        name: "FK_mfa_credencial_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sesion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expira_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    origen_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesion", x => x.id);
                    table.CheckConstraint("ck_sesion_estado", "estado IN ('activa', 'revocada', 'expirada')");
                    table.ForeignKey(
                        name: "FK_sesion_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_adscripcion_usuario_activa",
                table: "adscripcion",
                columns: new[] { "usuario_id", "consultorio_id" },
                filter: "estado = 'activa'");

            migrationBuilder.CreateIndex(
                name: "IX_adscripcion_consultorio_id",
                table: "adscripcion",
                column: "consultorio_id");

            migrationBuilder.CreateIndex(
                name: "IX_adscripcion_tenant_id",
                table: "adscripcion",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_adscripcion_usuario_id_consultorio_id_vigencia_desde",
                table: "adscripcion",
                columns: new[] { "usuario_id", "consultorio_id", "vigencia_desde" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_consultorio_tenant_id",
                table: "consultorio",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_medico_tenant_id_cedula_profesional",
                table: "medico",
                columns: new[] { "tenant_id", "cedula_profesional" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mfa_credencial_usuario_id",
                table: "mfa_credencial",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_sesion_usuario_id",
                table: "sesion",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_medico_id",
                table: "usuario",
                column: "medico_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_tenant_id_correo",
                table: "usuario",
                columns: new[] { "tenant_id", "correo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adscripcion");

            migrationBuilder.DropTable(
                name: "mfa_credencial");

            migrationBuilder.DropTable(
                name: "sesion");

            migrationBuilder.DropTable(
                name: "consultorio");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "medico");

            migrationBuilder.DropTable(
                name: "tenant");
        }
    }
}
