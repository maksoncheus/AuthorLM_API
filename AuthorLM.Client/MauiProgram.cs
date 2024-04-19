using AuthorLM.Client.Services;
using AuthorLM.Client.ViewModels;
using AuthorLM.Client.Views;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;

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
