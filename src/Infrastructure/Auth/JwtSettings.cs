namespace SellerInventer.Infrastructure.Auth;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SellerInventer";
    public string Audience { get; set; } = "SellerInventer";
    public int ExpirationHours { get; set; } = 24;
}
