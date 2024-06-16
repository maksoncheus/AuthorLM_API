using AuthorLM.Client.Services;
using DbLibrary.Entities;
using Microsoft.Maui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace AuthorLM.Client.ViewModels
{
    public class MainPageViewModel : ViewModel
    {
        private bool _isInitialized = true;
        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                _isInitialized = value;
                OnPropertyChanged();
            }
        }
        private bool _isAdmin = false;
        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                _isAdmin = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Book> _newBooks;
        public ObservableCollection<Book> NewBooks
        {
            get { return _newBooks; }
            set
            {
                _newBooks = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Book> _popularBooks;
        public ObservableCollection<Book> PopularBooks
        {
            get { return _popularBooks; }
            set
            {
                _popularBooks = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Book> _mostLikedBooks;
        public ObservableCollection<Book> MostLikedBooks
        {
            get { return _mostLikedBooks; }
            set
            {
                _mostLikedBooks = value;
                OnPropertyChanged();
            }
        }
        private Book? _selectedBook;
        public Book? SelectedBook
        {
            get => _selectedBook;
            set
            {
                _selectedBook = value;
                OnPropertyChanged();
            }
        }
        public Command NavigateToBook
        {
            get => new(async (o) =>
            {
                if (o == null) return;
                Book b = (Book)o;
                await _navigationService.NavigateToBookPage(b.Id);
                SelectedBook = null;
            });
        }
        public Command NavigateToLibrary
        {
            get => new(async () => await _navigationService.NavigateToMyLibrary());
        }
        public Command NavigateToRules
        {
            get => new(async () => await _navigationService.NavigateToRules());
        }
        public Command NavigateToCatalog
        {
            get => new(async (searchString) => await _navigationService.NavigateToCatalog((string?)searchString));
        }
        public string AppName { get => App.APP_NAME; }
        public Command RefreshPage
        {
            get => new(async () =>
            {
                IsRefreshing = true;
                await _init();
                IsRefreshing = false;
            });
        }
        public override async Task OnNavigatedTo()
        {
            if(_books == null) Books = new();
            Task.Run(_init);
        }
        public Command NavigateToAuthorizationPage
        {
            get => new(async() => await _navigationService.NavigateToAuthorizationPage());
        }
        public Command LogOut
        {
            get => new(async () =>
            {
                _accountService.LogOut();
                IsLoggedIn = _accountService.IsLoggedIn;
                CurrentUser=null;
            });
        }
        private async Task _init()
        {
            await Task.Run(async() =>
            {
                IsLoggedIn = _accountService.IsLoggedIn;
                if (_accountService.IsLoggedIn)
                {
                    CurrentUser = await _apiService.GetDetails();
                }
            }).ContinueWith(async (t) => await Task.Run(GetBooks));
            IsInitialized = false;
        }
        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser ?? new User();
            set
            {
                if (value == null)
                {
                    _currentUser = new User();
                    _accountService.LogOut();
                    IsAdmin = false;
                    OnPropertyChanged();
                    return;
                }
                string path = value.PathToPhoto;
                value.PathToPhoto = "";
                _currentUser = value;
                IsAdmin = _accountService.IsAdmin;
                OnPropertyChanged();
                _currentUser.PathToPhoto = path;
                OnPropertyChanged();
            }
        }
        public Command ToProfile
        {
            get => new(async (id) => await _navigationService.NavigateToProfilePage((int)id));
        }
        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }
        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                _isLoggedIn = value;
                OnPropertyChanged();
            }
        }
        private readonly ApiCallService _apiService;
        private readonly NavigationService _navigationService;
        private readonly AccountService _accountService;
        public MainPageViewModel(ApiCallService apiService, NavigationService navigationService, AccountService accountService)
        {
            _apiService = apiService;
            _navigationService = navigationService;
            _accountService = accountService;
        }
        private ObservableCollection<Book> _books;
        public ObservableCollection<Book> Books
        {
            get => _books;
            set
            {
                _books = value;
                OnPropertyChanged();
            }
        }
        private async void GetBooks()
        {
            if(Books == null)
            {
                PopularBooks = new();
                NewBooks = new();
                MostLikedBooks = new();
            }
            IEnumerable<Book> books = await _apiService.GetAllBooks();
                Books = new(books);
                PopularBooks = new ObservableCollection<Book>(_books.OrderBy(b => b.Rating).Take(10));
                NewBooks = new ObservableCollection<Book>(_books.OrderByDescending(b => b.PublicationDate).Take(10));
                MostLikedBooks = new ObservableCollection<Book>(_books.OrderByDescending(b => b.Rating).Take(10));
        }
    }
}
