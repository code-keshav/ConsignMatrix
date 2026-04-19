using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "consignment");

            migrationBuilder.CreateTable(
                name: "customer",
                schema: "consignment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    phone_no = table.Column<string>(type: "text", nullable: false),
                    secondary_phone_no = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    customer_type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_address",
                schema: "consignment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    address_type = table.Column<int>(type: "integer", nullable: false),
                    address_line1 = table.Column<string>(type: "text", nullable: false),
                    address_line2 = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "text", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false),
                    pin_code = table.Column<string>(type: "text", nullable: false),
                    landmark = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<string>(type: "text", nullable: true),
                    longitude = table.Column<string>(type: "text", nullable: true),
                    contact_no = table.Column<string>(type: "text", nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_address", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_address_customer_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "consignment",
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_customer_address_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_customer_rec_by_id",
                schema: "consignment",
                table: "customer",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_address_customer_id",
                schema: "consignment",
                table: "customer_address",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_address_rec_by_id",
                schema: "consignment",
                table: "customer_address",
                column: "rec_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_address",
                schema: "consignment");

            migrationBuilder.DropTable(
                name: "customer",
                schema: "consignment");
        }
    }
}
