using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClockTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddMercadoPagoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MercadoPagoSubscriptionId",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanoAtual",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MercadoPagoSubscriptionId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "PlanoAtual",
                table: "Tenants");
        }
    }
}
