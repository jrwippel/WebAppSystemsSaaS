using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClockTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddAliquotaTributosToParametros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AliquotaTributos",
                table: "Parametros",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AliquotaTributos",
                table: "Parametros");
        }
    }
}
