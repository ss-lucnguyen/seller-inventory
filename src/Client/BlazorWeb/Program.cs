using Blazored.LocalStorage;
using DevExpress.Blazor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SellerInventer.Client.BlazorWeb;
using SellerInventer.Client.BlazorWeb.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddDevExpressBlazor(options => {
    options.SizeMode = SizeMode.Medium;
});

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped(sp =>
{
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var handler = new AuthorizationMessageHandler(localStorage);
    handler.InnerHandler = new HttpClientHandler();

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000")
    };
});

await builder.Build().RunAsync();
