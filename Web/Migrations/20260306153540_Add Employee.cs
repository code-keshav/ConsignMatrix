using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employee",
                schema: "Base",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    employee_code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    alternate_phone = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    employee_type = table.Column<int>(type: "integer", nullable: false),
                    employee_status = table.Column<int>(type: "integer", nullable: false),
                    joining_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    termination_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    department = table.Column<string>(type: "text", nullable: true),
                    designation = table.Column<string>(type: "text", nullable: true),
                    current_branch_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employee", x => x.id);
                    table.ForeignKey(
                        name: "fk_employee_branch_current_branch_id",
                        column: x => x.current_branch_id,
                        principalSchema: "Base",
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_employee_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_employee_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "driver",
                schema: "Base",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    license_number = table.Column<string>(type: "text", nullable: false),
                    license_expiry = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_driver", x => x.id);
                    table.ForeignKey(
                        name: "fk_driver_employee_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "Base",
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_driver_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_driver_employee_id",
                schema: "Base",
                table: "driver",
                column: "employee_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_driver_rec_by_id",
                schema: "Base",
                table: "driver",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_current_branch_id",
                schema: "Base",
                table: "employee",
                column: "current_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_employee_code",
                schema: "Base",
                table: "employee",
                column: "employee_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_employee_rec_by_id",
                schema: "Base",
                table: "employee",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_user_id",
                schema: "Base",
                table: "employee",
                column: "user_id",
                unique: true,
                filter: "user_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "driver",
                schema: "Base");

            migrationBuilder.DropTable(
                name: "employee",
                schema: "Base");
        }
    }
}
