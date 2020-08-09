using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Payments.Backend.Migrations
{
    public partial class _1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    PrimaryAddress = table.Column<string>(nullable: false),
                    ExternalId = table.Column<string>(nullable: false),
                    CreatedUser = table.Column<string>(nullable: true),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: true),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    Owner = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DayDue = table.Column<int>(nullable: false),
                    MonthInterval = table.Column<int>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    CreatedUser = table.Column<string>(nullable: true),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: true),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillSchedules_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    CreatedUser = table.Column<string>(nullable: true),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: true),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    BillScheduleId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bills_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bills_BillSchedules_BillScheduleId",
                        column: x => x.BillScheduleId,
                        principalTable: "BillSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    ExternalId = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    BillId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<string>(nullable: true),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bills_AccountId",
                table: "Bills",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_BillScheduleId",
                table: "Bills",
                column: "BillScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_BillSchedules_AccountId",
                table: "BillSchedules",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BillId",
                table: "Transactions",
                column: "BillId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Bills");

            migrationBuilder.DropTable(
                name: "BillSchedules");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
