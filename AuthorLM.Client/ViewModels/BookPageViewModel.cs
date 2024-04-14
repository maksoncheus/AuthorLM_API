using AuthorLM.Client.Services;
using DbLibrary.Data.Entities;
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
            await Initialize((int)parameter);
            await base.OnNavigatingTo(parameter);
        }
        private async Task Initialize(int param)
        {
            await Task.Run(() =>
            {
                bookId = param;
                FindBookById(bookId);
                LoadComments(bookId);
            });
        }
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
                await Initialize(bookId);
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
            get => _book;
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
