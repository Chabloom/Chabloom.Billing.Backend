using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Chabloom.Payments.Data.Migrations.ApplicationDb
{
    public partial class ApplicationDbMigration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentsAccountUserRoles");

            migrationBuilder.DropTable(
                name: "PaymentsTenantUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentsTenantUsers",
                table: "PaymentsTenantUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentsAccountUsers",
                table: "PaymentsAccountUsers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PaymentsTenantUsers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PaymentsAccountUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "PaymentsTenantUsers",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "PaymentsAccountUsers",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentsTenantUsers",
                table: "PaymentsTenantUsers",
                columns: new[] { "UserId", "TenantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentsAccountUsers",
                table: "PaymentsAccountUsers",
                columns: new[] { "UserId", "AccountId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentsTenantUsers",
                table: "PaymentsTenantUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentsAccountUsers",
                table: "PaymentsAccountUsers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PaymentsTenantUsers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PaymentsAccountUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "PaymentsTenantUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "PaymentsAccountUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentsTenantUsers",
                table: "PaymentsTenantUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentsAccountUsers",
                table: "PaymentsAccountUsers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PaymentsAccountUserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsAccountUserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsAccountUserRoles_PaymentsAccountRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "PaymentsAccountRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentsAccountUserRoles_PaymentsAccountUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "PaymentsAccountUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsTenantUserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsTenantUserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsTenantUserRoles_PaymentsTenantRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "PaymentsTenantRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentsTenantUserRoles_PaymentsTenantUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "PaymentsTenantUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsAccountUserRoles_RoleId",
                table: "PaymentsAccountUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsAccountUserRoles_UserId",
                table: "PaymentsAccountUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsTenantUserRoles_RoleId",
                table: "PaymentsTenantUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsTenantUserRoles_UserId",
                table: "PaymentsTenantUserRoles",
                column: "UserId");
        }
    }
}
