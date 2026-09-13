using EggERP.Application.Businesses;
using EggERP.Application.Categories;
using EggERP.Application.Customers;
using EggERP.Application.Expenses;
using EggERP.Application.Inventory;
using EggERP.Application.Products;
using EggERP.Application.Purchases;
using EggERP.Application.Sales;
using EggERP.Application.Suppliers;
using EggERP.Infrastructure.Businesses;
using EggERP.Infrastructure.Categories;
using EggERP.Infrastructure.Customers;
using EggERP.Infrastructure.Expenses;
using EggERP.Infrastructure.Identity;
using EggERP.Infrastructure.Inventory;
using EggERP.Infrastructure.Persistence;
using EggERP.Infrastructure.Products;
using EggERP.Infrastructure.Purchases;
using EggERP.Infrastructure.Sales;
using EggERP.Application.Users;
using EggERP.Infrastructure.Suppliers;
using EggERP.Infrastructure.Users;
using EggERP.Shared.Models;
using EggERP.Shared.Services;
using EggERP.Web.Components;
using EggERP.Web.Data;
using EggERP.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<EggERPDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("EggERPConnection")));

// Identity (registration + authentication)
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Simplified password policy: small business owners should not be forced
    // into special-character requirements. Still requires reasonable length,
    // uppercase, lowercase, and a digit.
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
})
    .AddEntityFrameworkStores<EggERPDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// Enable [Authorize] support inside Blazor components
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// Business services
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
// Product services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
// Category services
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
// Inventory services
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
// Customer services
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
// Supplier services
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
// Sale services
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISaleService, SaleService>();
// Purchase services
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
// Expense services
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
// User management services
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

// API Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add device-specific services used by the EggERP.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// Business API client (calls this same app's own API endpoints)
builder.Services.AddHttpClient<IBusinessApiService, BusinessApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7062/");
});
// Product API client (calls this same app's own API endpoints)
builder.Services.AddHttpClient<IProductApiService, ProductApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7062/");
});
// Category API client (calls this same app's own API endpoints)
builder.Services.AddHttpClient<ICategoryApiService, CategoryApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7062/");
});
// Inventory API client (calls this same app's own API endpoints)
builder.Services.AddHttpClient<IInventoryApiService, InventoryApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7062/");
});
// Customer API client (calls this same app's own API endpoints)
builder.Services.AddHttpClient<ICustomerApiService, CustomerApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7062/");
});
// Supplier API client (calls this same app's own API endpoints)
builder.Services.AddHttpClient<ISupplierApiService, SupplierApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7062/");
});
// Sale API client (calls this same app's own API endpoints)
builder.Services.AddHttpClient<ISaleApiService, SaleApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7062/");
});
// Purchase API client (calls this same app's own API endpoints)
builder.Services.AddHttpClient<IPurchaseApiService, PurchaseApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7062/");
});
// Expense API client (calls this same app's own API endpoints)
builder.Services.AddHttpClient<IExpenseApiService, ExpenseApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7062/");
});
// User management API client (calls this same app's own API endpoints)
builder.Services.AddHttpClient<IUserManagementApiService, UserManagementApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7062/");
});


var app = builder.Build();

// Seed Identity roles and initial SuperAdmin/Admin accounts
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// API Controllers
app.MapControllers();

// Blazor
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(EggERP.Shared._Imports).Assembly);

app.Run();