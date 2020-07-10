using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payments.Backend.Migrations
{
    public partial class _1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentsPartitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsPartitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsPaymentSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DayDue = table.Column<int>(nullable: false),
                    Interval = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsPaymentSchedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    PrimaryAddress = table.Column<string>(nullable: false),
                    PaymentScheduleId = table.Column<Guid>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    PartitionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsAccounts_PaymentsPartitions_PartitionId",
                        column: x => x.PartitionId,
                        principalTable: "PaymentsPartitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentsAccounts_PaymentsPaymentSchedules_PaymentScheduleId",
                        column: x => x.PaymentScheduleId,
                        principalTable: "PaymentsPaymentSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsBills",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Completed = table.Column<bool>(nullable: false),
                    DueDate = table.Column<DateTime>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "PaymentsTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ExternalId = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(nullable: false),
                    BillId = table.Column<Guid>(nullable: false)
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

            migrationBuilder.InsertData(
                table: "PaymentsPartitions",
                columns: new[] { "Id", "Enabled", "Name" },
                values: new object[] { new Guid("040b9da1-352d-4b81-8e6a-78ecd2ff14d1"), true, "Default" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsAccounts_PartitionId",
                table: "PaymentsAccounts",
                column: "PartitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsAccounts_PaymentScheduleId",
                table: "PaymentsAccounts",
                column: "PaymentScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsBills_AccountId",
                table: "PaymentsBills",
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
                name: "PaymentsAccounts");

            migrationBuilder.DropTable(
                name: "PaymentsPartitions");

            migrationBuilder.DropTable(
                name: "PaymentsPaymentSchedules");
        }
    }
}
