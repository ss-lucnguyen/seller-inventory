namespace SellerInventer.Shared.Contracts.User;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
