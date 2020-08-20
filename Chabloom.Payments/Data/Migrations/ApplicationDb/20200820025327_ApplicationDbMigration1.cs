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
                    CreatedUser = table.Column<string>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false)
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
                    CreatedUser = table.Column<string>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false)
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
                    CreatedUser = table.Column<string>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false)
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
                name: "PaymentsTenantUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<string>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsTenantUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsTenantUsers_PaymentsTenants_TenantId",
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
                    CreatedUser = table.Column<string>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false)
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
                name: "PaymentsAccountUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<string>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "PaymentsBillSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DayDue = table.Column<int>(nullable: false),
                    Interval = table.Column<int>(nullable: false),
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
                name: "PaymentsTenantUserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<string>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsTenantUserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsTenantUserRoles_PaymentsTenantRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "PaymentsTenantRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentsTenantUserRoles_PaymentsTenantUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "PaymentsTenantUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsAccountUserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false),
                    CreatedUser = table.Column<string>(nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedUser = table.Column<string>(nullable: false),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsAccountUserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsAccountUserRoles_PaymentsAccountRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "PaymentsAccountRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentsAccountUserRoles_PaymentsAccountUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "PaymentsAccountUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsBillTransactions",
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
                    table.PrimaryKey("PK_PaymentsBillTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsBillTransactions_PaymentsBills_BillId",
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
                name: "IX_PaymentsAccountUserRoles_RoleId",
                table: "PaymentsAccountUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsAccountUserRoles_UserId",
                table: "PaymentsAccountUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsAccountUsers_AccountId",
                table: "PaymentsAccountUsers",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsBills_AccountId",
                table: "PaymentsBills",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsBillSchedules_AccountId",
                table: "PaymentsBillSchedules",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsBillTransactions_BillId",
                table: "PaymentsBillTransactions",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsTenantRoles_TenantId",
                table: "PaymentsTenantRoles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsTenantUserRoles_RoleId",
                table: "PaymentsTenantUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsTenantUserRoles_UserId",
                table: "PaymentsTenantUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsTenantUsers_TenantId",
                table: "PaymentsTenantUsers",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentsAccountUserRoles");

            migrationBuilder.DropTable(
                name: "PaymentsBillSchedules");

            migrationBuilder.DropTable(
                name: "PaymentsBillTransactions");

            migrationBuilder.DropTable(
                name: "PaymentsTenantUserRoles");

            migrationBuilder.DropTable(
                name: "PaymentsAccountRoles");

            migrationBuilder.DropTable(
                name: "PaymentsAccountUsers");

            migrationBuilder.DropTable(
                name: "PaymentsBills");

            migrationBuilder.DropTable(
                name: "PaymentsTenantRoles");

            migrationBuilder.DropTable(
                name: "PaymentsTenantUsers");

            migrationBuilder.DropTable(
                name: "PaymentsAccounts");

            migrationBuilder.DropTable(
                name: "PaymentsTenants");
        }
    }
}
