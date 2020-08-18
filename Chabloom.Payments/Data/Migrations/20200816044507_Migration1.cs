using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Chabloom.Payments.Data.Migrations
{
    public partial class Migration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentsAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    PrimaryAddress = table.Column<string>(nullable: false),
                    ExternalId = table.Column<string>(nullable: false),
                    Owner = table.Column<string>(nullable: false),
                    CreatedUser = table.Column<string>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsBillSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DayDue = table.Column<int>(nullable: false),
                    MonthInterval = table.Column<int>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<string>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsBillSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsBillSchedules_PaymentsAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "PaymentsAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsBills",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    BillScheduleId = table.Column<Guid>(nullable: true),
                    CreatedUser = table.Column<string>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsBills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsBills_PaymentsAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "PaymentsAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentsBills_PaymentsBillSchedules_BillScheduleId",
                        column: x => x.BillScheduleId,
                        principalTable: "PaymentsBillSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    ExternalId = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    BillId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<string>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsTransactions_PaymentsBills_BillId",
                        column: x => x.BillId,
                        principalTable: "PaymentsBills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsBills_AccountId",
                table: "PaymentsBills",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsBills_BillScheduleId",
                table: "PaymentsBills",
                column: "BillScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsBillSchedules_AccountId",
                table: "PaymentsBillSchedules",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsTransactions_BillId",
                table: "PaymentsTransactions",
                column: "BillId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentsTransactions");

            migrationBuilder.DropTable(
                name: "PaymentsBills");

            migrationBuilder.DropTable(
                name: "PaymentsBillSchedules");

            migrationBuilder.DropTable(
                name: "PaymentsAccounts");
        }
    }
}
