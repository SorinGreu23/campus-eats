-- Query to list all menu items and their current image URLs
SELECT "Id", "Name", "ImageUrl", "CategoryId" 
FROM menu_items 
ORDER BY "Name";
