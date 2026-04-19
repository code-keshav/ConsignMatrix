using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vehicle",
                schema: "Base",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vehicle_number = table.Column<string>(type: "text", nullable: false),
                    vehicle_type = table.Column<int>(type: "integer", nullable: false),
                    ownership_type = table.Column<int>(type: "integer", nullable: false),
                    max_weight_capacity = table.Column<decimal>(type: "numeric", nullable: false),
                    max_volume_capacity = table.Column<decimal>(type: "numeric", nullable: false),
                    supports_fragile = table.Column<bool>(type: "boolean", nullable: false),
                    has_cold_storage = table.Column<bool>(type: "boolean", nullable: false),
                    vehicle_status = table.Column<int>(type: "integer", nullable: false),
                    last_service_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    insurance_expiry = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fuel_type = table.Column<int>(type: "integer", nullable: false),
                    current_branch_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_branch_current_branch_id",
                        column: x => x.current_branch_id,
                        principalSchema: "Base",
                        principalTable: "branch",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_vehicle_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_current_branch_id",
                schema: "Base",
                table: "vehicle",
                column: "current_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_rec_by_id",
                schema: "Base",
                table: "vehicle",
                column: "rec_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicle",
                schema: "Base");
        }
    }
}
