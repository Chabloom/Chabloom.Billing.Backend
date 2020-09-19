using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Chabloom.Payments.Data.Migrations.ApplicationDb
{
    public partial class ApplicationDbMigration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentsApplicationRoles_PaymentsAccounts_AccountId",
                table: "PaymentsApplicationRoles");

            migrationBuilder.DropIndex(
                name: "IX_PaymentsApplicationRoles_AccountId",
                table: "PaymentsApplicationRoles");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "PaymentsApplicationRoles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "PaymentsApplicationRoles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsApplicationRoles_AccountId",
                table: "PaymentsApplicationRoles",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentsApplicationRoles_PaymentsAccounts_AccountId",
                table: "PaymentsApplicationRoles",
                column: "AccountId",
                principalTable: "PaymentsAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
