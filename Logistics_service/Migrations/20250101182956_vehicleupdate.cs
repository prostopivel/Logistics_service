using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taxi_App.Migrations
{
    /// <inheritdoc />
    public partial class vehicleupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "WeightCargo",
                table: "Vehicles");

            migrationBuilder.AddColumn<int>(
                name: "Speed",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Speed",
                table: "Vehicles");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Vehicles",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WeightCargo",
                table: "Vehicles",
                type: "int",
                nullable: true);
        }
    }
}
