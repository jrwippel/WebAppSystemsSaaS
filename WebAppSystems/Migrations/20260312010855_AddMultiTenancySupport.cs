using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class AddMultiTenancySupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ProcessRecord",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Department",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Client",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Attorney",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subdomain = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Document = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubscriptionExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxUsers = table.Column<int>(type: "int", nullable: false),
                    MaxClients = table.Column<int>(type: "int", nullable: false),
                    MaxStorageMB = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRecord_TenantId",
                table: "ProcessRecord",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Department_TenantId",
                table: "Department",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Client_TenantId",
                table: "Client",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Attorney_TenantId",
                table: "Attorney",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Subdomain",
                table: "Tenants",
                column: "Subdomain",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attorney_Tenants_TenantId",
                table: "Attorney",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Client_Tenants_TenantId",
                table: "Client",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Tenants_TenantId",
                table: "Department",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessRecord_Tenants_TenantId",
                table: "ProcessRecord",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attorney_Tenants_TenantId",
                table: "Attorney");

            migrationBuilder.DropForeignKey(
                name: "FK_Client_Tenants_TenantId",
                table: "Client");

            migrationBuilder.DropForeignKey(
                name: "FK_Department_Tenants_TenantId",
                table: "Department");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcessRecord_Tenants_TenantId",
                table: "ProcessRecord");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_ProcessRecord_TenantId",
                table: "ProcessRecord");

            migrationBuilder.DropIndex(
                name: "IX_Department_TenantId",
                table: "Department");

            migrationBuilder.DropIndex(
                name: "IX_Client_TenantId",
                table: "Client");

            migrationBuilder.DropIndex(
                name: "IX_Attorney_TenantId",
                table: "Attorney");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProcessRecord");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Attorney");
        }
    }
}
