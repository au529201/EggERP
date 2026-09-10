using EggERP.Application.Categories;
using EggERP.Application.Customers;
using EggERP.Application.Expenses;
using EggERP.Application.Inventory;
using EggERP.Application.Products;
using EggERP.Application.Purchases;
using EggERP.Application.Sales;
using EggERP.Application.Suppliers;
using EggERP.Infrastructure.Categories;
using EggERP.Infrastructure.Customers;
using EggERP.Infrastructure.Expenses;
using EggERP.Infrastructure.Inventory;
using EggERP.Infrastructure.Persistence;
using EggERP.Infrastructure.Products;
using EggERP.Infrastructure.Purchases;
using EggERP.Infrastructure.Sales;
using EggERP.Infrastructure.Suppliers;
using EggERP.Shared.Models;
using EggERP.Shared.Services;
using EggERP.Web.Components;
using EggERP.Web.Services;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<EggERPDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("EggERPConnection")));

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


var app = builder.Build();

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
app.UseAntiforgery();

// API Controllers
app.MapControllers();

// Blazor
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(EggERP.Shared._Imports).Assembly);

app.Run();