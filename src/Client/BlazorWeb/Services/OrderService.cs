using System.Net.Http.Json;
using SellerInventer.Shared.Contracts.Order;

namespace SellerInventer.Client.BlazorWeb.Services;

public class OrderService : IOrderService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/v1/orders";

    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<OrderResponse>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<OrderResponse>>(BaseUrl);
        return response ?? new List<OrderResponse>();
    }

    public async Task<IReadOnlyList<OrderResponse>> GetMyOrdersAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<OrderResponse>>($"{BaseUrl}/my-orders");
        return response ?? new List<OrderResponse>();
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<OrderResponse>($"{BaseUrl}/{id}");
    }

    public async Task<OrderResponse?> CreateAsync(CreateOrderRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(BaseUrl, request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<OrderResponse>();
    }

    public async Task<OrderResponse?> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request)
    {
        var response = await _httpClient.PatchAsJsonAsync($"{BaseUrl}/{id}/status", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<OrderResponse>();
    }

    public async Task<OrderResponse?> AddItemAsync(Guid orderId, AddOrderItemRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/{orderId}/items", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<OrderResponse>();
    }

    public async Task<bool> RemoveItemAsync(Guid orderId, Guid itemId)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{orderId}/items/{itemId}");
        return response.IsSuccessStatusCode;
    }
}
