using AuthorLM.Client.Services;
using DbLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.ViewModels
{
    public class BookPageViewModel : ViewModel
    {
        private readonly NavigationService _navigationService;
        private readonly ApiCallService _apiCallService;
        private int bookId;
        public override async Task OnNavigatingTo(object? parameter)
        {
            Initialize((int)parameter);
            await base.OnNavigatingTo(parameter);
        }
        private bool _isLiked;
        public bool IsLiked
        {
            get => _isLiked;
            set
            {
                _isLiked = value;
                OnPropertyChanged();
            }
        }
        public Command SetLike
        {
            get => new(async() => {
                int userId = Convert.ToInt32(Preferences.Get("userId", "2"));
                await _apiCallService.SetLike(userId, bookId);
                Refresh.Execute(null);
                });
        }
        public Command UnsetLike
        {
            get => new(async() => {
                int userId = Convert.ToInt32(Preferences.Get("userId", "2"));
                await _apiCallService.UnsetLike(userId, bookId);
                Refresh.Execute(null);
                });
        }
        private async void GetIsLiked(int id)
        {
            int userId = Convert.ToInt32(Preferences.Get("userId", "2"));
            List<Like> likes = (List<Like>)await _apiCallService.GetLikesByBookId(id);
            if (likes.AsQueryable().FirstOrDefault(l => l.Liker.Id == userId && Book.Id == id) != null)
                IsLiked = true;
            else
                IsLiked = false;
            //if (Preferences.ContainsKey("userId"))
            //{
            //    int userId = Convert.ToInt32(Preferences.Get("userId", "1"));
            //    List<Like> likes = (List<Like>) await _apiCallService.GetLikesByBookId(id);
            //    if (likes.AsQueryable().FirstOrDefault(l => l.Liker.Id == userId && Book.Id == id) != null)
            //        IsLiked = true;
            //    else
            //        IsLiked = false;
            //}
        }
        private async void Initialize(int param)
        {
            await Task.Run(() =>
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
        public BookPageViewModel(NavigationService navigationService, ApiCallService apiCallService)
        {
            _navigationService = navigationService;
            _apiCallService = apiCallService;
        }
    }
}
