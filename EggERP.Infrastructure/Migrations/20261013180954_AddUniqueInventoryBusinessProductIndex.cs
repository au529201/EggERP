using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EggERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueInventoryBusinessProductIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_Inventories_BusinessId_ProductId",
                table: "Inventories",
                columns: new[] { "BusinessId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Inventories_BusinessId_ProductId",
                table: "Inventories");
        }
    }
}
