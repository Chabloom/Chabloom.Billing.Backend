using Microsoft.EntityFrameworkCore.Migrations;

namespace Chabloom.Billing.Backend.Data.Migrations.Application
{
    public partial class ApplicationMigration3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_TenantId",
                table: "Accounts");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Accounts_TenantId_ReferenceId",
                table: "Accounts",
                columns: new[] { "TenantId", "ReferenceId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Accounts_TenantId_ReferenceId",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TenantId",
                table: "Accounts",
                column: "TenantId");
        }
    }
}
