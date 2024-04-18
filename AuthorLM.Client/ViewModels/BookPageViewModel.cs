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

        public override async Task OnNavigatingTo(object? parameter)
        {
            Task t = new Task(() => { Initialize((int)parameter); });
            t.Start();
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
        public Command PostComment
        {
            get => new Command( async()
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
        public Command ToProfile
        {
            get => new(async (id) =>  await _navigationService.NavigateToProfilePage((int)id));
        }
        private async void Initialize(int param)
        {
            Task.Run(() =>
            {
                bookId = param;
                GetIsLiked(bookId);
                FindBookById(bookId);
                LoadComments(bookId);
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
