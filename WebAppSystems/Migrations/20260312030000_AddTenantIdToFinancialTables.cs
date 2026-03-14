using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToFinancialTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== ValorCliente =====
            // 1. Adicionar coluna TenantId (permitindo NULL temporariamente)
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ValorCliente",
                type: "int",
                nullable: true);

            // 2. Atualizar registros existentes com TenantId = 1 (tenant padrão)
            migrationBuilder.Sql("UPDATE ValorCliente SET TenantId = 1 WHERE TenantId IS NULL");

            // 3. Tornar a coluna NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "ValorCliente",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 4. Criar índice
            migrationBuilder.CreateIndex(
                name: "IX_ValorCliente_TenantId",
                table: "ValorCliente",
                column: "TenantId");

            // 5. Adicionar Foreign Key
            migrationBuilder.AddForeignKey(
                name: "FK_ValorCliente_Tenants_TenantId",
                table: "ValorCliente",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ===== Mensalista =====
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Mensalista",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE Mensalista SET TenantId = 1 WHERE TenantId IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "Mensalista",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mensalista_TenantId",
                table: "Mensalista",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mensalista_Tenants_TenantId",
                table: "Mensalista",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ===== PercentualArea =====
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PercentualArea",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE PercentualArea SET TenantId = 1 WHERE TenantId IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "PercentualArea",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PercentualArea_TenantId",
                table: "PercentualArea",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_PercentualArea_Tenants_TenantId",
                table: "PercentualArea",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ValorCliente
            migrationBuilder.DropForeignKey(
                name: "FK_ValorCliente_Tenants_TenantId",
                table: "ValorCliente");

            migrationBuilder.DropIndex(
                name: "IX_ValorCliente_TenantId",
                table: "ValorCliente");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ValorCliente");

            // Mensalista
            migrationBuilder.DropForeignKey(
                name: "FK_Mensalista_Tenants_TenantId",
                table: "Mensalista");

            migrationBuilder.DropIndex(
                name: "IX_Mensalista_TenantId",
                table: "Mensalista");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Mensalista");

            // PercentualArea
            migrationBuilder.DropForeignKey(
                name: "FK_PercentualArea_Tenants_TenantId",
                table: "PercentualArea");

            migrationBuilder.DropIndex(
                name: "IX_PercentualArea_TenantId",
                table: "PercentualArea");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PercentualArea");
        }
    }
}
