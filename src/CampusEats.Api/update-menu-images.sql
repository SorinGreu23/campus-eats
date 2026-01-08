-- Update existing menu items with image URLs from Unsplash CDN

UPDATE menu_items 
SET "ImageUrl" = 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400&h=400&fit=crop', "UpdatedAt" = CURRENT_TIMESTAMP
WHERE "Name" = 'Classic Cheeseburger';

UPDATE menu_items 
SET "ImageUrl" = 'https://images.unsplash.com/photo-1626700051175-6818013e1d4f?w=400&h=400&fit=crop', "UpdatedAt" = CURRENT_TIMESTAMP
WHERE "Name" = 'Grilled Chicken Wrap';

UPDATE menu_items 
SET "ImageUrl" = 'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&h=400&fit=crop', "UpdatedAt" = CURRENT_TIMESTAMP
WHERE "Name" = 'Veggie Salad Bowl';

UPDATE menu_items 
SET "ImageUrl" = 'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=400&fit=crop', "UpdatedAt" = CURRENT_TIMESTAMP
WHERE "Name" = 'Spicy Ramen';

UPDATE menu_items 
SET "ImageUrl" = 'https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=400&h=400&fit=crop', "UpdatedAt" = CURRENT_TIMESTAMP
WHERE "Name" = 'Chocolate Brownie';

-- Verify the updates
SELECT "Id", "Name", "ImageUrl" FROM menu_items WHERE "ImageUrl" IS NOT NULL;
