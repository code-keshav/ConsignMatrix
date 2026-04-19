using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPickupTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pickup_task",
                schema: "consignment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    consignment_id = table.Column<long>(type: "bigint", nullable: false),
                    pickup_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    pickup_slot = table.Column<int>(type: "integer", nullable: false),
                    pickup_address = table.Column<string>(type: "text", nullable: false),
                    contact_phone = table.Column<string>(type: "text", nullable: false),
                    contact_name = table.Column<string>(type: "text", nullable: true),
                    assigned_driver_id = table.Column<long>(type: "bigint", nullable: true),
                    assigned_vehicle_id = table.Column<long>(type: "bigint", nullable: true),
                    task_status = table.Column<int>(type: "integer", nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    pickup_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    verified_weight = table.Column<decimal>(type: "numeric", nullable: true),
                    fail_reason = table.Column<int>(type: "integer", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pickup_task", x => x.id);
                    table.ForeignKey(
                        name: "fk_pickup_task_consignment_consignment_id",
                        column: x => x.consignment_id,
                        principalSchema: "consignment",
                        principalTable: "consignment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pickup_task_employee_assigned_driver_id",
                        column: x => x.assigned_driver_id,
                        principalSchema: "Base",
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_pickup_task_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pickup_task_vehicle_assigned_vehicle_id",
                        column: x => x.assigned_vehicle_id,
                        principalSchema: "Base",
                        principalTable: "vehicle",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pickup_task_assigned_driver_id",
                schema: "consignment",
                table: "pickup_task",
                column: "assigned_driver_id");

            migrationBuilder.CreateIndex(
                name: "ix_pickup_task_assigned_vehicle_id",
                schema: "consignment",
                table: "pickup_task",
                column: "assigned_vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_pickup_task_consignment_id",
                schema: "consignment",
                table: "pickup_task",
                column: "consignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_pickup_task_rec_by_id",
                schema: "consignment",
                table: "pickup_task",
                column: "rec_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pickup_task",
                schema: "consignment");
        }
    }
}
