using AuthorLM.Client.Services;
using CommunityToolkit.Maui.Alerts;
using DbLibrary.Entities;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageCropper;
using CommunityToolkit.Maui.Core;

namespace AuthorLM.Client.ViewModels
{
    public class EditProfileViewModel : ViewModel
    {
        private readonly AccountService _accountService;
        private readonly ApiCallService _callService;
        private readonly NavigationService _navigation;
        private readonly IPopupService _popup;
        private User _user;
        private bool _isRefreshing;
        public override Task OnNavigatingTo(object? parameter)
        {
            Task.Run(_init);
            return base.OnNavigatingTo(parameter);
        }
        private async Task _init()
        {
            User = await _callService.GetDetails();
        }
        public User User
        {
            get => _user;
            set
            {
                string path = value.PathToPhoto;
                value.PathToPhoto = "";
                _user = value;
                OnPropertyChanged();
                _user.PathToPhoto = path;
                OnPropertyChanged();
            }
        }
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }
        public Command Refresh
        {
            get => new(async () =>
            {
                IsRefreshing = true;
                await _init();
                IsRefreshing = false;
            }
            );
        }
        public Command ChangePhoto
        {
            get => new(async () =>
            {
                try
                {
                    var result = await FilePicker.PickAsync(new() { FileTypes = FilePickerFileType.Images, PickerTitle = "Выберите изображение" });
                    if (result != null)
                    {
                        if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                            result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                        {
                            var stream = await result.OpenReadAsync();
                            IImage img = PlatformImage.FromStream(stream);
                            stream.Close();
                            if (img.Height < 150 || img.Width < 150)
                            {
                                await Toast.Make("Файл должен быть больше 150px в ширину и высоту!").Show();
                                return;
                            }
                            if (new FileInfo(result.FullPath).Length > 3145728*8)
                            {
                                await Toast.Make("Размер файла не должен превышать 3МБ!").Show();
                                return;
                            }
                            FileResult? cropped = null;
                            new ImageCropper.Maui.ImageCropper()
                            {
                                PageTitle = "Обрезать изображение",
                                AspectRatioX = 1,
                                AspectRatioY = 1,
                                CropShape = ImageCropper.Maui.ImageCropper.CropShapeType.Oval,
                                Success = async (imageFile) =>
                                {
                                    cropped = new FileResult(imageFile, contentType: result.ContentType);
                                    HttpResponseMessage respone = await _callService.ChangePhoto(cropped);
                                    Refresh.Execute(null);
                                    if (respone.IsSuccessStatusCode)
                                    {
                                        await Toast.Make("Вы успешно изменили фотографию!").Show();
                                        return;
                                    }
                                    else
                                    {
                                        await Toast.Make("Что-то пошло не так ):").Show();
                                        return;
                                    }
                                },
                                Failure = async() =>
                                {
                                    await Toast.Make("Выберите файл").Show();
                                }
                            }.Show(_navigation.GetCurrentPage(), result.FullPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Toast.Make(ex.Message).Show();
                }
            });
        }
        public Command Back
        {
            get => new(async () => await _navigation.NavigateBack());
        }
        public Command ChangePassword
        {
            get => new(() => _popup.ShowPopup<ChangePasswordViewModel>());
        }
        public Command ChangeDetails
        {
            get => new(async () =>
            {
                HttpResponseMessage msg = await _callService.ChangeDetails(User.Username, User.EmailAddress, User.Status);
                await Toast.Make(await msg.Content.ReadAsStringAsync()).Show();
            });
        }
        public EditProfileViewModel(AccountService accountService, ApiCallService callService, NavigationService navigation, IPopupService popup)
        {
            _accountService = accountService;
            _callService = callService;
            _navigation = navigation;
            _popup = popup;
        }
    }
}
