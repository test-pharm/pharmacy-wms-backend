-- Remove StorageLocation from Products, add Supplier
ALTER TABLE "Products" DROP COLUMN IF EXISTS "StorageLocation";
ALTER TABLE "Products" ADD COLUMN IF NOT EXISTS "Supplier" TEXT NOT NULL DEFAULT '';

-- Add Supplier and Recipient to Orders
ALTER TABLE "Orders" ADD COLUMN IF NOT EXISTS "Supplier" TEXT NOT NULL DEFAULT '';
ALTER TABLE "Orders" ADD COLUMN IF NOT EXISTS "Recipient" TEXT NOT NULL DEFAULT '';
