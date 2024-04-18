using AuthorLM.Client.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.ViewModels
{
    public class AuthorizationViewModel : ViewModel
    {
        private readonly AccountService _accountService;
        private readonly NavigationService _navigationService;
        private readonly ApiCallService _callService;
        private string _authString;
        private string _password;
        public string AuthString
        {
            get => _authString;
            set
            {
                _authString = value;
                OnPropertyChanged();
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }
        public Command Back
        {
            get => new(async () => await _navigationService.NavigateToRoot());
        }

        public Command ToRegistration
        {
            get => new Command(async () => await _navigationService.NavigateToRegistrationPage());
        }
        public string AppName
        {
            get => App.APP_NAME;
        }
        public Command LogIn
        {
            get => new(async() =>
            {
                var response = await _callService.Authenticate(_authString, _password);
                if(response.StatusCode!=HttpStatusCode.OK)
                {
                    await Toast.Make("Вы ввели неверные данные!").Show();
                    return;
                }
                string token = await response.Content.ReadAsStringAsync();
                _accountService.LogIn(token);
                await _navigationService.NavigateToRoot();
                await Toast.Make("Вы успешно авторизовались").Show();
            });
        }
        public AuthorizationViewModel(AccountService accountService, NavigationService navigationService, ApiCallService callService)
        {
            _accountService = accountService;
            _navigationService = navigationService;
            _callService = callService;
        }
    }
}
