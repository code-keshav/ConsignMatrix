using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class updateuserentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "Base",
                table: "user",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "created_by_id",
                schema: "Base",
                table: "user",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "Base",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_login",
                schema: "Base",
                table: "user",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_created_by_id",
                schema: "Base",
                table: "user",
                column: "created_by_id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_user_created_by_id",
                schema: "Base",
                table: "user",
                column: "created_by_id",
                principalSchema: "Base",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(@"UPDATE ""Base"".""user""
SET is_active = true::boolean;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_user_created_by_id",
                schema: "Base",
                table: "user");

            migrationBuilder.DropIndex(
                name: "ix_user_created_by_id",
                schema: "Base",
                table: "user");

            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "Base",
                table: "user");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                schema: "Base",
                table: "user");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "Base",
                table: "user");

            migrationBuilder.DropColumn(
                name: "last_login",
                schema: "Base",
                table: "user");
        }
    }
}
