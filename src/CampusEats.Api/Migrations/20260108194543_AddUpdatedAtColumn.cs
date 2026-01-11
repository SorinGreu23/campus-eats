using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusEats.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add UpdatedAt column to inventory_items if it doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'inventory_items' AND column_name = 'UpdatedAt'
                    ) THEN
                        ALTER TABLE inventory_items ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                    END IF;
                END $$;
            ");

            // Add CreatedAt column to inventory_transactions if it doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'inventory_transactions' AND column_name = 'CreatedAt'
                    ) THEN
                        ALTER TABLE inventory_transactions ADD COLUMN ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "inventory_transactions");
        }
    }
}
