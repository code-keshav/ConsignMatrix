using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddConsignmentPackageStatusLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "consignment",
                schema: "consignment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tracking_number = table.Column<string>(type: "text", nullable: false),
                    sender_id = table.Column<long>(type: "bigint", nullable: false),
                    sender_address_id = table.Column<long>(type: "bigint", nullable: false),
                    receiver_id = table.Column<long>(type: "bigint", nullable: false),
                    receiver_address_id = table.Column<long>(type: "bigint", nullable: false),
                    origin_branch_id = table.Column<long>(type: "bigint", nullable: false),
                    destination_branch_id = table.Column<long>(type: "bigint", nullable: false),
                    service_type = table.Column<int>(type: "integer", nullable: false),
                    payment_mode = table.Column<int>(type: "integer", nullable: false),
                    declared_value = table.Column<decimal>(type: "numeric", nullable: true),
                    cod_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    special_instructions = table.Column<string>(type: "text", nullable: true),
                    total_weight = table.Column<decimal>(type: "numeric", nullable: false),
                    volumetric_weight = table.Column<decimal>(type: "numeric", nullable: false),
                    chargeable_weight = table.Column<decimal>(type: "numeric", nullable: false),
                    total_volume = table.Column<decimal>(type: "numeric", nullable: false),
                    package_count = table.Column<int>(type: "integer", nullable: false),
                    expected_delivery_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    actual_delivery_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consignment", x => x.id);
                    table.ForeignKey(
                        name: "fk_consignment_branch_destination_branch_id",
                        column: x => x.destination_branch_id,
                        principalSchema: "Base",
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_consignment_branch_origin_branch_id",
                        column: x => x.origin_branch_id,
                        principalSchema: "Base",
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_consignment_customer_address_receiver_address_id",
                        column: x => x.receiver_address_id,
                        principalSchema: "consignment",
                        principalTable: "customer_address",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_consignment_customer_address_sender_address_id",
                        column: x => x.sender_address_id,
                        principalSchema: "consignment",
                        principalTable: "customer_address",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_consignment_customer_receiver_id",
                        column: x => x.receiver_id,
                        principalSchema: "consignment",
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_consignment_customer_sender_id",
                        column: x => x.sender_id,
                        principalSchema: "consignment",
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_consignment_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "consignment_status_log",
                schema: "consignment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    consignment_id = table.Column<long>(type: "bigint", nullable: false),
                    status_type = table.Column<int>(type: "integer", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consignment_status_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_consignment_status_log_consignment_consignment_id",
                        column: x => x.consignment_id,
                        principalSchema: "consignment",
                        principalTable: "consignment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_consignment_status_log_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "package",
                schema: "consignment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    consignment_id = table.Column<long>(type: "bigint", nullable: false),
                    package_number = table.Column<int>(type: "integer", nullable: false),
                    barcode = table.Column<string>(type: "text", nullable: false),
                    weight = table.Column<decimal>(type: "numeric", nullable: false),
                    length = table.Column<decimal>(type: "numeric", nullable: false),
                    width = table.Column<decimal>(type: "numeric", nullable: false),
                    height = table.Column<decimal>(type: "numeric", nullable: false),
                    volume = table.Column<decimal>(type: "numeric", nullable: false),
                    volumetric_weight = table.Column<decimal>(type: "numeric", nullable: false),
                    package_type = table.Column<int>(type: "integer", nullable: false),
                    content_description = table.Column<string>(type: "text", nullable: true),
                    is_fragile = table.Column<bool>(type: "boolean", nullable: false),
                    is_hazardous = table.Column<bool>(type: "boolean", nullable: false),
                    can_be_stacked = table.Column<bool>(type: "boolean", nullable: false),
                    requires_cold_chain = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_package", x => x.id);
                    table.ForeignKey(
                        name: "fk_package_consignment_consignment_id",
                        column: x => x.consignment_id,
                        principalSchema: "consignment",
                        principalTable: "consignment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_package_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_consignment_destination_branch_id",
                schema: "consignment",
                table: "consignment",
                column: "destination_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_consignment_origin_branch_id",
                schema: "consignment",
                table: "consignment",
                column: "origin_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_consignment_rec_by_id",
                schema: "consignment",
                table: "consignment",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_consignment_receiver_address_id",
                schema: "consignment",
                table: "consignment",
                column: "receiver_address_id");

            migrationBuilder.CreateIndex(
                name: "ix_consignment_receiver_id",
                schema: "consignment",
                table: "consignment",
                column: "receiver_id");

            migrationBuilder.CreateIndex(
                name: "ix_consignment_sender_address_id",
                schema: "consignment",
                table: "consignment",
                column: "sender_address_id");

            migrationBuilder.CreateIndex(
                name: "ix_consignment_sender_id",
                schema: "consignment",
                table: "consignment",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "ix_consignment_tracking_number",
                schema: "consignment",
                table: "consignment",
                column: "tracking_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_consignment_status_log_consignment_id",
                schema: "consignment",
                table: "consignment_status_log",
                column: "consignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_consignment_status_log_rec_by_id",
                schema: "consignment",
                table: "consignment_status_log",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_package_barcode",
                schema: "consignment",
                table: "package",
                column: "barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_package_consignment_id",
                schema: "consignment",
                table: "package",
                column: "consignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_package_rec_by_id",
                schema: "consignment",
                table: "package",
                column: "rec_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consignment_status_log",
                schema: "consignment");

            migrationBuilder.DropTable(
                name: "package",
                schema: "consignment");

            migrationBuilder.DropTable(
                name: "consignment",
                schema: "consignment");
        }
    }
}
