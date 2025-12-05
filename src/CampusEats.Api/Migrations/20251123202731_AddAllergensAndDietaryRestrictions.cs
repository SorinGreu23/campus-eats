using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusEats.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAllergensAndDietaryRestrictions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_inventory_transactions_inventory_items_inventory_item_id",
                table: "inventory_transactions");

            migrationBuilder.DropForeignKey(
                name: "f_k_menu_items_categories_category_id",
                table: "menu_items");

            migrationBuilder.DropForeignKey(
                name: "f_k_order_items_menu_items_menu_item_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "f_k_order_items_orders_order_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "f_k_orders__users_user_id",
                table: "orders");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropPrimaryKey(
                name: "p_k_payments",
                table: "payments");

            migrationBuilder.DropPrimaryKey(
                name: "p_k_orders",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "i_x_orders_user_id",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "p_k_order_items",
                table: "order_items");

            migrationBuilder.DropPrimaryKey(
                name: "p_k_notifications",
                table: "notifications");

            migrationBuilder.DropPrimaryKey(
                name: "p_k_menu_items",
                table: "menu_items");

            migrationBuilder.DropPrimaryKey(
                name: "p_k_loyalty_rewards",
                table: "loyalty_rewards");

            migrationBuilder.DropPrimaryKey(
                name: "p_k_loyalty_accounts",
                table: "loyalty_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "p_k_inventory_transactions",
                table: "inventory_transactions");

            migrationBuilder.DropPrimaryKey(
                name: "p_k_inventory_items",
                table: "inventory_items");

            migrationBuilder.DropPrimaryKey(
                name: "p_k_categories",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "loyalty_rewards");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "loyalty_rewards");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "loyalty_accounts");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "loyalty_accounts");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "inventory_transactions");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "inventory_transactions");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "categories");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "payments",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "payments",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "payments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "payments",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "payments",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "transaction_id",
                table: "payments",
                newName: "TransactionId");

            migrationBuilder.RenameColumn(
                name: "payment_method",
                table: "payments",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "order_id",
                table: "payments",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "payments",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_payments_order_id",
                table: "payments",
                newName: "IX_payments_OrderId");

            migrationBuilder.RenameColumn(
                name: "total",
                table: "orders",
                newName: "Total");

            migrationBuilder.RenameColumn(
                name: "tax",
                table: "orders",
                newName: "Tax");

            migrationBuilder.RenameColumn(
                name: "subtotal",
                table: "orders",
                newName: "Subtotal");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "orders",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "discount",
                table: "orders",
                newName: "Discount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "orders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "orders",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "special_instructions",
                table: "orders",
                newName: "SpecialInstructions");

            migrationBuilder.RenameColumn(
                name: "pickup_time",
                table: "orders",
                newName: "PickupTime");

            migrationBuilder.RenameColumn(
                name: "order_number",
                table: "orders",
                newName: "OrderNumber");

            migrationBuilder.RenameColumn(
                name: "completed_at",
                table: "orders",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "cancelled_at",
                table: "orders",
                newName: "CancelledAt");

            migrationBuilder.RenameColumn(
                name: "cancellation_reason",
                table: "orders",
                newName: "CancellationReason");

            migrationBuilder.RenameColumn(
                name: "subtotal",
                table: "order_items",
                newName: "Subtotal");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "order_items",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "order_items",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "unit_price",
                table: "order_items",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "special_instructions",
                table: "order_items",
                newName: "SpecialInstructions");

            migrationBuilder.RenameColumn(
                name: "order_id",
                table: "order_items",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "menu_item_id",
                table: "order_items",
                newName: "MenuItemId");

            migrationBuilder.RenameIndex(
                name: "i_x_order_items_order_id",
                table: "order_items",
                newName: "IX_order_items_OrderId");

            migrationBuilder.RenameIndex(
                name: "i_x_order_items_menu_item_id",
                table: "order_items",
                newName: "IX_order_items_MenuItemId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "notifications",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "notifications",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "notifications",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "notifications",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "notifications",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "order_id",
                table: "notifications",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "is_read",
                table: "notifications",
                newName: "IsRead");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "menu_items",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "menu_items",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "menu_items",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "calories",
                table: "menu_items",
                newName: "Calories");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "menu_items",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "menu_items",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "preparation_time_minutes",
                table: "menu_items",
                newName: "PreparationTimeMinutes");

            migrationBuilder.RenameColumn(
                name: "is_available",
                table: "menu_items",
                newName: "IsAvailable");

            migrationBuilder.RenameColumn(
                name: "image_url",
                table: "menu_items",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "menu_items",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "menu_items",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "i_x_menu_items_category_id",
                table: "menu_items",
                newName: "IX_menu_items_CategoryId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "loyalty_rewards",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "loyalty_rewards",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "loyalty_rewards",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "valid_until",
                table: "loyalty_rewards",
                newName: "ValidUntil");

            migrationBuilder.RenameColumn(
                name: "valid_from",
                table: "loyalty_rewards",
                newName: "ValidFrom");

            migrationBuilder.RenameColumn(
                name: "points_cost",
                table: "loyalty_rewards",
                newName: "PointsCost");

            migrationBuilder.RenameColumn(
                name: "menu_item_id",
                table: "loyalty_rewards",
                newName: "MenuItemId");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "loyalty_rewards",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "discount_value",
                table: "loyalty_rewards",
                newName: "DiscountValue");

            migrationBuilder.RenameColumn(
                name: "tier",
                table: "loyalty_accounts",
                newName: "Tier");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "loyalty_accounts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "loyalty_accounts",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "points_balance",
                table: "loyalty_accounts",
                newName: "PointsBalance");

            migrationBuilder.RenameColumn(
                name: "lifetime_points",
                table: "loyalty_accounts",
                newName: "LifetimePoints");

            migrationBuilder.RenameColumn(
                name: "reason",
                table: "inventory_transactions",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "inventory_transactions",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "inventory_transactions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "transaction_type",
                table: "inventory_transactions",
                newName: "TransactionType");

            migrationBuilder.RenameColumn(
                name: "performed_by",
                table: "inventory_transactions",
                newName: "PerformedBy");

            migrationBuilder.RenameColumn(
                name: "inventory_item_id",
                table: "inventory_transactions",
                newName: "InventoryItemId");

            migrationBuilder.RenameIndex(
                name: "i_x_inventory_transactions_inventory_item_id",
                table: "inventory_transactions",
                newName: "IX_inventory_transactions_InventoryItemId");

            migrationBuilder.RenameColumn(
                name: "unit",
                table: "inventory_items",
                newName: "Unit");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "inventory_items",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "inventory_items",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "minimum_quantity",
                table: "inventory_items",
                newName: "MinimumQuantity");

            migrationBuilder.RenameColumn(
                name: "current_quantity",
                table: "inventory_items",
                newName: "CurrentQuantity");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "categories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "categories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "categories",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "display_order",
                table: "categories",
                newName: "DisplayOrder");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "payments",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "payments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "orders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "order_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MenuItemId",
                table: "order_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "notifications",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "notifications",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "menu_items",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "menu_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "menu_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "menu_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "loyalty_accounts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TransactionType",
                table: "inventory_transactions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "InventoryItemId",
                table: "inventory_transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_payments",
                table: "payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_orders",
                table: "orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_order_items",
                table: "order_items",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notifications",
                table: "notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_menu_items",
                table: "menu_items",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_loyalty_rewards",
                table: "loyalty_rewards",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_loyalty_accounts",
                table: "loyalty_accounts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inventory_transactions",
                table: "inventory_transactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inventory_items",
                table: "inventory_items",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_categories",
                table: "categories",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "allergens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_allergens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dietary_restrictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dietary_restrictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "menu_item_allergens",
                columns: table => new
                {
                    MenuItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllergenId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_item_allergens", x => new { x.MenuItemId, x.AllergenId });
                    table.ForeignKey(
                        name: "FK_menu_item_allergens_allergens_AllergenId",
                        column: x => x.AllergenId,
                        principalTable: "allergens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_item_allergens_menu_items_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "menu_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu_item_dietary_restrictions",
                columns: table => new
                {
                    MenuItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    DietaryRestrictionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_item_dietary_restrictions", x => new { x.MenuItemId, x.DietaryRestrictionId });
                    table.ForeignKey(
                        name: "FK_menu_item_dietary_restrictions_dietary_restrictions_Dietary~",
                        column: x => x.DietaryRestrictionId,
                        principalTable: "dietary_restrictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_item_dietary_restrictions_menu_items_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "menu_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_menu_item_allergens_AllergenId",
                table: "menu_item_allergens",
                column: "AllergenId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_item_dietary_restrictions_DietaryRestrictionId",
                table: "menu_item_dietary_restrictions",
                column: "DietaryRestrictionId");

            migrationBuilder.AddForeignKey(
                name: "FK_inventory_transactions_inventory_items_InventoryItemId",
                table: "inventory_transactions",
                column: "InventoryItemId",
                principalTable: "inventory_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_menu_items_categories_CategoryId",
                table: "menu_items",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_menu_items_MenuItemId",
                table: "order_items",
                column: "MenuItemId",
                principalTable: "menu_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_orders_OrderId",
                table: "order_items",
                column: "OrderId",
                principalTable: "orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_inventory_transactions_inventory_items_InventoryItemId",
                table: "inventory_transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_menu_items_categories_CategoryId",
                table: "menu_items");

            migrationBuilder.DropForeignKey(
                name: "FK_order_items_menu_items_MenuItemId",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_order_items_orders_OrderId",
                table: "order_items");

            migrationBuilder.DropTable(
                name: "menu_item_allergens");

            migrationBuilder.DropTable(
                name: "menu_item_dietary_restrictions");

            migrationBuilder.DropTable(
                name: "allergens");

            migrationBuilder.DropTable(
                name: "dietary_restrictions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payments",
                table: "payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_orders",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_order_items",
                table: "order_items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notifications",
                table: "notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_menu_items",
                table: "menu_items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_loyalty_rewards",
                table: "loyalty_rewards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_loyalty_accounts",
                table: "loyalty_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_inventory_transactions",
                table: "inventory_transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_inventory_items",
                table: "inventory_items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_categories",
                table: "categories");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "payments",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "payments",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "payments",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "payments",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "payments",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "payments",
                newName: "transaction_id");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "payments",
                newName: "payment_method");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "payments",
                newName: "order_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "payments",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_payments_OrderId",
                table: "payments",
                newName: "IX_payments_order_id");

            migrationBuilder.RenameColumn(
                name: "Total",
                table: "orders",
                newName: "total");

            migrationBuilder.RenameColumn(
                name: "Tax",
                table: "orders",
                newName: "tax");

            migrationBuilder.RenameColumn(
                name: "Subtotal",
                table: "orders",
                newName: "subtotal");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "orders",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Discount",
                table: "orders",
                newName: "discount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "orders",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "orders",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "SpecialInstructions",
                table: "orders",
                newName: "special_instructions");

            migrationBuilder.RenameColumn(
                name: "PickupTime",
                table: "orders",
                newName: "pickup_time");

            migrationBuilder.RenameColumn(
                name: "OrderNumber",
                table: "orders",
                newName: "order_number");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "orders",
                newName: "completed_at");

            migrationBuilder.RenameColumn(
                name: "CancelledAt",
                table: "orders",
                newName: "cancelled_at");

            migrationBuilder.RenameColumn(
                name: "CancellationReason",
                table: "orders",
                newName: "cancellation_reason");

            migrationBuilder.RenameColumn(
                name: "Subtotal",
                table: "order_items",
                newName: "subtotal");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "order_items",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "order_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "order_items",
                newName: "unit_price");

            migrationBuilder.RenameColumn(
                name: "SpecialInstructions",
                table: "order_items",
                newName: "special_instructions");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "order_items",
                newName: "order_id");

            migrationBuilder.RenameColumn(
                name: "MenuItemId",
                table: "order_items",
                newName: "menu_item_id");

            migrationBuilder.RenameIndex(
                name: "IX_order_items_OrderId",
                table: "order_items",
                newName: "i_x_order_items_order_id");

            migrationBuilder.RenameIndex(
                name: "IX_order_items_MenuItemId",
                table: "order_items",
                newName: "i_x_order_items_menu_item_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "notifications",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "notifications",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "notifications",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "notifications",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "notifications",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "notifications",
                newName: "order_id");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "notifications",
                newName: "is_read");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "menu_items",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "menu_items",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "menu_items",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Calories",
                table: "menu_items",
                newName: "calories");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "menu_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "menu_items",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PreparationTimeMinutes",
                table: "menu_items",
                newName: "preparation_time_minutes");

            migrationBuilder.RenameColumn(
                name: "IsAvailable",
                table: "menu_items",
                newName: "is_available");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "menu_items",
                newName: "image_url");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "menu_items",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "menu_items",
                newName: "category_id");

            migrationBuilder.RenameIndex(
                name: "IX_menu_items_CategoryId",
                table: "menu_items",
                newName: "i_x_menu_items_category_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "loyalty_rewards",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "loyalty_rewards",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "loyalty_rewards",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ValidUntil",
                table: "loyalty_rewards",
                newName: "valid_until");

            migrationBuilder.RenameColumn(
                name: "ValidFrom",
                table: "loyalty_rewards",
                newName: "valid_from");

            migrationBuilder.RenameColumn(
                name: "PointsCost",
                table: "loyalty_rewards",
                newName: "points_cost");

            migrationBuilder.RenameColumn(
                name: "MenuItemId",
                table: "loyalty_rewards",
                newName: "menu_item_id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "loyalty_rewards",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "DiscountValue",
                table: "loyalty_rewards",
                newName: "discount_value");

            migrationBuilder.RenameColumn(
                name: "Tier",
                table: "loyalty_accounts",
                newName: "tier");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "loyalty_accounts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "loyalty_accounts",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "PointsBalance",
                table: "loyalty_accounts",
                newName: "points_balance");

            migrationBuilder.RenameColumn(
                name: "LifetimePoints",
                table: "loyalty_accounts",
                newName: "lifetime_points");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "inventory_transactions",
                newName: "reason");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "inventory_transactions",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "inventory_transactions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TransactionType",
                table: "inventory_transactions",
                newName: "transaction_type");

            migrationBuilder.RenameColumn(
                name: "PerformedBy",
                table: "inventory_transactions",
                newName: "performed_by");

            migrationBuilder.RenameColumn(
                name: "InventoryItemId",
                table: "inventory_transactions",
                newName: "inventory_item_id");

            migrationBuilder.RenameIndex(
                name: "IX_inventory_transactions_InventoryItemId",
                table: "inventory_transactions",
                newName: "i_x_inventory_transactions_inventory_item_id");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "inventory_items",
                newName: "unit");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "inventory_items",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "inventory_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "MinimumQuantity",
                table: "inventory_items",
                newName: "minimum_quantity");

            migrationBuilder.RenameColumn(
                name: "CurrentQuantity",
                table: "inventory_items",
                newName: "current_quantity");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "categories",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "categories",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "categories",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "DisplayOrder",
                table: "categories",
                newName: "display_order");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "payments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "payment_method",
                table: "payments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<Guid>(
                name: "order_id",
                table: "payments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "orders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "order_id",
                table: "order_items",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "menu_item_id",
                table: "order_items",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "order_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "order_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "message",
                table: "notifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "notifications",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "menu_items",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                table: "menu_items",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "menu_items",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "category_id",
                table: "menu_items",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "loyalty_rewards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "loyalty_rewards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "loyalty_accounts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "loyalty_accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "loyalty_accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "transaction_type",
                table: "inventory_transactions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<Guid>(
                name: "inventory_item_id",
                table: "inventory_transactions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "inventory_transactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "inventory_transactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "inventory_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "inventory_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "p_k_payments",
                table: "payments",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_orders",
                table: "orders",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_order_items",
                table: "order_items",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_notifications",
                table: "notifications",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_menu_items",
                table: "menu_items",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_loyalty_rewards",
                table: "loyalty_rewards",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_loyalty_accounts",
                table: "loyalty_accounts",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_inventory_transactions",
                table: "inventory_transactions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_inventory_items",
                table: "inventory_items",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_categories",
                table: "categories",
                column: "id");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    first_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    role = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_orders_user_id",
                table: "orders",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_inventory_transactions_inventory_items_inventory_item_id",
                table: "inventory_transactions",
                column: "inventory_item_id",
                principalTable: "inventory_items",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_menu_items_categories_category_id",
                table: "menu_items",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_order_items_menu_items_menu_item_id",
                table: "order_items",
                column: "menu_item_id",
                principalTable: "menu_items",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "f_k_order_items_orders_order_id",
                table: "order_items",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_orders__users_user_id",
                table: "orders",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
