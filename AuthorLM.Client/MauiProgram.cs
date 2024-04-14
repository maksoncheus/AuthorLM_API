using AuthorLM.Client.Services;
using AuthorLM.Client.ViewModels;
using AuthorLM.Client.Views;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace AuthorLM.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<ApiCallService>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<BookPage>();
            builder.Services.AddTransient<BookPageViewModel>();
            builder.Services.AddSingleton<NavigationService>();
#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
