using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusEats.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedLoyaltyRewards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert sample loyalty rewards
            migrationBuilder.Sql(@"
                INSERT INTO loyalty_rewards (""Id"", ""Name"", ""Description"", ""PointsCost"", ""DiscountValue"", ""IsActive"")
                VALUES 
                    ('11111111-1111-1111-1111-111111111111'::uuid, '5 RON Off', 'Get 5 RON discount on your order', 50, 5.00, true),
                    ('22222222-2222-2222-2222-222222222222'::uuid, '10 RON Off', 'Get 10 RON discount on your order', 100, 10.00, true),
                    ('33333333-3333-3333-3333-333333333333'::uuid, '15 RON Off', 'Get 15 RON discount on your order', 200, 15.00, true),
                    ('44444444-4444-4444-4444-444444444444'::uuid, '20 RON Off', 'Get 20 RON discount on your order', 300, 20.00, true),
                    ('55555555-5555-5555-5555-555555555555'::uuid, '25 RON Off', 'Get 25 RON discount on your order', 400, 25.00, true)
                ON CONFLICT (""Id"") DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded rewards
            migrationBuilder.Sql(@"
                DELETE FROM loyalty_rewards 
                WHERE ""Id"" IN (
                    '11111111-1111-1111-1111-111111111111'::uuid,
                    '22222222-2222-2222-2222-222222222222'::uuid,
                    '33333333-3333-3333-3333-333333333333'::uuid,
                    '44444444-4444-4444-4444-444444444444'::uuid,
                    '55555555-5555-5555-5555-555555555555'::uuid
                );
            ");
        }
    }
}

