using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EggERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RedesignFlocksAndEggMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EggProductions");

            migrationBuilder.DropColumn(
                name: "CurrentCount",
                table: "Flocks");

            migrationBuilder.DropColumn(
                name: "InitialCount",
                table: "Flocks");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Flocks");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "Flocks",
                newName: "Species");

            migrationBuilder.RenameColumn(
                name: "BirdType",
                table: "Flocks",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "AcquisitionDate",
                table: "Flocks",
                newName: "PlacementDate");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Breed",
                table: "Flocks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CloseDate",
                table: "Flocks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direction",
                table: "FlockMovements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "FlockId",
                table: "Expenses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EggMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EggMovements", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EggMovements");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Breed",
                table: "Flocks");

            migrationBuilder.DropColumn(
                name: "CloseDate",
                table: "Flocks");

            migrationBuilder.DropColumn(
                name: "Direction",
                table: "FlockMovements");

            migrationBuilder.DropColumn(
                name: "FlockId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "Species",
                table: "Flocks",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "PlacementDate",
                table: "Flocks",
                newName: "AcquisitionDate");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Flocks",
                newName: "BirdType");

            migrationBuilder.AddColumn<int>(
                name: "CurrentCount",
                table: "Flocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InitialCount",
                table: "Flocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Flocks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EggProductions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuantityProduced = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EggProductions", x => x.Id);
                });
        }
    }
}
