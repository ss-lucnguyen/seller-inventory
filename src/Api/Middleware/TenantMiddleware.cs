using System.Security.Claims;
using SellerInventory.Application.Interfaces;
using SellerInventory.Domain.Enums;

namespace SellerInventory.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var storeIdClaim = context.User.FindFirst("StoreId")?.Value;
            var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                Guid? storeId = Guid.TryParse(storeIdClaim, out var sid) ? sid : null;
                var role = Enum.TryParse<UserRole>(roleClaim, out var parsedRole) ? parsedRole : UserRole.Staff;

                tenantContext.SetTenant(storeId, userId, role);
            }
        }

        await _next(context);
    }
}

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}
