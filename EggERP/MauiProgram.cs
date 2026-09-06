using EggERP.Services;
using EggERP.Shared.Services;
using Microsoft.Extensions.Logging;

namespace EggERP
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Add device-specific services used by the EggERP.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddHttpClient<IProductApiService, ProductApiService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7062/");
            });

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
