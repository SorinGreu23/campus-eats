using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusEats.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedTestLoyaltyAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create loyalty accounts for all existing users with default points
            migrationBuilder.Sql(@"
                INSERT INTO loyalty_accounts (""Id"", ""UserId"", ""PointsBalance"", ""LifetimePoints"", ""Tier"")
                SELECT 
                    gen_random_uuid(),
                    u.""Id"",
                    500,
                    1000,
                    'Silver'
                FROM ""AspNetUsers"" u
                WHERE NOT EXISTS (
                    SELECT 1 FROM loyalty_accounts la WHERE la.""UserId"" = u.""Id""
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Don't delete accounts as they might have been used
            // This is a data seed, so we leave it in place
        }
    }
}

