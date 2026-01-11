using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusEats.Api.Migrations
{
    /// <inheritdoc />
    public partial class RecreateRewardsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "loyalty_rewards");

            migrationBuilder.DropColumn(
                name: "ExpiryDays",
                table: "loyalty_rewards");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "loyalty_rewards");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "loyalty_rewards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExpiryDays",
                table: "loyalty_rewards",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "loyalty_rewards",
                type: "text",
                nullable: true);
        }
    }
}
