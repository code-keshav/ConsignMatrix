using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceBranchEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "branch_type",
                schema: "Base",
                table: "branch",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "city",
                schema: "Base",
                table: "branch",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "current_load",
                schema: "Base",
                table: "branch",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "latitude",
                schema: "Base",
                table: "branch",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "longitude",
                schema: "Base",
                table: "branch",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "operating_hours",
                schema: "Base",
                table: "branch",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state",
                schema: "Base",
                table: "branch",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "storage_capacity",
                schema: "Base",
                table: "branch",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "branch_pin_code",
                schema: "Base",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    pin_code = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_branch_pin_code", x => x.id);
                    table.ForeignKey(
                        name: "fk_branch_pin_code_branch_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "Base",
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_branch_pin_code_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_branch_pin_code_branch_id",
                schema: "Base",
                table: "branch_pin_code",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_branch_pin_code_rec_by_id",
                schema: "Base",
                table: "branch_pin_code",
                column: "rec_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "branch_pin_code",
                schema: "Base");

            migrationBuilder.DropColumn(
                name: "branch_type",
                schema: "Base",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "city",
                schema: "Base",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "current_load",
                schema: "Base",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "latitude",
                schema: "Base",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "longitude",
                schema: "Base",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "operating_hours",
                schema: "Base",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "state",
                schema: "Base",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "storage_capacity",
                schema: "Base",
                table: "branch");
        }
    }
}
