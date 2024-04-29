using AuthorLM.Client.Services;
using CommunityToolkit.Maui.Alerts;
using DbLibrary.Entities;
using Microsoft.Maui.Graphics.Platform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.ViewModels
{
    public class PublishBookViewModel : ViewModel
    {
        private readonly NavigationService _navigation;
        private readonly ApiCallService _callService;
        private string _title;
        private string _description;
        private int _genreId;
        private ImageSource _coverImage;
        private FileResult? _cover;
        private FileResult? _content;
        private ObservableCollection<Genre> _genres;
        private Genre? _selectedGenre;
        public override Task OnNavigatingTo(object? parameter)
        {
            Task.Run(_init);
            return base.OnNavigatingTo(parameter);
        }
        public Command Back
        {
            get => new(async () => await _navigation.NavigateBack());
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }
        
        public int GenreId
        {
            get => _genreId;
            set
            {
                _genreId = value;
                OnPropertyChanged();
            }
        }
        public ImageSource CoverImage
        {
            get => _coverImage;
            set
            {
                _coverImage = "";
                OnPropertyChanged();
                _coverImage = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Genre> Genres
        {
            get => _genres;
            set
            {
                _genres = value;
                OnPropertyChanged();
            }
        }
        public Genre? SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                _selectedGenre = value;
                OnPropertyChanged();
            }
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
                            Microsoft.Maui.Graphics.IImage img = PlatformImage.FromStream(stream);
                            stream.Close();
                            if (img.Height < 160 || img.Width < 120)
                            {
                                await Toast.Make("Файл должен быть не менее 120x160 px!").Show();
                                return;
                            }
                            if (new FileInfo(result.FullPath).Length > 3145728)
                            {
                                await Toast.Make("Размер файла не должен превышать 3МБ!").Show();
                                return;
                            }
                            //FileResult? cropped = null;
                            new ImageCropper.Maui.ImageCropper()
                            {
                                PageTitle = "Обрезать изображение",
                                AspectRatioX = 3,
                                AspectRatioY = 4,
                                CropShape = ImageCropper.Maui.ImageCropper.CropShapeType.Rectangle,
                                Success = async (imageFile) =>
                                {
                                    _cover = new FileResult(imageFile, contentType: result.ContentType);
                                    CoverImage = ImageSource.FromFile(imageFile);
                                },
                                Failure = async () =>
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
        public Command ChangeContent
        {
            get => new(async () =>
            {
                try
                {
                    var result = await FilePicker.PickAsync(new() { PickerTitle = "Выберите файл (fb2, txt)" });
                    if (result != null)
                    {
                        if (result.FileName.EndsWith("fb2", StringComparison.OrdinalIgnoreCase) ||
                            result.FileName.EndsWith("txt", StringComparison.OrdinalIgnoreCase))
                        {
                            _content = result;
                            return;
                        }
                    }
                        await Toast.Make("Файл не найден").Show();
                }
                catch (Exception ex)
                {
                    await Toast.Make(ex.Message).Show();
                }
            });
        }
        public Command Publish
        {
            get => new(async () =>
            {
                if (_content == null || _selectedGenre == null || string.IsNullOrEmpty(_title))
                {
                    await Toast.Make("Заполните все поля!").Show();
                    return;
                }
                HttpResponseMessage respone = await _callService.PublishBook(Title, Description, _selectedGenre.Id, _cover, _content);
                if (respone.IsSuccessStatusCode)
                {
                    await Toast.Make("Успешно!").Show();
                    await _navigation.NavigateBack();
                    return;
                }
                else
                {
                    await Toast.Make(await respone.Content.ReadAsStringAsync()).Show();
                    return;
                }
            });
        }
        public PublishBookViewModel(NavigationService nav,  ApiCallService callService)
        {
            Title = string.Empty;
            Description = string.Empty;
            GenreId = 0;
            _cover = null;
            _content = null;
            _navigation = nav;
            _callService = callService;
        }
        private async Task _init()
        {
            Genres = new(await _callService.GetGenres());
            SelectedGenre = Genres.FirstOrDefault();
        }
    }
}
