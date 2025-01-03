using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taxi_App.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleGarage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WeightCargo",
                table: "Vehicles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Vehicles",
                type: "int",
                nullable: true,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GarageId",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_GarageId",
                table: "Vehicles",
                column: "GarageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Points_GarageId",
                table: "Vehicles",
                column: "GarageId",
                principalTable: "Points",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Points_GarageId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_GarageId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "GarageId",
                table: "Vehicles");

            migrationBuilder.AlterColumn<int>(
                name: "WeightCargo",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 0);
        }
    }
}
