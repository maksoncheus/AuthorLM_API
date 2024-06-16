using AuthorLM.Client.Models;
using AuthorLM.Client.Services;
using CommunityToolkit.Maui.Alerts;
using DbLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.ViewModels
{
    public class BookPageViewModel : ViewModel
    {
        private readonly AccountService _accountService;
        private readonly NavigationService _navigationService;
        private readonly ApiCallService _apiCallService;
        public int bookId;
        private bool _isLiked;
        private bool _canDelete = false;
        private ObservableCollection<BookLibraryEntryModel> _entries = new()
        {
            new() {EntryName = "Reading", Action = "Читаю"},
            new() {EntryName = "Read", Action = "Прочитано"},
            new() {EntryName = "Favorite", Action = "Избранное"}
        };
        private BookLibraryEntryModel? _entry = null;
        public bool CanDelete
        {
            get => _canDelete;
            set
            {
                _canDelete = value;
                OnPropertyChanged();
            }
        }
        public bool IsAdmin
        {
            get => _accountService.IsAdmin;
        }
        public override async Task OnNavigatingTo(object? parameter)
        {
            Task t = new Task(() => { Initialize((int)parameter); });
            t.Start();
        }
        private int _selectedEntryIndex;
        public int SelectedEntryIndex
        {
            get => _selectedEntryIndex;
            set
            {
                _selectedEntryIndex = value;
                OnPropertyChanged();
            }
        }
        public BookLibraryEntryModel? SelectedEntry
        {
            get => _entry;
            set
            {
                if (_entry == value) return;
                if (_accountService.IsLoggedIn)
                {
                    if (value == null)
                    {
                        _entry = null;
                        _apiCallService.RemoveBookFromLibrary(bookId);
                    }
                    else
                    {
                        _entry = value;
                        _apiCallService.AddBookToLibrary(bookId, value.EntryName.ToLower());
                    }
                }
                else Toast.Make("Сначала авторизуйтесь!").Show();
                OnPropertyChanged();
            }
        }
        public ObservableCollection<BookLibraryEntryModel> Entries
        {
            get => _entries;
            set
            {
                _entries = value;
                OnPropertyChanged();
            }
        }
        public bool IsLiked
        {
            get => _isLiked;
            set
            {
                _isLiked = value;
                OnPropertyChanged();
            }
        }
        private string _commentText;
        public string CommentText
        {
            get => _commentText;
            set
            {
                _commentText = value;
                OnPropertyChanged();
            }
        }
        private Command _deleteBookFromLibrary;
        public Command DeleteBookFromLibrary
        {
            get => _deleteBookFromLibrary ??= new Command(
                async () =>
                {
                    await Task.Delay(500);
                    SelectedEntry = null;
                });
        }
        private Command _postComment;
        public Command PostComment
        {
            get => _postComment ??= new Command(async ()
                =>
            {

                if (string.IsNullOrEmpty(_commentText))
                {
                    await Toast.Make("Напишите комментарий!").Show();
                    return;
                }
                string commentText = _commentText;

                if (!_accountService.IsLoggedIn)
                {
                    await Toast.Make("Для выполнения данного действия вам необходимо авторизоваться").Show();
                    return;
                }
                await _apiCallService.PostComment(bookId, _commentText);
                CommentText = string.Empty;
                Refresh.Execute(null);
            });
        }
        public Command SetLike
        {
            get => new(SetLikeMethod);
        }
        private async void SetLikeMethod()
        {
            if (!_accountService.IsLoggedIn)
            {
                await Toast.Make("Для выполнения данного действия вам необходимо авторизоваться").Show();
                return;
            }
            IsLiked = true;
            Book.Rating += 1;
            await _apiCallService.SetLike(bookId);
            Refresh.Execute(null);
        }
        public Command UnsetLike
        {
            get => new(UnsetLikeMethod);
        }
        private async void UnsetLikeMethod()
        {
            if (!_accountService.IsLoggedIn)
            {
                await Toast.Make("Для выполнения данного действия вам необходимо авторизоваться").Show();
                return;
            }
            IsLiked = false;
            Book.Rating -= 1;
            await _apiCallService.UnsetLike(bookId);
            Refresh.Execute(null);
        }
        private async void GetIsLiked(int id)
        {
            if (_accountService.IsLoggedIn)
            {
                int userId = (await _apiCallService.GetDetails()).Id;
                List<Like> likes = (List<Like>)await _apiCallService.GetLikesByBookId(id);
                if (likes.AsQueryable().FirstOrDefault(l => l.Liker.Id == userId && Book.Id == id) != null)
                {
                    IsLiked = true;
                    return;
                }
            }
            IsLiked = false;
        }
        private async void GetEntry(int id)
        {
            if (_accountService.IsLoggedIn)
            {
                string entry = await _apiCallService.GetLibraryEntry(id);
                if (entry != "none")
                {
                    SelectedEntry = Entries.FirstOrDefault(e => e.EntryName.ToLower() == entry);
                }
            }
        }

        public Command ToProfile
        {
            get => new(async (id) => await _navigationService.NavigateToProfilePage((int)id));
        }
        private async void Initialize(int param)
        {
            Task.Run(() =>
            {
                bookId = param;
                GetIsLiked(bookId);
                FindBookById(bookId);
                LoadComments(bookId);
                GetEntry(bookId);
            });
        }
        private async Task _init(int param)
            => await Task.Run(() => Initialize(param));
        private bool _isRefreshing;
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
                await _init(bookId);
                IsRefreshing = false;
            }
            );
        }
        public Command Back
        {
            get => new(async () => await _navigationService.NavigateBack());
        }
        private async void FindBookById(int bookId)
        {
            List<Book> b = (List<Book>)await _apiCallService.GetAllBooks();
            Book = b.FirstOrDefault(_book => _book.Id == bookId);
        }
        private async void LoadComments(int bookId)
        {
            Comments = new ObservableCollection<Comment>((await _apiCallService.GetCommentsByBookId(bookId)).OrderByDescending(b => b.TimeStamp));
        }
        private Book _book;
        public Book Book
        {
            get => _book ?? new Book();
            set
            {
                _book = value;
                OnPropertyChanged();
            }
        }
        private Command _removeComment;
        public Command RemoveComment
        {
            get => _removeComment ??= new(async (id) =>
            {
                HttpResponseMessage response = await _apiCallService.RemoveComment((int)id);
                if (!response.IsSuccessStatusCode)
                {
                    await Toast.Make("Не удалось удалить комментарий").Show();
                    return;
                }
                await Toast.Make("Комментарий успешно удален").Show();
                Comments.Remove(Comments.First(c => c.Id == (int)id));
            });
        }
        private Command _openBook;
        public Command OpenBook
        {
            get => _openBook ??= new(async (id) =>
            {
                if (!_accountService.IsLoggedIn)
                {
                    await Toast.Make("Читать книги могут только авторизованные пользователи").Show();
                    return;
                }
                await Permissions.RequestAsync<Permissions.StorageRead>();
                await Permissions.RequestAsync<Permissions.StorageWrite>();
                await _navigationService.NavigateToReader(id);
            });
        }
        private ObservableCollection<Comment> _comments;
        public ObservableCollection<Comment> Comments
        {
            get => _comments;
            set
            {
                _comments = value;
                OnPropertyChanged();
            }
        }
        public BookPageViewModel(NavigationService navigationService, ApiCallService apiCallService, AccountService accountService)
        {
            _navigationService = navigationService;
            _apiCallService = apiCallService;
            _accountService = accountService;
        }
    }
}
