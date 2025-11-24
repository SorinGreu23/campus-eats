using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusEats.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyaltyClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoyaltyClaim",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoyaltyAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RewardId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyClaim_loyalty_accounts_LoyaltyAccountId",
                        column: x => x.LoyaltyAccountId,
                        principalTable: "loyalty_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoyaltyClaim_loyalty_rewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "loyalty_rewards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyClaim_LoyaltyAccountId",
                table: "LoyaltyClaim",
                column: "LoyaltyAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyClaim_RewardId",
                table: "LoyaltyClaim",
                column: "RewardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoyaltyClaim");
        }
    }
}
