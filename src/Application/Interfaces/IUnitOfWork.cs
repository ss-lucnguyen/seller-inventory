using SellerInventer.Domain.Entities;

namespace SellerInventer.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Category> Categories { get; }
    IRepository<Product> Products { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
