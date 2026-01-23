namespace SellerInventory.Infrastructure.Auth;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SellerInventory";
    public string Audience { get; set; } = "SellerInventory";
    public int ExpirationHours { get; set; } = 24;
}
