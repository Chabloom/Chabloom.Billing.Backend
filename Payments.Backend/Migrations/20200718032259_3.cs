using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payments.Backend.Migrations
{
    public partial class _3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "PaymentsTransactions");

            migrationBuilder.DropColumn(
                name: "Completed",
                table: "PaymentsBills");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "PaymentsAccounts");

            migrationBuilder.DropColumn(
                name: "DayDue",
                table: "PaymentsAccounts");

            migrationBuilder.DropColumn(
                name: "Interval",
                table: "PaymentsAccounts");

            migrationBuilder.AlterColumn<int>(
                name: "Amount",
                table: "PaymentsTransactions",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceId",
                table: "PaymentsTransactions",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Amount",
                table: "PaymentsBills",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PaymentsPaymentSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Amount = table.Column<int>(nullable: false),
                    DayDue = table.Column<int>(nullable: false),
                    MonthInterval = table.Column<int>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsPaymentSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsPaymentSchedules_PaymentsAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "PaymentsAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsPaymentSchedules_AccountId",
                table: "PaymentsPaymentSchedules",
                column: "AccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentsPaymentSchedules");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "PaymentsTransactions");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "PaymentsBills");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "PaymentsTransactions",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "PaymentsTransactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Completed",
                table: "PaymentsBills",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "PaymentsAccounts",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DayDue",
                table: "PaymentsAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Interval",
                table: "PaymentsAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
