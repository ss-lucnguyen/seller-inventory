using SellerInventory.Shared.Contracts.Customer;

namespace SellerInventory.Client.BlazorWeb.Services;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerResponse>> GetAllAsync();
    Task<CustomerResponse?> GetByIdAsync(Guid id);
    Task<CustomerResponse?> GetDefaultAsync();
    Task<CustomerResponse?> CreateAsync(CreateCustomerRequest request);
    Task<CustomerResponse?> UpdateAsync(Guid id, UpdateCustomerRequest request);
    Task<bool> DeleteAsync(Guid id);
}
