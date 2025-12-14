using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusEats.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderTypeToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SpecialInstructions",
                table: "orders",
                newName: "DeliveryInstructions"
            );

            migrationBuilder.AddColumn<string>(
                name: "order_type",
                table: "orders",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "order_type", table: "orders");

            migrationBuilder.RenameColumn(
                name: "DeliveryInstructions",
                table: "orders",
                newName: "SpecialInstructions"
            );
        }
    }
}
