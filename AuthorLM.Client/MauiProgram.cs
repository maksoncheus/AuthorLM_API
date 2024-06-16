using AuthorLM.Client.Services;
using AuthorLM.Client.ViewModels;
using AuthorLM.Client.Views;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using System.Text;
using Syncfusion.Maui.Core.Hosting;

namespace AuthorLM.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Montserrat-VariableFont_wght.ttf", "Montserrat");
                    fonts.AddFont("Raleway-VariableFont_wght.ttf", "Raleway");
                    fonts.AddFont("Roboto-Regular.ttf", "Roboto");
                });
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddTransient<FileResolveService>();
            builder.Services.AddTransient<HttpRequestHeaderService>();
            builder.Services.AddSingleton<ApiCallService>();
            builder.Services.AddSingleton<AccountService>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<BookPageViewModel>();
            builder.Services.AddTransient<BookPage>();
            builder.Services.AddTransient<AuthorizationViewModel>();
            builder.Services.AddTransient<AuthorizationPage>();
            builder.Services.AddTransient<RegistrationPageViewModel>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<ProfilePageViewModel>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<EditProfileViewModel>();
            builder.Services.AddTransient<EditProfilePage>();
            builder.Services.AddTransient<PublishBookViewModel>();
            builder.Services.AddTransient<PublishBookPage>();
            builder.Services.AddTransient<MyLibraryPage>();
            builder.Services.AddTransient<MyLibraryViewModel>();
            builder.Services.AddTransient<CatalogPage>();
            builder.Services.AddTransient<Reader>();
            builder.Services.AddTransient<RulesPage>();
            builder.Services.AddTransient<RulesPageViewModel>();
            builder.Services.AddTransient<ReaderViewModel>();
            builder.Services.AddTransient<CatalogPageViewModel>();
            builder.Services.AddTransient<ChangePasswordPopup>();
            builder.Services.AddTransient<ChangePasswordViewModel>();
            builder.Services.AddTransientPopup<ChangePasswordPopup, ChangePasswordViewModel>();

            builder.Services.AddSingleton<NavigationService>();
#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
