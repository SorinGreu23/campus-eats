using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusEats.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixInventoryTransactionRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_inventory_transactions_inventory_items_InventoryItemId",
                table: "inventory_transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_inventory_transactions_inventory_items_InventoryItemId",
                table: "inventory_transactions",
                column: "InventoryItemId",
                principalTable: "inventory_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_inventory_transactions_inventory_items_InventoryItemId",
                table: "inventory_transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_inventory_transactions_inventory_items_InventoryItemId",
                table: "inventory_transactions",
                column: "InventoryItemId",
                principalTable: "inventory_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
