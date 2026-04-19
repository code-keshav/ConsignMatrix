using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class initialonuserbranchandrolepermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Base");

            migrationBuilder.EnsureSchema(
                name: "acl");

            migrationBuilder.CreateTable(
                name: "branch",
                schema: "Base",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    contact_no = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_branch", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organization_info",
                schema: "Base",
                columns: table => new
                {
                    item_key = table.Column<string>(type: "text", nullable: false),
                    item_value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_info", x => x.item_key);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "Base",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    normalized_user_name = table.Column<string>(type: "text", nullable: false),
                    normalized_email = table.Column<string>(type: "text", nullable: false),
                    contact_no = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    security_stamp = table.Column<string>(type: "text", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    user_level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_branch_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "Base",
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBranchTransfer",
                schema: "Base",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    from_branch_id = table.Column<long>(type: "bigint", nullable: false),
                    to_branch_id = table.Column<long>(type: "bigint", nullable: false),
                    request_initiator_id = table.Column<long>(type: "bigint", nullable: false),
                    request_note = table.Column<string>(type: "text", nullable: false),
                    request_sent_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    responder_id = table.Column<long>(type: "bigint", nullable: true),
                    response_note = table.Column<string>(type: "text", nullable: true),
                    response_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    transfer_status = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_branch_transfer", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_branch_transfer_branch_from_branch_id",
                        column: x => x.from_branch_id,
                        principalSchema: "Base",
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_branch_transfer_branch_to_branch_id",
                        column: x => x.to_branch_id,
                        principalSchema: "Base",
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_branch_transfer_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_branch_transfer_user_request_initiator_id",
                        column: x => x.request_initiator_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_branch_transfer_user_responder_id",
                        column: x => x.responder_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_branch_transfer_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role",
                schema: "acl",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    priority = table.Column<long>(type: "bigint", nullable: true),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_global = table.Column<bool>(type: "boolean", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_branch_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "Base",
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permission",
                schema: "acl",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    permission = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permission", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_permission_branch_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "Base",
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permission_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "acl",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                schema: "acl",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    rec_by_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_role_branch_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "Base",
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "acl",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_user_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "Base",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_branch_transfer_from_branch_id",
                schema: "Base",
                table: "UserBranchTransfer",
                column: "from_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_branch_transfer_rec_by_id",
                schema: "Base",
                table: "UserBranchTransfer",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_branch_transfer_request_initiator_id",
                schema: "Base",
                table: "UserBranchTransfer",
                column: "request_initiator_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_branch_transfer_responder_id",
                schema: "Base",
                table: "UserBranchTransfer",
                column: "responder_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_branch_transfer_to_branch_id",
                schema: "Base",
                table: "UserBranchTransfer",
                column: "to_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_branch_transfer_user_id",
                schema: "Base",
                table: "UserBranchTransfer",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_branch_id",
                schema: "acl",
                table: "role",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_rec_by_id",
                schema: "acl",
                table: "role",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_permission_branch_id",
                schema: "acl",
                table: "role_permission",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_permission_role_id",
                schema: "acl",
                table: "role_permission",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_branch_id",
                schema: "Base",
                table: "user",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_branch_id",
                schema: "acl",
                table: "user_role",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_rec_by_id",
                schema: "acl",
                table: "user_role",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_role_id",
                schema: "acl",
                table: "user_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_user_id",
                schema: "acl",
                table: "user_role",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBranchTransfer",
                schema: "Base");

            migrationBuilder.DropTable(
                name: "organization_info",
                schema: "Base");

            migrationBuilder.DropTable(
                name: "role_permission",
                schema: "acl");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "acl");

            migrationBuilder.DropTable(
                name: "role",
                schema: "acl");

            migrationBuilder.DropTable(
                name: "user",
                schema: "Base");

            migrationBuilder.DropTable(
                name: "branch",
                schema: "Base");
        }
    }
}
