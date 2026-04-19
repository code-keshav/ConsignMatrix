using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTripBranchOps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trip",
                schema: "consignment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trip_number = table.Column<string>(type: "text", nullable: false),
                    trip_type = table.Column<int>(type: "integer", nullable: false),
                    from_branch_id = table.Column<long>(type: "bigint", nullable: false),
                    to_branch_id = table.Column<long>(type: "bigint", nullable: false),
                    driver_id = table.Column<long>(type: "bigint", nullable: false),
                    vehicle_id = table.Column<long>(type: "bigint", nullable: false),
                    scheduled_departure = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    actual_departure = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    trip_status = table.Column<int>(type: "integer", nullable: false),
                    total_consignments = table.Column<int>(type: "integer", nullable: false),
                    total_weight = table.Column<decimal>(type: "numeric", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip", x => x.id);
                    table.ForeignKey(
                        name: "fk_trip_branch_from_branch_id",
                        column: x => x.from_branch_id,
                        principalSchema: "Base",
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_trip_branch_to_branch_id",
                        column: x => x.to_branch_id,
                        principalSchema: "Base",
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_trip_employee_driver_id",
                        column: x => x.driver_id,
                        principalSchema: "Base",
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_trip_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_trip_vehicle_vehicle_id",
                        column: x => x.vehicle_id,
                        principalSchema: "Base",
                        principalTable: "vehicle",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_consignment",
                schema: "consignment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trip_id = table.Column<long>(type: "bigint", nullable: false),
                    consignment_id = table.Column<long>(type: "bigint", nullable: false),
                    loaded_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip_consignment", x => x.id);
                    table.ForeignKey(
                        name: "fk_trip_consignment_consignment_consignment_id",
                        column: x => x.consignment_id,
                        principalSchema: "consignment",
                        principalTable: "consignment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_trip_consignment_trip_trip_id",
                        column: x => x.trip_id,
                        principalSchema: "consignment",
                        principalTable: "trip",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_trip_consignment_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_trip_driver_id",
                schema: "consignment",
                table: "trip",
                column: "driver_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_from_branch_id",
                schema: "consignment",
                table: "trip",
                column: "from_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_rec_by_id",
                schema: "consignment",
                table: "trip",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_to_branch_id",
                schema: "consignment",
                table: "trip",
                column: "to_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_trip_number",
                schema: "consignment",
                table: "trip",
                column: "trip_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trip_vehicle_id",
                schema: "consignment",
                table: "trip",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_consignment_consignment_id",
                schema: "consignment",
                table: "trip_consignment",
                column: "consignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_consignment_rec_by_id",
                schema: "consignment",
                table: "trip_consignment",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_consignment_trip_id",
                schema: "consignment",
                table: "trip_consignment",
                column: "trip_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trip_consignment",
                schema: "consignment");

            migrationBuilder.DropTable(
                name: "trip",
                schema: "consignment");
        }
    }
}
