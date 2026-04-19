using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vehicle_assignment",
                schema: "Base",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vehicle_id = table.Column<long>(type: "bigint", nullable: false),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    assignment_type = table.Column<int>(type: "integer", nullable: false),
                    assigned_from = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    assigned_to = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    trip_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_assignment", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_assignment_employee_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "Base",
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vehicle_assignment_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vehicle_assignment_vehicle_vehicle_id",
                        column: x => x.vehicle_id,
                        principalSchema: "Base",
                        principalTable: "vehicle",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_assignment_employee_id",
                schema: "Base",
                table: "vehicle_assignment",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_assignment_rec_by_id",
                schema: "Base",
                table: "vehicle_assignment",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_assignment_vehicle_id",
                schema: "Base",
                table: "vehicle_assignment",
                column: "vehicle_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicle_assignment",
                schema: "Base");
        }
    }
}
