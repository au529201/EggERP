using EggERP.Application.Categories;
using EggERP.Infrastructure.Categories;
using EggERP.Shared.Models;
using EggERP.Shared.Services;
using EggERP.Web.Components;
using EggERP.Web.Services;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using EggERP.Application.Products;
using EggERP.Infrastructure.Products;

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