using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Chabloom.Payments.Data.Migrations.ApplicationDb
{
    public partial class ApplicationDbMigration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationRoles",
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
                    table.PrimaryKey("PK_ApplicationRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
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
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
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
                    table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationUsers_ApplicationRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "ApplicationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
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
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantRoles",
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
                    table.PrimaryKey("PK_TenantRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantRoles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountRoles",
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
                    table.PrimaryKey("PK_AccountRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountRoles_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    Currency = table.Column<string>(maxLength: 255, nullable: false),
                    Day = table.Column<int>(nullable: false),
                    MonthInterval = table.Column<int>(nullable: false),
                    BeginDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    TransactionScheduleId = table.Column<Guid>(nullable: false),
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
                    table.PrimaryKey("PK_PaymentSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentSchedules_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUsers",
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
                    table.PrimaryKey("PK_TenantUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantUsers_TenantRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "TenantRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantUsers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountUsers",
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
                    table.PrimaryKey("PK_AccountUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountUsers_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountUsers_AccountRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AccountRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    Currency = table.Column<string>(maxLength: 255, nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    Complete = table.Column<bool>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    PaymentScheduleId = table.Column<Guid>(nullable: true),
                    TransactionId = table.Column<Guid>(nullable: false),
                    TransactionScheduleId = table.Column<Guid>(nullable: false),
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
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentSchedules_PaymentScheduleId",
                        column: x => x.PaymentScheduleId,
                        principalTable: "PaymentSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountRoles_AccountId",
                table: "AccountRoles",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TenantId",
                table: "Accounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountUsers_AccountId",
                table: "AccountUsers",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountUsers_RoleId",
                table: "AccountUsers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_RoleId",
                table: "ApplicationUsers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AccountId",
                table: "Payments",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentScheduleId",
                table: "Payments",
                column: "PaymentScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedules_AccountId",
                table: "PaymentSchedules",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantRoles_TenantId",
                table: "TenantRoles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUsers_RoleId",
                table: "TenantUsers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUsers_TenantId",
                table: "TenantUsers",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountUsers");

            migrationBuilder.DropTable(
                name: "ApplicationUsers");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "TenantUsers");

            migrationBuilder.DropTable(
                name: "AccountRoles");

            migrationBuilder.DropTable(
                name: "ApplicationRoles");

            migrationBuilder.DropTable(
                name: "PaymentSchedules");

            migrationBuilder.DropTable(
                name: "TenantRoles");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
