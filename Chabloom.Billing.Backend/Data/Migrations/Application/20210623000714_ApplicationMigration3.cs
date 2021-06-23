using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Chabloom.Billing.Backend.Data.Migrations.Application
{
    public partial class ApplicationMigration3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TenantHosts",
                columns: new[] { "Hostname", "TenantId" },
                values: new object[,]
                {
                    { "billing-dev-1.chabloom.com", new Guid("a2cee23f-3250-4b1b-93dc-87443b02dd89") },
                    { "billing-uat-1.chabloom.com", new Guid("a2cee23f-3250-4b1b-93dc-87443b02dd89") },
                    { "north-augusta.dev-1.chabloom.com", new Guid("a2cee23f-3250-4b1b-93dc-87443b02dd89") },
                    { "north-augusta.uat-1.chabloom.com", new Guid("a2cee23f-3250-4b1b-93dc-87443b02dd89") },
                    { "aiken.dev-1.chabloom.com", new Guid("7ba3a979-5abf-407f-aee1-75e2d5522711") },
                    { "aiken.uat-1.chabloom.com", new Guid("7ba3a979-5abf-407f-aee1-75e2d5522711") }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TenantHosts",
                keyColumn: "Hostname",
                keyValue: "aiken.dev-1.chabloom.com");

            migrationBuilder.DeleteData(
                table: "TenantHosts",
                keyColumn: "Hostname",
                keyValue: "aiken.uat-1.chabloom.com");

            migrationBuilder.DeleteData(
                table: "TenantHosts",
                keyColumn: "Hostname",
                keyValue: "billing-dev-1.chabloom.com");

            migrationBuilder.DeleteData(
                table: "TenantHosts",
                keyColumn: "Hostname",
                keyValue: "billing-uat-1.chabloom.com");

            migrationBuilder.DeleteData(
                table: "TenantHosts",
                keyColumn: "Hostname",
                keyValue: "north-augusta.dev-1.chabloom.com");

            migrationBuilder.DeleteData(
                table: "TenantHosts",
                keyColumn: "Hostname",
                keyValue: "north-augusta.uat-1.chabloom.com");
        }
    }
}
