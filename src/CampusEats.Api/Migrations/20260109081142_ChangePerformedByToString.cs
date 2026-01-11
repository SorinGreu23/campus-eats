using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusEats.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangePerformedByToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "inventory_transactions",
                type: "text",
                nullable: false,
                oldClrType: typeof(System.Guid),
                oldType: "uuid",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<System.Guid>(
                name: "PerformedBy",
                table: "inventory_transactions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
