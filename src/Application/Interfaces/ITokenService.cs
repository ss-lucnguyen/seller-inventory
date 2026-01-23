using SellerInventory.Domain.Entities;

namespace SellerInventory.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
