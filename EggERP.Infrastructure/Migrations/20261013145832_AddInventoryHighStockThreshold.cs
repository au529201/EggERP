using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EggERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryHighStockThreshold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HighStockThreshold",
                table: "Inventories",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighStockThreshold",
                table: "Inventories");
        }
    }
}
