using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    /// <inheritdoc />
    public partial class ConvertRecordTypeToActivityType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Criar tabela ActivityTypes
            migrationBuilder.CreateTable(
                name: "ActivityTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityTypes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTypes_TenantId",
                table: "ActivityTypes",
                column: "TenantId");

            // 2. (seed removido - tipos de atividade são criados pelo próprio tenant)

            // 3. Adicionar coluna ActivityTypeId em ProcessRecord (permitindo NULL temporariamente)
            migrationBuilder.AddColumn<int>(
                name: "ActivityTypeId",
                table: "ProcessRecord",
                type: "int",
                nullable: true);

            // 4. (migração de RecordType removida - não aplicável em novos deploys)

            // 5. Tornar ActivityTypeId NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "ActivityTypeId",
                table: "ProcessRecord",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 6. Criar índice e FK
            migrationBuilder.CreateIndex(
                name: "IX_ProcessRecord_ActivityTypeId",
                table: "ProcessRecord",
                column: "ActivityTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessRecord_ActivityTypes_ActivityTypeId",
                table: "ProcessRecord",
                column: "ActivityTypeId",
                principalTable: "ActivityTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 7. Remover coluna RecordType antiga
            migrationBuilder.DropColumn(
                name: "RecordType",
                table: "ProcessRecord");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverter mudanças
            migrationBuilder.DropForeignKey(
                name: "FK_ProcessRecord_ActivityTypes_ActivityTypeId",
                table: "ProcessRecord");

            migrationBuilder.DropIndex(
                name: "IX_ProcessRecord_ActivityTypeId",
                table: "ProcessRecord");

            // Recriar coluna RecordType
            migrationBuilder.AddColumn<int>(
                name: "RecordType",
                table: "ProcessRecord",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Remover ActivityTypeId
            migrationBuilder.DropColumn(
                name: "ActivityTypeId",
                table: "ProcessRecord");

            // Remover tabela ActivityTypes
            migrationBuilder.DropTable(
                name: "ActivityTypes");
        }
    }
}
