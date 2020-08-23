using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Chabloom.Payments.Data.Migrations.ApplicationDb
{
    public partial class ApplicationDbMigration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentsTenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    CreatedUser = table.Column<Guid>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<Guid>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    Disabled = table.Column<bool>(nullable: false),
                    DisabledUser = table.Column<Guid>(nullable: false),
                    DisabledTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsTenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    ExternalId = table.Column<string>(nullable: false),
                    PrimaryAddress = table.Column<string>(nullable: false),
                    TenantId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<Guid>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<Guid>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    Disabled = table.Column<bool>(nullable: false),
                    DisabledUser = table.Column<Guid>(nullable: false),
                    DisabledTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsAccounts_PaymentsTenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "PaymentsTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsTenantRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    TenantId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<Guid>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<Guid>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    Disabled = table.Column<bool>(nullable: false),
                    DisabledUser = table.Column<Guid>(nullable: false),
                    DisabledTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsTenantRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsTenantRoles_PaymentsTenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "PaymentsTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsAccountRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<Guid>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<Guid>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    Disabled = table.Column<bool>(nullable: false),
                    DisabledUser = table.Column<Guid>(nullable: false),
                    DisabledTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsAccountRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsAccountRoles_PaymentsAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "PaymentsAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsApplicationRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<Guid>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<Guid>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    Disabled = table.Column<bool>(nullable: false),
                    DisabledUser = table.Column<Guid>(nullable: false),
                    DisabledTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsApplicationRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsApplicationRoles_PaymentsAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "PaymentsAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DayDue = table.Column<int>(nullable: false),
                    Interval = table.Column<int>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<Guid>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<Guid>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    Disabled = table.Column<bool>(nullable: false),
                    DisabledUser = table.Column<Guid>(nullable: false),
                    DisabledTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsSchedules_PaymentsAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "PaymentsAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsTenantUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: true),
                    CreatedUser = table.Column<Guid>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<Guid>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    Disabled = table.Column<bool>(nullable: false),
                    DisabledUser = table.Column<Guid>(nullable: false),
                    DisabledTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsTenantUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsTenantUsers_PaymentsTenantRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "PaymentsTenantRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentsTenantUsers_PaymentsTenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "PaymentsTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsAccountUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: true),
                    CreatedUser = table.Column<Guid>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<Guid>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    Disabled = table.Column<bool>(nullable: false),
                    DisabledUser = table.Column<Guid>(nullable: false),
                    DisabledTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsAccountUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsAccountUsers_PaymentsAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "PaymentsAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentsAccountUsers_PaymentsAccountRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "PaymentsAccountRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsApplicationUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: true),
                    CreatedUser = table.Column<Guid>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<Guid>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    Disabled = table.Column<bool>(nullable: false),
                    DisabledUser = table.Column<Guid>(nullable: false),
                    DisabledTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsApplicationUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsApplicationUsers_PaymentsApplicationRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "PaymentsApplicationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    ScheduleId = table.Column<Guid>(nullable: true),
                    CreatedUser = table.Column<Guid>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<Guid>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    Disabled = table.Column<bool>(nullable: false),
                    DisabledUser = table.Column<Guid>(nullable: false),
                    DisabledTimestamp = table.Column<DateTimeOffset>(nullable: false)
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
                        name: "FK_PaymentsBills_PaymentsSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "PaymentsSchedules",
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
                    CreatedUser = table.Column<Guid>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<Guid>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    Disabled = table.Column<bool>(nullable: false),
                    DisabledUser = table.Column<Guid>(nullable: false),
                    DisabledTimestamp = table.Column<DateTimeOffset>(nullable: false)
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
                name: "IX_PaymentsAccountRoles_AccountId",
                table: "PaymentsAccountRoles",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsAccounts_TenantId",
                table: "PaymentsAccounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsAccountUsers_AccountId",
                table: "PaymentsAccountUsers",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsAccountUsers_RoleId",
                table: "PaymentsAccountUsers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsApplicationRoles_AccountId",
                table: "PaymentsApplicationRoles",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsApplicationUsers_RoleId",
                table: "PaymentsApplicationUsers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsBills_AccountId",
                table: "PaymentsBills",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsBills_ScheduleId",
                table: "PaymentsBills",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsSchedules_AccountId",
                table: "PaymentsSchedules",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsTenantRoles_TenantId",
                table: "PaymentsTenantRoles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsTenantUsers_RoleId",
                table: "PaymentsTenantUsers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsTenantUsers_TenantId",
                table: "PaymentsTenantUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsTransactions_BillId",
                table: "PaymentsTransactions",
                column: "BillId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentsAccountUsers");

            migrationBuilder.DropTable(
                name: "PaymentsApplicationUsers");

            migrationBuilder.DropTable(
                name: "PaymentsTenantUsers");

            migrationBuilder.DropTable(
                name: "PaymentsTransactions");

            migrationBuilder.DropTable(
                name: "PaymentsAccountRoles");

            migrationBuilder.DropTable(
                name: "PaymentsApplicationRoles");

            migrationBuilder.DropTable(
                name: "PaymentsTenantRoles");

            migrationBuilder.DropTable(
                name: "PaymentsBills");

            migrationBuilder.DropTable(
                name: "PaymentsSchedules");

            migrationBuilder.DropTable(
                name: "PaymentsAccounts");

            migrationBuilder.DropTable(
                name: "PaymentsTenants");
        }
    }
}
