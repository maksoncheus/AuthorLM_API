using AuthorLM.Client.Services;
using CommunityToolkit.Maui.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.ViewModels
{
    public class ChangePasswordViewModel : ViewModel
    {
        private readonly ApiCallService _callService;
        private string oldPassword;
        private string newPassword;
        private string confirmPassword;

        public string OldPassword
        {
            get => oldPassword;
            set
            {
                oldPassword = value;
                OnPropertyChanged();
            }
        }
        public string NewPassword
        {
            get => newPassword;
            set
            {
                newPassword = value;
                OnPropertyChanged();
            }
        }
        public string ConfirmPassword
        {
            get => confirmPassword;
            set
            {
                confirmPassword = value;
                OnPropertyChanged();
            }
        }
        public async Task<HttpResponseMessage> ChangePassword(string oldPassword, string newPassword) => await _callService.ChangePassword(oldPassword, newPassword);
        public ChangePasswordViewModel(ApiCallService callService)
        {
            _callService = callService;
            OldPassword = "";
            NewPassword = "";
            ConfirmPassword = "";
        }
    }
}
