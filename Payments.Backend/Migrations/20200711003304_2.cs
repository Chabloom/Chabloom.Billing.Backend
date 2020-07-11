using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payments.Backend.Migrations
{
    public partial class _2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentsAccounts_PaymentsPaymentSchedules_PaymentScheduleId",
                table: "PaymentsAccounts");

            migrationBuilder.DropTable(
                name: "PaymentsPaymentSchedules");

            migrationBuilder.DropIndex(
                name: "IX_PaymentsAccounts_PaymentScheduleId",
                table: "PaymentsAccounts");

            migrationBuilder.DropColumn(
                name: "PaymentScheduleId",
                table: "PaymentsAccounts");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "PaymentsAccounts",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DayDue",
                table: "PaymentsAccounts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Interval",
                table: "PaymentsAccounts",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "PaymentsAccounts");

            migrationBuilder.DropColumn(
                name: "DayDue",
                table: "PaymentsAccounts");

            migrationBuilder.DropColumn(
                name: "Interval",
                table: "PaymentsAccounts");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentScheduleId",
                table: "PaymentsAccounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "PaymentsPaymentSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DayDue = table.Column<int>(type: "int", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsPaymentSchedules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsAccounts_PaymentScheduleId",
                table: "PaymentsAccounts",
                column: "PaymentScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentsAccounts_PaymentsPaymentSchedules_PaymentScheduleId",
                table: "PaymentsAccounts",
                column: "PaymentScheduleId",
                principalTable: "PaymentsPaymentSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
