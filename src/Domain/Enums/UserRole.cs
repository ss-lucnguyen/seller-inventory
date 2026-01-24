namespace SellerInventory.Domain.Enums;

public enum UserRole
{
    Staff = 0,      // Store-level: basic operations (POS, view products)
    Manager = 1,    // Store-level: full store management (CRUD operations, reports)
    SystemAdmin = 2 // System-level: platform administration (not tied to a store)
}
