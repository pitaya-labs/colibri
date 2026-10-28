using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace colibriapi.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Wave1_IdentityAndAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    responsible_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    rfc = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant", x => x.id);
                    table.CheckConstraint("ck_tenant_status", "status IN ('active', 'suspended')");
                });

            migrationBuilder.CreateTable(
                name: "clinic",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    contact = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    time_zone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinic", x => x.id);
                    table.CheckConstraint("ck_clinic_status", "status IN ('active', 'inactive')");
                    table.ForeignKey(
                        name: "FK_clinic_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "doctor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    professional_license = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    specialty_license = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    verification_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor", x => x.id);
                    table.CheckConstraint("ck_doctor_verification_status", "verification_status IN ('pending', 'verified', 'rejected')");
                    table.ForeignKey(
                        name: "FK_doctor_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "app_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    full_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    mfa_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    last_access_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_user", x => x.id);
                    table.CheckConstraint("ck_app_user_doctor_id", "(type = 'doctor') = (doctor_id IS NOT NULL)");
                    table.CheckConstraint("ck_app_user_status", "status IN ('active', 'suspended', 'terminated')");
                    table.CheckConstraint("ck_app_user_type", "type IN ('doctor', 'administrative', 'system_admin', 'support')");
                    table.ForeignKey(
                        name: "FK_app_user_doctor_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_app_user_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assignment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    clinic_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assignment", x => x.id);
                    table.CheckConstraint("ck_assignment_role", "role IN ('doctor', 'administrative')");
                    table.CheckConstraint("ck_assignment_status", "status IN ('active', 'inactive')");
                    table.CheckConstraint("ck_assignment_validity", "valid_to IS NULL OR valid_to >= valid_from");
                    table.ForeignKey(
                        name: "FK_assignment_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assignment_clinic_clinic_id",
                        column: x => x.clinic_id,
                        principalTable: "clinic",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assignment_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mfa_credential",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    encrypted_secret = table.Column<byte[]>(type: "bytea", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mfa_credential", x => x.id);
                    table.CheckConstraint("ck_mfa_credential_status", "status IN ('active', 'revoked')");
                    table.CheckConstraint("ck_mfa_credential_type", "type IN ('totp')");
                    table.ForeignKey(
                        name: "FK_mfa_credential_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "session",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    origin_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session", x => x.id);
                    table.CheckConstraint("ck_session_status", "status IN ('active', 'revoked', 'expired')");
                    table.ForeignKey(
                        name: "FK_session_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_user_doctor_id",
                table: "app_user",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_user_tenant_id_email",
                table: "app_user",
                columns: new[] { "tenant_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_assignment_active_user",
                table: "assignment",
                columns: new[] { "user_id", "clinic_id" },
                filter: "status = 'active'");

            migrationBuilder.CreateIndex(
                name: "IX_assignment_clinic_id",
                table: "assignment",
                column: "clinic_id");

            migrationBuilder.CreateIndex(
                name: "IX_assignment_tenant_id",
                table: "assignment",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_assignment_user_id_clinic_id_valid_from",
                table: "assignment",
                columns: new[] { "user_id", "clinic_id", "valid_from" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clinic_tenant_id",
                table: "clinic",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctor_tenant_id_professional_license",
                table: "doctor",
                columns: new[] { "tenant_id", "professional_license" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mfa_credential_user_id",
                table: "mfa_credential",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_user_id",
                table: "session",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assignment");

            migrationBuilder.DropTable(
                name: "mfa_credential");

            migrationBuilder.DropTable(
                name: "session");

            migrationBuilder.DropTable(
                name: "clinic");

            migrationBuilder.DropTable(
                name: "app_user");

            migrationBuilder.DropTable(
                name: "doctor");

            migrationBuilder.DropTable(
                name: "tenant");
        }
    }
}
