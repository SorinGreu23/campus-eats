CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE TABLE categories (
        id uuid NOT NULL,
        name character varying(200) NOT NULL,
        display_order integer,
        is_active boolean NOT NULL DEFAULT TRUE,
        created_at timestamp with time zone,
        updated_at timestamp with time zone,
        CONSTRAINT p_k_categories PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE TABLE inventory_items (
        id uuid NOT NULL,
        name character varying(250) NOT NULL,
        unit character varying(64),
        current_quantity numeric(18,2) NOT NULL,
        minimum_quantity numeric(18,2) NOT NULL,
        created_at timestamp with time zone,
        updated_at timestamp with time zone,
        CONSTRAINT p_k_inventory_items PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE TABLE loyalty_accounts (
        id uuid NOT NULL,
        user_id uuid,
        points_balance integer NOT NULL DEFAULT 0,
        lifetime_points integer NOT NULL DEFAULT 0,
        tier character varying(64),
        created_at timestamp with time zone,
        updated_at timestamp with time zone,
        CONSTRAINT p_k_loyalty_accounts PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE TABLE loyalty_rewards (
        id uuid NOT NULL,
        name character varying(200) NOT NULL,
        description text,
        points_cost integer NOT NULL,
        discount_value numeric(18,2),
        menu_item_id uuid,
        is_active boolean NOT NULL DEFAULT TRUE,
        valid_from timestamp with time zone,
        valid_until timestamp with time zone,
        created_at timestamp with time zone,
        updated_at timestamp with time zone,
        CONSTRAINT p_k_loyalty_rewards PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE TABLE notifications (
        id uuid NOT NULL,
        user_id uuid,
        type character varying(128),
        title character varying(250),
        message text,
        is_read boolean NOT NULL DEFAULT FALSE,
        order_id uuid,
        created_at timestamp with time zone,
        updated_at timestamp with time zone,
        CONSTRAINT p_k_notifications PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE TABLE payments (
        id uuid NOT NULL,
        order_id uuid,
        user_id uuid,
        amount numeric(18,2) NOT NULL,
        status character varying(64),
        payment_method character varying(128),
        transaction_id character varying(256),
        created_at timestamp with time zone,
        updated_at timestamp with time zone,
        CONSTRAINT p_k_payments PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE TABLE users (
        id uuid NOT NULL,
        email character varying(256) NOT NULL,
        password_hash character varying(512),
        first_name character varying(128),
        last_name character varying(128),
        phone character varying(32),
        role character varying(64),
        is_active boolean NOT NULL DEFAULT TRUE,
        created_at timestamp with time zone,
        updated_at timestamp with time zone,
        CONSTRAINT p_k_users PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE TABLE menu_items (
        id uuid NOT NULL,
        name character varying(250) NOT NULL,
        description text,
        price numeric(18,2) NOT NULL,
        category_id uuid,
        image_url character varying(512),
        preparation_time_minutes integer,
        is_available boolean NOT NULL DEFAULT TRUE,
        calories integer,
        created_at timestamp with time zone,
        updated_at timestamp with time zone,
        CONSTRAINT p_k_menu_items PRIMARY KEY (id),
        CONSTRAINT f_k_menu_items_categories_category_id FOREIGN KEY (category_id) REFERENCES categories (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE TABLE inventory_transactions (
        id uuid NOT NULL,
        inventory_item_id uuid,
        transaction_type character varying(64),
        quantity numeric(18,2) NOT NULL,
        reason text,
        performed_by uuid,
        created_at timestamp with time zone,
        updated_at timestamp with time zone,
        CONSTRAINT p_k_inventory_transactions PRIMARY KEY (id),
        CONSTRAINT f_k_inventory_transactions_inventory_items_inventory_item_id FOREIGN KEY (inventory_item_id) REFERENCES inventory_items (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE TABLE orders (
        id uuid NOT NULL,
        order_number character varying(100),
        user_id uuid,
        status character varying(64),
        subtotal numeric(18,2) NOT NULL,
        tax numeric(18,2) NOT NULL,
        discount numeric(18,2) NOT NULL,
        total numeric(18,2) NOT NULL,
        special_instructions text,
        pickup_time timestamp with time zone,
        completed_at timestamp with time zone,
        cancelled_at timestamp with time zone,
        cancellation_reason text,
        created_at timestamp with time zone,
        updated_at timestamp with time zone,
        CONSTRAINT p_k_orders PRIMARY KEY (id),
        CONSTRAINT f_k_orders__users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE TABLE order_items (
        id uuid NOT NULL,
        order_id uuid,
        menu_item_id uuid,
        quantity integer NOT NULL,
        unit_price numeric(18,2) NOT NULL,
        subtotal numeric(18,2) NOT NULL,
        special_instructions text,
        created_at timestamp with time zone,
        updated_at timestamp with time zone,
        CONSTRAINT p_k_order_items PRIMARY KEY (id),
        CONSTRAINT f_k_order_items_menu_items_menu_item_id FOREIGN KEY (menu_item_id) REFERENCES menu_items (id) ON DELETE SET NULL,
        CONSTRAINT f_k_order_items_orders_order_id FOREIGN KEY (order_id) REFERENCES orders (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE INDEX i_x_inventory_transactions_inventory_item_id ON inventory_transactions (inventory_item_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE INDEX i_x_menu_items_category_id ON menu_items (category_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE INDEX i_x_order_items_menu_item_id ON order_items (menu_item_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE INDEX i_x_order_items_order_id ON order_items (order_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE INDEX i_x_orders_user_id ON orders (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    CREATE INDEX "IX_payments_order_id" ON payments (order_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251110081448_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251110081448_InitialCreate', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE TABLE categories (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "DisplayOrder" integer,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        CONSTRAINT "PK_categories" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE TABLE inventory_items (
        "Id" uuid NOT NULL,
        "Name" character varying(250) NOT NULL,
        "Unit" character varying(64),
        "CurrentQuantity" numeric(18,2) NOT NULL,
        "MinimumQuantity" numeric(18,2) NOT NULL,
        CONSTRAINT "PK_inventory_items" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE TABLE loyalty_accounts (
        "Id" uuid NOT NULL,
        "UserId" text NOT NULL,
        "PointsBalance" integer NOT NULL DEFAULT 0,
        "LifetimePoints" integer NOT NULL DEFAULT 0,
        "Tier" character varying(64),
        CONSTRAINT "PK_loyalty_accounts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE TABLE loyalty_rewards (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" text,
        "PointsCost" integer NOT NULL,
        "DiscountValue" numeric(18,2),
        "MenuItemId" uuid,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "ValidFrom" timestamp with time zone,
        "ValidUntil" timestamp with time zone,
        CONSTRAINT "PK_loyalty_rewards" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE TABLE notifications (
        "Id" uuid NOT NULL,
        "UserId" text NOT NULL,
        "Type" character varying(128),
        "Title" character varying(250),
        "Message" text NOT NULL,
        "IsRead" boolean NOT NULL DEFAULT FALSE,
        "OrderId" uuid,
        CONSTRAINT "PK_notifications" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE TABLE payments (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "UserId" text,
        "Amount" numeric(18,2) NOT NULL,
        "Status" character varying(64),
        "PaymentMethod" character varying(128) NOT NULL,
        "TransactionId" character varying(256),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_payments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE TABLE menu_items (
        "Id" uuid NOT NULL,
        "Name" character varying(250) NOT NULL,
        "Description" text NOT NULL,
        "Price" numeric(18,2) NOT NULL,
        "CategoryId" uuid NOT NULL,
        "ImageUrl" character varying(512),
        "PreparationTimeMinutes" integer,
        "IsAvailable" boolean NOT NULL DEFAULT TRUE,
        "Calories" integer,
        CONSTRAINT "PK_menu_items" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_menu_items_categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES categories ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE TABLE inventory_transactions (
        "Id" uuid NOT NULL,
        "InventoryItemId" uuid NOT NULL,
        "TransactionType" character varying(64) NOT NULL,
        "Quantity" numeric(18,2) NOT NULL,
        "Reason" text,
        "PerformedBy" uuid,
        CONSTRAINT "PK_inventory_transactions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_inventory_transactions_inventory_items_InventoryItemId" FOREIGN KEY ("InventoryItemId") REFERENCES inventory_items ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE TABLE orders (
        "Id" uuid NOT NULL,
        "OrderNumber" character varying(100),
        "UserId" text NOT NULL,
        "Status" character varying(64) NOT NULL,
        "Subtotal" numeric(18,2) NOT NULL,
        "Tax" numeric(18,2) NOT NULL,
        "Discount" numeric(18,2) NOT NULL,
        "Total" numeric(18,2) NOT NULL,
        "SpecialInstructions" text,
        "PickupTime" timestamp with time zone,
        "CompletedAt" timestamp with time zone,
        "CancelledAt" timestamp with time zone,
        "CancellationReason" text,
        CONSTRAINT "PK_orders" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE TABLE order_items (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "MenuItemId" uuid NOT NULL,
        "Quantity" integer NOT NULL,
        "UnitPrice" numeric(18,2) NOT NULL,
        "Subtotal" numeric(18,2) NOT NULL,
        "SpecialInstructions" text,
        CONSTRAINT "PK_order_items" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_order_items_menu_items_MenuItemId" FOREIGN KEY ("MenuItemId") REFERENCES menu_items ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_order_items_orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES orders ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE INDEX "IX_inventory_transactions_InventoryItemId" ON inventory_transactions ("InventoryItemId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE INDEX "IX_menu_items_CategoryId" ON menu_items ("CategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE INDEX "IX_order_items_MenuItemId" ON order_items ("MenuItemId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE INDEX "IX_order_items_OrderId" ON order_items ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE INDEX "IX_orders_UserId" ON orders ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    CREATE INDEX "IX_payments_OrderId" ON payments ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123104037_InitialCampus') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251123104037_InitialCampus', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123110224_RemoveOrderUserNavigation') THEN

                    DO $$ 
                    BEGIN
                        IF EXISTS (
                            SELECT 1 FROM information_schema.table_constraints 
                            WHERE constraint_name = 'FK_orders_User_UserId'
                        ) THEN
                            ALTER TABLE orders DROP CONSTRAINT "FK_orders_User_UserId";
                        END IF;
                    END $$;
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123110224_RemoveOrderUserNavigation') THEN
    DROP TABLE IF EXISTS "User";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123110224_RemoveOrderUserNavigation') THEN
    DROP INDEX IF EXISTS "IX_orders_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123110224_RemoveOrderUserNavigation') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251123110224_RemoveOrderUserNavigation', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN

                    DO $$ 
                    BEGIN
                        IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'f_k_inventory_transactions_inventory_items_inventory_item_id') THEN
                            ALTER TABLE inventory_transactions DROP CONSTRAINT f_k_inventory_transactions_inventory_items_inventory_item_id;
                        END IF;
                    END $$;
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN

                    DO $$ 
                    BEGIN
                        IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'f_k_menu_items_categories_category_id') THEN
                            ALTER TABLE menu_items DROP CONSTRAINT f_k_menu_items_categories_category_id;
                        END IF;
                    END $$;
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN

                    DO $$ 
                    BEGIN
                        IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'f_k_order_items_menu_items_menu_item_id') THEN
                            ALTER TABLE order_items DROP CONSTRAINT f_k_order_items_menu_items_menu_item_id;
                        END IF;
                    END $$;
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN

                    DO $$ 
                    BEGIN
                        IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'f_k_order_items_orders_order_id') THEN
                            ALTER TABLE order_items DROP CONSTRAINT f_k_order_items_orders_order_id;
                        END IF;
                    END $$;
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN

                    DO $$ 
                    BEGIN
                        IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'f_k_orders__users_user_id') THEN
                            ALTER TABLE orders DROP CONSTRAINT f_k_orders__users_user_id;
                        END IF;
                    END $$;
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN

                    DO $$ 
                    BEGIN
                        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'users') THEN
                            DROP TABLE users;
                        END IF;
                    END $$;
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments DROP CONSTRAINT p_k_payments;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders DROP CONSTRAINT p_k_orders;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    DROP INDEX i_x_orders_user_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items DROP CONSTRAINT p_k_order_items;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications DROP CONSTRAINT p_k_notifications;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items DROP CONSTRAINT p_k_menu_items;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_rewards DROP CONSTRAINT p_k_loyalty_rewards;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_accounts DROP CONSTRAINT p_k_loyalty_accounts;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_transactions DROP CONSTRAINT p_k_inventory_transactions;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_items DROP CONSTRAINT p_k_inventory_items;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE categories DROP CONSTRAINT p_k_categories;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders DROP COLUMN created_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders DROP COLUMN updated_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items DROP COLUMN created_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items DROP COLUMN updated_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications DROP COLUMN created_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications DROP COLUMN updated_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_rewards DROP COLUMN created_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_rewards DROP COLUMN updated_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_accounts DROP COLUMN created_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_accounts DROP COLUMN updated_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_transactions DROP COLUMN created_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_transactions DROP COLUMN updated_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_items DROP COLUMN created_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_items DROP COLUMN updated_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE categories DROP COLUMN created_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE categories DROP COLUMN updated_at;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments RENAME COLUMN status TO "Status";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments RENAME COLUMN amount TO "Amount";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments RENAME COLUMN id TO "Id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments RENAME COLUMN user_id TO "UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments RENAME COLUMN updated_at TO "UpdatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments RENAME COLUMN transaction_id TO "TransactionId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments RENAME COLUMN payment_method TO "PaymentMethod";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments RENAME COLUMN order_id TO "OrderId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments RENAME COLUMN created_at TO "CreatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER INDEX "IX_payments_order_id" RENAME TO "IX_payments_OrderId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders RENAME COLUMN total TO "Total";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders RENAME COLUMN tax TO "Tax";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders RENAME COLUMN subtotal TO "Subtotal";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders RENAME COLUMN status TO "Status";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders RENAME COLUMN discount TO "Discount";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders RENAME COLUMN id TO "Id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders RENAME COLUMN user_id TO "UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders RENAME COLUMN special_instructions TO "SpecialInstructions";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders RENAME COLUMN pickup_time TO "PickupTime";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders RENAME COLUMN order_number TO "OrderNumber";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders RENAME COLUMN completed_at TO "CompletedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders RENAME COLUMN cancelled_at TO "CancelledAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders RENAME COLUMN cancellation_reason TO "CancellationReason";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items RENAME COLUMN subtotal TO "Subtotal";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items RENAME COLUMN quantity TO "Quantity";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items RENAME COLUMN id TO "Id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items RENAME COLUMN unit_price TO "UnitPrice";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items RENAME COLUMN special_instructions TO "SpecialInstructions";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items RENAME COLUMN order_id TO "OrderId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items RENAME COLUMN menu_item_id TO "MenuItemId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER INDEX i_x_order_items_order_id RENAME TO "IX_order_items_OrderId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER INDEX i_x_order_items_menu_item_id RENAME TO "IX_order_items_MenuItemId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications RENAME COLUMN type TO "Type";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications RENAME COLUMN title TO "Title";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications RENAME COLUMN message TO "Message";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications RENAME COLUMN id TO "Id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications RENAME COLUMN user_id TO "UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications RENAME COLUMN order_id TO "OrderId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications RENAME COLUMN is_read TO "IsRead";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items RENAME COLUMN price TO "Price";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items RENAME COLUMN name TO "Name";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items RENAME COLUMN description TO "Description";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items RENAME COLUMN calories TO "Calories";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items RENAME COLUMN id TO "Id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items RENAME COLUMN updated_at TO "UpdatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items RENAME COLUMN preparation_time_minutes TO "PreparationTimeMinutes";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items RENAME COLUMN is_available TO "IsAvailable";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items RENAME COLUMN image_url TO "ImageUrl";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items RENAME COLUMN created_at TO "CreatedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items RENAME COLUMN category_id TO "CategoryId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER INDEX i_x_menu_items_category_id RENAME TO "IX_menu_items_CategoryId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_rewards RENAME COLUMN name TO "Name";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_rewards RENAME COLUMN description TO "Description";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_rewards RENAME COLUMN id TO "Id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_rewards RENAME COLUMN valid_until TO "ValidUntil";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_rewards RENAME COLUMN valid_from TO "ValidFrom";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_rewards RENAME COLUMN points_cost TO "PointsCost";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_rewards RENAME COLUMN menu_item_id TO "MenuItemId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_rewards RENAME COLUMN is_active TO "IsActive";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_rewards RENAME COLUMN discount_value TO "DiscountValue";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_accounts RENAME COLUMN tier TO "Tier";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_accounts RENAME COLUMN id TO "Id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_accounts RENAME COLUMN user_id TO "UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_accounts RENAME COLUMN points_balance TO "PointsBalance";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_accounts RENAME COLUMN lifetime_points TO "LifetimePoints";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_transactions RENAME COLUMN reason TO "Reason";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_transactions RENAME COLUMN quantity TO "Quantity";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_transactions RENAME COLUMN id TO "Id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_transactions RENAME COLUMN transaction_type TO "TransactionType";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_transactions RENAME COLUMN performed_by TO "PerformedBy";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_transactions RENAME COLUMN inventory_item_id TO "InventoryItemId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER INDEX i_x_inventory_transactions_inventory_item_id RENAME TO "IX_inventory_transactions_InventoryItemId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_items RENAME COLUMN unit TO "Unit";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_items RENAME COLUMN name TO "Name";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_items RENAME COLUMN id TO "Id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_items RENAME COLUMN minimum_quantity TO "MinimumQuantity";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_items RENAME COLUMN current_quantity TO "CurrentQuantity";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE categories RENAME COLUMN name TO "Name";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE categories RENAME COLUMN id TO "Id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE categories RENAME COLUMN is_active TO "IsActive";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE categories RENAME COLUMN display_order TO "DisplayOrder";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments ALTER COLUMN "UserId" TYPE text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE payments SET "UpdatedAt" = TIMESTAMPTZ '-infinity' WHERE "UpdatedAt" IS NULL;
    ALTER TABLE payments ALTER COLUMN "UpdatedAt" SET NOT NULL;
    ALTER TABLE payments ALTER COLUMN "UpdatedAt" SET DEFAULT TIMESTAMPTZ '-infinity';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE payments SET "PaymentMethod" = '' WHERE "PaymentMethod" IS NULL;
    ALTER TABLE payments ALTER COLUMN "PaymentMethod" SET NOT NULL;
    ALTER TABLE payments ALTER COLUMN "PaymentMethod" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE payments SET "OrderId" = '00000000-0000-0000-0000-000000000000' WHERE "OrderId" IS NULL;
    ALTER TABLE payments ALTER COLUMN "OrderId" SET NOT NULL;
    ALTER TABLE payments ALTER COLUMN "OrderId" SET DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE payments SET "CreatedAt" = TIMESTAMPTZ '-infinity' WHERE "CreatedAt" IS NULL;
    ALTER TABLE payments ALTER COLUMN "CreatedAt" SET NOT NULL;
    ALTER TABLE payments ALTER COLUMN "CreatedAt" SET DEFAULT TIMESTAMPTZ '-infinity';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE orders SET "Status" = '' WHERE "Status" IS NULL;
    ALTER TABLE orders ALTER COLUMN "Status" SET NOT NULL;
    ALTER TABLE orders ALTER COLUMN "Status" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders ALTER COLUMN "UserId" TYPE text;
    UPDATE orders SET "UserId" = '' WHERE "UserId" IS NULL;
    ALTER TABLE orders ALTER COLUMN "UserId" SET NOT NULL;
    ALTER TABLE orders ALTER COLUMN "UserId" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE order_items SET "OrderId" = '00000000-0000-0000-0000-000000000000' WHERE "OrderId" IS NULL;
    ALTER TABLE order_items ALTER COLUMN "OrderId" SET NOT NULL;
    ALTER TABLE order_items ALTER COLUMN "OrderId" SET DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE order_items SET "MenuItemId" = '00000000-0000-0000-0000-000000000000' WHERE "MenuItemId" IS NULL;
    ALTER TABLE order_items ALTER COLUMN "MenuItemId" SET NOT NULL;
    ALTER TABLE order_items ALTER COLUMN "MenuItemId" SET DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE notifications SET "Message" = '' WHERE "Message" IS NULL;
    ALTER TABLE notifications ALTER COLUMN "Message" SET NOT NULL;
    ALTER TABLE notifications ALTER COLUMN "Message" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications ALTER COLUMN "UserId" TYPE text;
    UPDATE notifications SET "UserId" = '' WHERE "UserId" IS NULL;
    ALTER TABLE notifications ALTER COLUMN "UserId" SET NOT NULL;
    ALTER TABLE notifications ALTER COLUMN "UserId" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE menu_items SET "Description" = '' WHERE "Description" IS NULL;
    ALTER TABLE menu_items ALTER COLUMN "Description" SET NOT NULL;
    ALTER TABLE menu_items ALTER COLUMN "Description" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE menu_items SET "UpdatedAt" = TIMESTAMPTZ '-infinity' WHERE "UpdatedAt" IS NULL;
    ALTER TABLE menu_items ALTER COLUMN "UpdatedAt" SET NOT NULL;
    ALTER TABLE menu_items ALTER COLUMN "UpdatedAt" SET DEFAULT TIMESTAMPTZ '-infinity';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE menu_items SET "CreatedAt" = TIMESTAMPTZ '-infinity' WHERE "CreatedAt" IS NULL;
    ALTER TABLE menu_items ALTER COLUMN "CreatedAt" SET NOT NULL;
    ALTER TABLE menu_items ALTER COLUMN "CreatedAt" SET DEFAULT TIMESTAMPTZ '-infinity';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE menu_items SET "CategoryId" = '00000000-0000-0000-0000-000000000000' WHERE "CategoryId" IS NULL;
    ALTER TABLE menu_items ALTER COLUMN "CategoryId" SET NOT NULL;
    ALTER TABLE menu_items ALTER COLUMN "CategoryId" SET DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_accounts ALTER COLUMN "UserId" TYPE text;
    UPDATE loyalty_accounts SET "UserId" = '' WHERE "UserId" IS NULL;
    ALTER TABLE loyalty_accounts ALTER COLUMN "UserId" SET NOT NULL;
    ALTER TABLE loyalty_accounts ALTER COLUMN "UserId" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE inventory_transactions SET "TransactionType" = '' WHERE "TransactionType" IS NULL;
    ALTER TABLE inventory_transactions ALTER COLUMN "TransactionType" SET NOT NULL;
    ALTER TABLE inventory_transactions ALTER COLUMN "TransactionType" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    UPDATE inventory_transactions SET "InventoryItemId" = '00000000-0000-0000-0000-000000000000' WHERE "InventoryItemId" IS NULL;
    ALTER TABLE inventory_transactions ALTER COLUMN "InventoryItemId" SET NOT NULL;
    ALTER TABLE inventory_transactions ALTER COLUMN "InventoryItemId" SET DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments ADD CONSTRAINT "PK_payments" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders ADD CONSTRAINT "PK_orders" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items ADD CONSTRAINT "PK_order_items" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications ADD CONSTRAINT "PK_notifications" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items ADD CONSTRAINT "PK_menu_items" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_rewards ADD CONSTRAINT "PK_loyalty_rewards" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_accounts ADD CONSTRAINT "PK_loyalty_accounts" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_transactions ADD CONSTRAINT "PK_inventory_transactions" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_items ADD CONSTRAINT "PK_inventory_items" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE categories ADD CONSTRAINT "PK_categories" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    CREATE TABLE allergens (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Icon" character varying(50),
        CONSTRAINT "PK_allergens" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    CREATE TABLE dietary_restrictions (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Icon" character varying(50),
        CONSTRAINT "PK_dietary_restrictions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    CREATE TABLE menu_item_allergens (
        "MenuItemId" uuid NOT NULL,
        "AllergenId" uuid NOT NULL,
        CONSTRAINT "PK_menu_item_allergens" PRIMARY KEY ("MenuItemId", "AllergenId"),
        CONSTRAINT "FK_menu_item_allergens_allergens_AllergenId" FOREIGN KEY ("AllergenId") REFERENCES allergens ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_menu_item_allergens_menu_items_MenuItemId" FOREIGN KEY ("MenuItemId") REFERENCES menu_items ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    CREATE TABLE menu_item_dietary_restrictions (
        "MenuItemId" uuid NOT NULL,
        "DietaryRestrictionId" uuid NOT NULL,
        CONSTRAINT "PK_menu_item_dietary_restrictions" PRIMARY KEY ("MenuItemId", "DietaryRestrictionId"),
        CONSTRAINT "FK_menu_item_dietary_restrictions_dietary_restrictions_Dietary~" FOREIGN KEY ("DietaryRestrictionId") REFERENCES dietary_restrictions ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_menu_item_dietary_restrictions_menu_items_MenuItemId" FOREIGN KEY ("MenuItemId") REFERENCES menu_items ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    CREATE INDEX "IX_menu_item_allergens_AllergenId" ON menu_item_allergens ("AllergenId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    CREATE INDEX "IX_menu_item_dietary_restrictions_DietaryRestrictionId" ON menu_item_dietary_restrictions ("DietaryRestrictionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_transactions ADD CONSTRAINT "FK_inventory_transactions_inventory_items_InventoryItemId" FOREIGN KEY ("InventoryItemId") REFERENCES inventory_items ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items ADD CONSTRAINT "FK_menu_items_categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES categories ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items ADD CONSTRAINT "FK_order_items_menu_items_MenuItemId" FOREIGN KEY ("MenuItemId") REFERENCES menu_items ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items ADD CONSTRAINT "FK_order_items_orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES orders ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123202731_AddAllergensAndDietaryRestrictions') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251123202731_AddAllergensAndDietaryRestrictions', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123215748_AddMinimumTierToLoyaltyReward') THEN
    ALTER TABLE loyalty_rewards ADD "MinimumTier" character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123215748_AddMinimumTierToLoyaltyReward') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251123215748_AddMinimumTierToLoyaltyReward', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123221117_AddLoyaltyClaim') THEN
    CREATE TABLE "LoyaltyClaim" (
        "Id" uuid NOT NULL,
        "LoyaltyAccountId" uuid NOT NULL,
        "RewardId" uuid NOT NULL,
        "ClaimedAt" timestamp with time zone NOT NULL,
        "Notes" character varying(500),
        CONSTRAINT "PK_LoyaltyClaim" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_LoyaltyClaim_loyalty_accounts_LoyaltyAccountId" FOREIGN KEY ("LoyaltyAccountId") REFERENCES loyalty_accounts ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_LoyaltyClaim_loyalty_rewards_RewardId" FOREIGN KEY ("RewardId") REFERENCES loyalty_rewards ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123221117_AddLoyaltyClaim') THEN
    CREATE INDEX "IX_LoyaltyClaim_LoyaltyAccountId" ON "LoyaltyClaim" ("LoyaltyAccountId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123221117_AddLoyaltyClaim') THEN
    CREATE INDEX "IX_LoyaltyClaim_RewardId" ON "LoyaltyClaim" ("RewardId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123221117_AddLoyaltyClaim') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251123221117_AddLoyaltyClaim', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126093741_AddOrderTypeToOrder') THEN
    ALTER TABLE orders RENAME COLUMN "SpecialInstructions" TO "DeliveryInstructions";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126093741_AddOrderTypeToOrder') THEN
    ALTER TABLE orders ADD order_type character varying(32);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126093741_AddOrderTypeToOrder') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251126093741_AddOrderTypeToOrder', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments ALTER COLUMN "PaymentMethod" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    ALTER TABLE payments ALTER COLUMN "OrderId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders ALTER COLUMN "UserId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    ALTER TABLE orders ALTER COLUMN "Status" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items ALTER COLUMN "OrderId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    ALTER TABLE order_items ALTER COLUMN "MenuItemId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications ALTER COLUMN "UserId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    ALTER TABLE notifications ALTER COLUMN "Message" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items ALTER COLUMN "Description" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    ALTER TABLE menu_items ALTER COLUMN "CategoryId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    ALTER TABLE loyalty_accounts ALTER COLUMN "UserId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_transactions ALTER COLUMN "TransactionType" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    ALTER TABLE inventory_transactions ALTER COLUMN "InventoryItemId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    CREATE TABLE allergens (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" character varying(500),
        "Icon" character varying(50),
        CONSTRAINT "PK_allergens" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    CREATE TABLE dietary_restrictions (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" character varying(500),
        "Icon" character varying(50),
        CONSTRAINT "PK_dietary_restrictions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    CREATE TABLE menu_item_allergens (
        "MenuItemId" uuid NOT NULL,
        "AllergenId" uuid NOT NULL,
        CONSTRAINT "PK_menu_item_allergens" PRIMARY KEY ("MenuItemId", "AllergenId"),
        CONSTRAINT "FK_menu_item_allergens_allergens_AllergenId" FOREIGN KEY ("AllergenId") REFERENCES allergens ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_menu_item_allergens_menu_items_MenuItemId" FOREIGN KEY ("MenuItemId") REFERENCES menu_items ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    CREATE TABLE menu_item_dietary_restrictions (
        "MenuItemId" uuid NOT NULL,
        "DietaryRestrictionId" uuid NOT NULL,
        CONSTRAINT "PK_menu_item_dietary_restrictions" PRIMARY KEY ("MenuItemId", "DietaryRestrictionId"),
        CONSTRAINT "FK_menu_item_dietary_restrictions_dietary_restrictions_Dietary~" FOREIGN KEY ("DietaryRestrictionId") REFERENCES dietary_restrictions ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_menu_item_dietary_restrictions_menu_items_MenuItemId" FOREIGN KEY ("MenuItemId") REFERENCES menu_items ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    CREATE INDEX "IX_menu_item_allergens_AllergenId" ON menu_item_allergens ("AllergenId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    CREATE INDEX "IX_menu_item_dietary_restrictions_DietaryRestrictionId" ON menu_item_dietary_restrictions ("DietaryRestrictionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251126204226_AddDescriptionToAllergensAndDietaryRestrictions') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251126204226_AddDescriptionToAllergensAndDietaryRestrictions', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108194543_AddUpdatedAtColumn') THEN

                    DO $$
                    BEGIN
                        IF NOT EXISTS (
                            SELECT 1 FROM information_schema.columns 
                            WHERE table_name = 'inventory_items' AND column_name = 'UpdatedAt'
                        ) THEN
                            ALTER TABLE inventory_items ADD COLUMN "UpdatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                        END IF;
                    END $$;
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108194543_AddUpdatedAtColumn') THEN

                    DO $$
                    BEGIN
                        IF NOT EXISTS (
                            SELECT 1 FROM information_schema.columns 
                            WHERE table_name = 'inventory_transactions' AND column_name = 'CreatedAt'
                        ) THEN
                            ALTER TABLE inventory_transactions ADD COLUMN "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                        END IF;
                    END $$;
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260108194543_AddUpdatedAtColumn') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260108194543_AddUpdatedAtColumn', '9.0.10');
    END IF;
END $EF$;
COMMIT;

