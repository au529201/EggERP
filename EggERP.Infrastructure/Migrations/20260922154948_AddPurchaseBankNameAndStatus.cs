using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EggERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseBankNameAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Purchases",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Purchases");
        }
    }
}
