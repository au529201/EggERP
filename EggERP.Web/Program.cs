using EggERP.Shared.Services;
using EggERP.Web.Components;
using EggERP.Web.Services;
using EggERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using EggERP.Application.Products;
using EggERP.Infrastructure.Products;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EggERPDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("EggERPConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add device-specific services used by the EggERP.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(EggERP.Shared._Imports).Assembly);

app.Run();
