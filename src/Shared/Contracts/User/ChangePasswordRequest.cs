namespace SellerInventory.Shared.Contracts.User;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
