using AuthorLM.Client.Services;
using CommunityToolkit.Maui.Alerts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AuthorLM.Client.ViewModels
{
    public class RegistrationPageViewModel : ViewModel
    {
        private readonly NavigationService _navigationService;
        private readonly ApiCallService _callService;
        private readonly AccountService _accountService;
        private string _username;
        private string _email;
        private string _password;
        private string _confirmPassword;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
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
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }
        public string AppName
        {
            get => App.APP_NAME;
        }
        public Command ToAuthorization
        {
            get => new Command(async () => await _navigationService.NavigateToAuthorizationPage());
        }
        public Command Back
        {
            get => new(async () => await _navigationService.NavigateToRoot());
        }
        public Command Register
        {
            get => new(async() =>
            {
                if(string.IsNullOrEmpty(_username?.Trim()) || string.IsNullOrEmpty(_email?.Trim()) || string.IsNullOrEmpty(_password?.Trim()) || string.IsNullOrEmpty(_confirmPassword?.Trim()))
                {
                    await Toast.Make("Заполните все поля").Show();
                    return;
                }
                if(_password != _confirmPassword)
                {
                    await Toast.Make("Пароли не совпадают").Show();
                    return;
                }
                HttpResponseMessage response = await _callService.Register(_username, _email, _password);
                if(!response.IsSuccessStatusCode)
                {
                    try
                    {
                        await Toast.Make(Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(await response.Content.ReadAsStringAsync()).FirstOrDefault()).Show();
                    }
                    catch
                    {
                        await Toast.Make("Что-то пошло не так").Show();
                    }
                    return;
                }
                HttpResponseMessage responseLogin = await _callService.Authenticate(_username, _password);
                string responseContent = await responseLogin.Content.ReadAsStringAsync();
                var def = new { token = "", isAdmin = false };
                var result = JsonConvert.DeserializeAnonymousType(responseContent, def);
                _accountService.LogIn(result.token, result.isAdmin);
                await _navigationService.NavigateToRoot();
                await Toast.Make("Вы успешно зарегистрировались!").Show();
            });
        }
        public RegistrationPageViewModel(NavigationService navigationService, ApiCallService callService, AccountService accountService)
        {
            _navigationService = navigationService;
            _callService = callService;
            _accountService = accountService;
        }
    }
}
