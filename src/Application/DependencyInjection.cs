using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SellerInventer.Application.Interfaces;
using SellerInventer.Application.Services;

namespace SellerInventer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<IApplicationMarker>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}

public interface IApplicationMarker { }
