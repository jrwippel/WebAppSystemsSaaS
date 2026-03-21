using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClockTrack.Migrations
{
    /// <inheritdoc />
    public partial class ValorClienteAttorneyNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove FK existente
            migrationBuilder.DropForeignKey(
                name: "FK_ValorCliente_Attorney_AttorneyId",
                table: "ValorCliente");

            // Remove índice existente
            migrationBuilder.DropIndex(
                name: "IX_ValorCliente_AttorneyId",
                table: "ValorCliente");

            // Torna a coluna nullable
            migrationBuilder.AlterColumn<int>(
                name: "AttorneyId",
                table: "ValorCliente",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Recria índice
            migrationBuilder.CreateIndex(
                name: "IX_ValorCliente_AttorneyId",
                table: "ValorCliente",
                column: "AttorneyId");

            // Recria FK com SetNull
            migrationBuilder.AddForeignKey(
                name: "FK_ValorCliente_Attorney_AttorneyId",
                table: "ValorCliente",
                column: "AttorneyId",
                principalTable: "Attorney",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ValorCliente_Attorney_AttorneyId",
                table: "ValorCliente");

            migrationBuilder.DropIndex(
                name: "IX_ValorCliente_AttorneyId",
                table: "ValorCliente");

            migrationBuilder.AlterColumn<int>(
                name: "AttorneyId",
                table: "ValorCliente",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValorCliente_AttorneyId",
                table: "ValorCliente",
                column: "AttorneyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ValorCliente_Attorney_AttorneyId",
                table: "ValorCliente",
                column: "AttorneyId",
                principalTable: "Attorney",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
