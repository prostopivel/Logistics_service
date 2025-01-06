using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics_service.Migrations
{
    /// <inheritdoc />
    public partial class CustomerOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "ReadyOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "ReadyOrders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
