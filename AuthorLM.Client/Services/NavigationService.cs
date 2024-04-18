using AuthorLM.Client.ViewModels;
using AuthorLM.Client.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.Services
{
    public class NavigationService
    {
        readonly IServiceProvider _services;
        protected INavigation Navigation
        {
            get
            {
                INavigation? navigation = Application.Current?.MainPage?.Navigation;
                if (navigation is not null)
                    return navigation;
                else
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    throw new Exception();
                }
            }
        }
        public NavigationService(IServiceProvider services)
            => _services = services;
        public Task NavigateBack()
        {
            if (Navigation.NavigationStack.Count > 1)
                return Navigation.PopAsync();
            throw new InvalidOperationException("No pages to navigate back to!");
        }
        public Task NavigateToBookPage(int id)
            => NavigateToPage<BookPage>(id);
        public Task NavigateToMainPage()
            => NavigateToPage<MainPage>();
        public Task NavigateToAuthorizationPage()
            => NavigateToPage<AuthorizationPage>();
        public Task NavigateToRegistrationPage()
            => NavigateToPage<RegistrationPage>();
        public Task NavigateToProfilePage(int id)
        {
            return NavigateToPage<ProfilePage>(id);
        }
        public Task NavigateToRoot()
            => Navigation.PopToRootAsync();
        private async Task NavigateToPage<T>(object? parameter = null) where T : Page
        {
            var toPage = ResolvePage<T>();
            if (toPage is not null)
            {
                toPage.NavigatedTo += Page_NavigatedTo;
                var toViewModel = GetPageViewModel(toPage);
                if (toViewModel is not null)
                    await toViewModel.OnNavigatingTo(parameter);
                await Navigation.PushAsync(toPage, true);
                toPage.NavigatedFrom += Page_NavigatedFrom;
            }    
            else
                throw new InvalidOperationException($"Unable to resolve type {typeof(T).FullName}");
        }
        private async void Page_NavigatedTo(object? sender, NavigatedToEventArgs e)
            => await CallNavigatedTo(sender as Page);
        private Task CallNavigatedTo(Page? p)
        {
            var fromViewModel = GetPageViewModel(p);
            if (fromViewModel is not null)
                return fromViewModel.OnNavigatedTo();
            return Task.CompletedTask;
        }
        private async void Page_NavigatedFrom(object? sender, NavigatedFromEventArgs e)
        {
            bool isForwardNavigation = Navigation.NavigationStack.Count > 1
                && Navigation.NavigationStack[^2] == sender;
            if (sender is Page thisPage)
            {
                if (!isForwardNavigation)
                {
                    thisPage.NavigatedTo -= Page_NavigatedTo;
                    thisPage.NavigatedFrom -= Page_NavigatedFrom;
                }
                await CallNavigatedFrom(thisPage, isForwardNavigation);
            }
        }
        private Task CallNavigatedFrom(Page p, bool isForward)
        {
            var fromViewModel = GetPageViewModel(p);
            if (fromViewModel is not null)
                return fromViewModel.OnNavigatedFrom(isForward);
            return Task.CompletedTask;
        }

        private ViewModel? GetPageViewModel(Page? p)
            => p?.BindingContext as ViewModel;
        private T? ResolvePage<T>() where T : Page
            => _services.GetService<T>();
    }
}
