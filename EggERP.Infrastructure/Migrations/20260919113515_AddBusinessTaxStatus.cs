using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EggERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessTaxStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PercentageTaxOption",
                table: "Businesses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxStatus",
                table: "Businesses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PercentageTaxOption",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "TaxStatus",
                table: "Businesses");
        }
    }
}
