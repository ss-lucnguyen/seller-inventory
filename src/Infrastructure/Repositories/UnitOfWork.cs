using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Entities;
using SellerInventory.Infrastructure.Data;

namespace SellerInventory.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IRepository<User>? _users;
    private IRepository<Category>? _categories;
    private IRepository<Product>? _products;
    private IRepository<Order>? _orders;
    private IRepository<OrderItem>? _orderItems;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);
    public IRepository<Product> Products => _products ??= new Repository<Product>(_context);
    public IRepository<Order> Orders => _orders ??= new Repository<Order>(_context);
    public IRepository<OrderItem> OrderItems => _orderItems ??= new Repository<OrderItem>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
