using SellerInventory.Domain.Entities;

namespace SellerInventory.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Store> Stores { get; }
    IRepository<StoreInvitation> StoreInvitations { get; }
    IRepository<User> Users { get; }
    IRepository<Category> Categories { get; }
    IRepository<Product> Products { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<Invoice> Invoices { get; }
    IRepository<Customer> Customers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
