using SellerInventer.Domain.Entities;

namespace SellerInventer.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
