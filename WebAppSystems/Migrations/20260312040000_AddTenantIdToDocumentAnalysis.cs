using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToDocumentAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Adicionar coluna TenantId (permitindo NULL temporariamente)
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "DocumentAnalysis",
                type: "int",
                nullable: true);

            // 2. Atualizar registros existentes com TenantId = 1 (tenant padrão)
            migrationBuilder.Sql("UPDATE DocumentAnalysis SET TenantId = 1 WHERE TenantId IS NULL");

            // 3. Tornar a coluna NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "DocumentAnalysis",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 4. Criar índice
            migrationBuilder.CreateIndex(
                name: "IX_DocumentAnalysis_TenantId",
                table: "DocumentAnalysis",
                column: "TenantId");

            // 5. Adicionar Foreign Key
            migrationBuilder.AddForeignKey(
                name: "FK_DocumentAnalysis_Tenants_TenantId",
                table: "DocumentAnalysis",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentAnalysis_Tenants_TenantId",
                table: "DocumentAnalysis");

            migrationBuilder.DropIndex(
                name: "IX_DocumentAnalysis_TenantId",
                table: "DocumentAnalysis");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DocumentAnalysis");
        }
    }
}
