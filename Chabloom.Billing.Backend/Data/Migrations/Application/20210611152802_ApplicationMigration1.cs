using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Chabloom.Billing.Backend.Data.Migrations.Application
{
    public partial class ApplicationMigration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantLookupId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUser = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUser = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DisabledUser = table.Column<Guid>(type: "uuid", nullable: true),
                    DisabledTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.UniqueConstraint("AK_Accounts_TenantId_TenantLookupId", x => new { x.TenantId, x.TenantLookupId });
                    table.ForeignKey(
                        name: "FK_Accounts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantHosts",
                columns: table => new
                {
                    Hostname = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantHosts", x => x.Hostname);
                    table.ForeignKey(
                        name: "FK_TenantHosts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantRoles", x => x.Id);
                    table.UniqueConstraint("AK_TenantRoles_NormalizedName_TenantId", x => new { x.NormalizedName, x.TenantId });
                    table.ForeignKey(
                        name: "FK_TenantRoles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUsers", x => x.Id);
                    table.UniqueConstraint("AK_TenantUsers_NormalizedUserName_TenantId", x => new { x.NormalizedUserName, x.TenantId });
                    table.ForeignKey(
                        name: "FK_TenantUsers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CurrencyId = table.Column<string>(type: "text", nullable: false),
                    PaymentId = table.Column<string>(type: "text", nullable: true),
                    PaymentScheduleId = table.Column<string>(type: "text", nullable: true),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUser = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUser = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DisabledUser = table.Column<Guid>(type: "uuid", nullable: true),
                    DisabledTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "BillSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CurrencyId = table.Column<string>(type: "text", nullable: false),
                    PaymentScheduleId = table.Column<string>(type: "text", nullable: true),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    MonthInterval = table.Column<int>(type: "integer", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false)
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
                name: "TenantRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantRoleClaims_TenantRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "TenantRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantUserClaims_TenantUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "TenantUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_TenantUserLogins_TenantUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "TenantUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_TenantUserRoles_TenantRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "TenantRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantUserRoles_TenantUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "TenantUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_TenantUserTokens_TenantUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "TenantUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => new { x.UserId, x.AccountId });
                    table.ForeignKey(
                        name: "FK_UserAccounts_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAccounts_TenantUsers_TenantUserId",
                        column: x => x.TenantUserId,
                        principalTable: "TenantUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("a2cee23f-3250-4b1b-93dc-87443b02dd89"), "North Augusta" },
                    { new Guid("7ba3a979-5abf-407f-aee1-75e2d5522711"), "Aiken" }
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "Address", "CreatedTimestamp", "CreatedUser", "DisabledTimestamp", "DisabledUser", "Name", "TenantId", "TenantLookupId", "UpdatedTimestamp", "UpdatedUser" },
                values: new object[,]
                {
                    { new Guid("0c994055-dfe2-4863-a087-2b47cd2a1e25"), "400 W Martintown Rd", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "400 W Martintown Rd", new Guid("a2cee23f-3250-4b1b-93dc-87443b02dd89"), "12345", null, null },
                    { new Guid("c68ddf06-20f6-41f0-869a-6f39cd8d9431"), "402 W Martintown Rd", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "402 W Martintown Rd", new Guid("a2cee23f-3250-4b1b-93dc-87443b02dd89"), "12346", null, null },
                    { new Guid("5a79115e-2e5a-4fca-9ea1-e6a21e7de4d4"), "403 W Martintown Rd", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "403 W Martintown Rd", new Guid("a2cee23f-3250-4b1b-93dc-87443b02dd89"), "12347", null, null },
                    { new Guid("1ba99ecc-48d3-4839-b748-7565e2a72f77"), "404 W Martintown Rd", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "404 W Martintown Rd", new Guid("a2cee23f-3250-4b1b-93dc-87443b02dd89"), "12348", null, null },
                    { new Guid("31a59e75-197f-433c-b6b4-6160c6cace42"), "400 Richland Ave", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "400 Richland Ave", new Guid("7ba3a979-5abf-407f-aee1-75e2d5522711"), "12345", null, null },
                    { new Guid("d1eaffb6-5757-4fca-b15f-04ec11073275"), "402 Richland Ave", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "402 Richland Ave", new Guid("7ba3a979-5abf-407f-aee1-75e2d5522711"), "12346", null, null },
                    { new Guid("8a276669-64f9-4057-a741-10b6b5ff92b0"), "403 Richland Ave", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "403 Richland Ave", new Guid("7ba3a979-5abf-407f-aee1-75e2d5522711"), "12347", null, null },
                    { new Guid("e89ee340-96d2-49b2-a7c9-f444fb95c148"), "404 Richland Ave", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "404 Richland Ave", new Guid("7ba3a979-5abf-407f-aee1-75e2d5522711"), "12348", null, null }
                });

            migrationBuilder.InsertData(
                table: "TenantRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName", "TenantId" },
                values: new object[,]
                {
                    { new Guid("99a3b0a6-0adb-4fee-b2cc-380ee21ea446"), "c4558764-0f55-4538-ae3a-ad6c8f8124dc", "Admin", "ADMIN", new Guid("a2cee23f-3250-4b1b-93dc-87443b02dd89") },
                    { new Guid("f94c10f9-69dd-459f-a2fe-3be09c2c4075"), "127fb553-75cd-4c9a-932f-b5b036d40505", "Manager", "MANAGER", new Guid("a2cee23f-3250-4b1b-93dc-87443b02dd89") },
                    { new Guid("191e5e91-0e14-460e-a481-2f00c72b8228"), "e9da379a-ccf0-4209-9460-555d013831b1", "Admin", "ADMIN", new Guid("7ba3a979-5abf-407f-aee1-75e2d5522711") },
                    { new Guid("52c71ae1-9b6b-4694-9ea8-e70501a8aca2"), "6bad7203-2f73-4ba7-92b8-98ee8ad95f3f", "Manager", "MANAGER", new Guid("7ba3a979-5abf-407f-aee1-75e2d5522711") }
                });

            migrationBuilder.InsertData(
                table: "TenantUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TenantId", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { new Guid("39332843-d6dd-425f-8c5d-3ec565857059"), 0, "893D41CD-4642-4542-B81C-E8368FA03906", "mdcasey@chabloom.com", true, false, null, "MDCASEY@CHABLOOM.COM", "MDCASEY@CHABLOOM.COM", "AQAAAAEAACcQAAAAELYyWQtU3cVbIfdmk4LHrtYsKTiYVW7OAge27lolZ3I8D97OE4QQ6Yn4XwGhO8YPuQ==", "+18036179564", true, "C3KZM3I2WQCCAD7EVHRZQSGRFRX5MY3I", new Guid("a2cee23f-3250-4b1b-93dc-87443b02dd89"), false, "mdcasey@chabloom.com" },
                    { new Guid("dfb1359f-a11d-4de1-b8ac-ada45aef0b72"), 0, "CB0FCA5D-D2F3-4D0B-8277-2091FED613B3", "mdcasey@chabloom.com", true, false, null, "MDCASEY@CHABLOOM.COM", "MDCASEY@CHABLOOM.COM", "AQAAAAEAACcQAAAAELYyWQtU3cVbIfdmk4LHrtYsKTiYVW7OAge27lolZ3I8D97OE4QQ6Yn4XwGhO8YPuQ==", "+18036179564", true, "C3KZM3I2WQCCAD7EVHRZQSGRFRX5MY3I", new Guid("7ba3a979-5abf-407f-aee1-75e2d5522711"), false, "mdcasey@chabloom.com" }
                });

            migrationBuilder.InsertData(
                table: "BillSchedules",
                columns: new[] { "Id", "AccountId", "Amount", "CurrencyId", "Day", "MonthInterval", "Name", "PaymentScheduleId" },
                values: new object[,]
                {
                    { new Guid("cf28d12e-e51a-491c-ada8-cd6fcf840e88"), new Guid("e89ee340-96d2-49b2-a7c9-f444fb95c148"), 3350m, "USD", 1, 1, "Monthly Water", null },
                    { new Guid("be05ae00-081d-48d4-b047-e93fb7523934"), new Guid("0c994055-dfe2-4863-a087-2b47cd2a1e25"), 3350m, "USD", 1, 1, "Monthly Water", null },
                    { new Guid("bfb53864-8389-4038-9b49-60bbb1358158"), new Guid("8a276669-64f9-4057-a741-10b6b5ff92b0"), 3350m, "USD", 1, 1, "Monthly Water", null },
                    { new Guid("298aa43a-6376-4049-ae6e-6c0d1741fcc4"), new Guid("c68ddf06-20f6-41f0-869a-6f39cd8d9431"), 3350m, "USD", 1, 1, "Monthly Water", null },
                    { new Guid("d28f8ec7-e716-4d5a-b0c4-832d9a1134d5"), new Guid("5a79115e-2e5a-4fca-9ea1-e6a21e7de4d4"), 3350m, "USD", 1, 1, "Monthly Water", null },
                    { new Guid("35a5cc23-e120-48fe-afda-d42c26233193"), new Guid("d1eaffb6-5757-4fca-b15f-04ec11073275"), 3350m, "USD", 1, 1, "Monthly Water", null },
                    { new Guid("38fbb0fe-b1b3-42a9-b591-88c37381e5bc"), new Guid("1ba99ecc-48d3-4839-b748-7565e2a72f77"), 3350m, "USD", 1, 1, "Monthly Water", null },
                    { new Guid("600d7c6f-6b6a-4a0c-a62e-c5ed482ebc09"), new Guid("31a59e75-197f-433c-b6b4-6160c6cace42"), 3350m, "USD", 1, 1, "Monthly Water", null }
                });

            migrationBuilder.InsertData(
                table: "Bills",
                columns: new[] { "Id", "AccountId", "Amount", "CreatedTimestamp", "CreatedUser", "CurrencyId", "DisabledTimestamp", "DisabledUser", "DueDate", "Name", "PaymentId", "PaymentScheduleId", "UpdatedTimestamp", "UpdatedUser" },
                values: new object[,]
                {
                    { new Guid("61ce9459-55aa-452e-bd29-5e31913e42ff"), new Guid("8a276669-64f9-4057-a741-10b6b5ff92b0"), 7622m, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "USD", null, null, new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "August 2021", null, null, null, null },
                    { new Guid("b0eb88dc-c8ea-45c2-80a7-8abf5978cee5"), new Guid("d1eaffb6-5757-4fca-b15f-04ec11073275"), 4252m, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "USD", null, null, new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "August 2021", null, null, null, null },
                    { new Guid("7690274e-c4ad-4964-9cc5-3625ccaabcca"), new Guid("0c994055-dfe2-4863-a087-2b47cd2a1e25"), 4329m, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "USD", null, null, new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "August 2021", null, null, null, null },
                    { new Guid("27bc4cdc-e4f8-43df-a23d-4136d371254b"), new Guid("e89ee340-96d2-49b2-a7c9-f444fb95c148"), 3688m, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "USD", null, null, new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "August 2021", null, null, null, null },
                    { new Guid("6eec413f-f55e-4dac-8a6d-2f38e5a04fe7"), new Guid("1ba99ecc-48d3-4839-b748-7565e2a72f77"), 4591m, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "USD", null, null, new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "August 2021", null, null, null, null },
                    { new Guid("8240327d-547a-4875-92c8-b1dcfa5f13a9"), new Guid("5a79115e-2e5a-4fca-9ea1-e6a21e7de4d4"), 6123m, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "USD", null, null, new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "August 2021", null, null, null, null },
                    { new Guid("8be7e620-7f46-4460-a49a-de09b8895640"), new Guid("c68ddf06-20f6-41f0-869a-6f39cd8d9431"), 5521m, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "USD", null, null, new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "August 2021", null, null, null, null },
                    { new Guid("eb54f672-a3aa-458c-8b4d-18cacae438d6"), new Guid("31a59e75-197f-433c-b6b4-6160c6cace42"), 3371m, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("00000000-0000-0000-0000-000000000000"), "USD", null, null, new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "August 2021", null, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "TenantUserClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "UserId" },
                values: new object[,]
                {
                    { 2, "name", "Matthew Casey", new Guid("dfb1359f-a11d-4de1-b8ac-ada45aef0b72") },
                    { 1, "name", "Matthew Casey", new Guid("39332843-d6dd-425f-8c5d-3ec565857059") }
                });

            migrationBuilder.InsertData(
                table: "TenantUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { new Guid("99a3b0a6-0adb-4fee-b2cc-380ee21ea446"), new Guid("39332843-d6dd-425f-8c5d-3ec565857059") },
                    { new Guid("191e5e91-0e14-460e-a481-2f00c72b8228"), new Guid("dfb1359f-a11d-4de1-b8ac-ada45aef0b72") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bills_AccountId",
                table: "Bills",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BillSchedules_AccountId",
                table: "BillSchedules",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantHosts_TenantId",
                table: "TenantHosts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantRoleClaims_RoleId",
                table: "TenantRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantRoles_TenantId",
                table: "TenantRoles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "TenantRoles",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUserClaims_UserId",
                table: "TenantUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUserLogins_UserId",
                table: "TenantUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUserRoles_RoleId",
                table: "TenantUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "TenantUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUsers_TenantId",
                table: "TenantUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "TenantUsers",
                column: "NormalizedUserName");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_AccountId",
                table: "UserAccounts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_TenantUserId",
                table: "UserAccounts",
                column: "TenantUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bills");

            migrationBuilder.DropTable(
                name: "BillSchedules");

            migrationBuilder.DropTable(
                name: "TenantHosts");

            migrationBuilder.DropTable(
                name: "TenantRoleClaims");

            migrationBuilder.DropTable(
                name: "TenantUserClaims");

            migrationBuilder.DropTable(
                name: "TenantUserLogins");

            migrationBuilder.DropTable(
                name: "TenantUserRoles");

            migrationBuilder.DropTable(
                name: "TenantUserTokens");

            migrationBuilder.DropTable(
                name: "UserAccounts");

            migrationBuilder.DropTable(
                name: "TenantRoles");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "TenantUsers");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
