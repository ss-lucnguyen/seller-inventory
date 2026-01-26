using SellerInventory.Application.DTOs.Customer;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Entities;
using SellerInventory.Domain.Enums;

namespace SellerInventory.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CustomerService(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer is null) return null;

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && customer.StoreId != _tenantContext.CurrentStoreId)
        {
            return null;
        }

        return MapToDto(customer);
    }

    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Customer> customers;

        if (_tenantContext.IsSystemAdmin)
        {
            customers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);
        }
        else if (_tenantContext.CurrentStoreId.HasValue)
        {
            customers = await _unitOfWork.Customers.FindAsync(
                c => c.StoreId == _tenantContext.CurrentStoreId.Value, cancellationToken);
        }
        else
        {
            return new List<CustomerDto>();
        }

        return customers.Select(MapToDto).ToList();
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.CurrentStoreId.HasValue)
        {
            throw new UnauthorizedAccessException("No store context available");
        }

        var customer = new Customer
        {
            Name = dto.Name,
            Gender = dto.Gender,
            Mobile = dto.Mobile,
            Address = dto.Address,
            AccountNumber = GenerateAccountNumber(),
            IsDefault = false,
            StoreId = _tenantContext.CurrentStoreId.Value,
            IsActive = true
        };

        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(customer);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer with id {id} not found");

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && customer.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Customer does not belong to your store");
        }

        // Don't allow modifying default customer's name or active status
        if (customer.IsDefault)
        {
            throw new InvalidOperationException("Cannot modify the default customer");
        }

        customer.Name = dto.Name;
        customer.Gender = dto.Gender;
        customer.Mobile = dto.Mobile;
        customer.Address = dto.Address;
        customer.IsActive = dto.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer with id {id} not found");

        // Verify tenant access
        if (!_tenantContext.IsSystemAdmin && customer.StoreId != _tenantContext.CurrentStoreId)
        {
            throw new UnauthorizedAccessException("Customer does not belong to your store");
        }

        // Don't allow deleting default customer
        if (customer.IsDefault)
        {
            throw new InvalidOperationException("Cannot delete the default customer");
        }

        await _unitOfWork.Customers.DeleteAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CustomerDto> GetOrCreateDefaultAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.CurrentStoreId.HasValue)
        {
            throw new UnauthorizedAccessException("No store context available");
        }

        var storeId = _tenantContext.CurrentStoreId.Value;

        // Try to find existing default customer
        var customers = await _unitOfWork.Customers.FindAsync(
            c => c.StoreId == storeId && c.IsDefault, cancellationToken);

        var defaultCustomer = customers.FirstOrDefault();
        if (defaultCustomer is not null)
        {
            return MapToDto(defaultCustomer);
        }

        // Create default customer (Anonymous / Khach vang lai)
        var customer = new Customer
        {
            Name = "Anonymous",
            Gender = Gender.Unknown,
            AccountNumber = GenerateAccountNumber(),
            IsDefault = true,
            StoreId = storeId,
            IsActive = true
        };

        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(customer);
    }

    private static string GenerateAccountNumber()
    {
        return $"CUST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static CustomerDto MapToDto(Customer customer) =>
        new(
            customer.Id,
            customer.Name,
            customer.Gender,
            customer.Mobile,
            customer.AccountNumber,
            customer.Address,
            customer.IsDefault,
            customer.IsActive,
            customer.CreatedAt,
            customer.UpdatedAt
        );
}
