using AuthorLM.Client.Services;
using DbLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AuthorLM.Client.ViewModels
{
    public class MainPageViewModel : ViewModel
    {
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
        private bool _isFlyoutVisible;
        public bool IsFlyoutVisible
        {
            get => _isFlyoutVisible;
            set
            {
                _isFlyoutVisible = value;
                OnPropertyChanged();
            }
        }
        public Command OpenFlyout
        {
            get => new(() => IsFlyoutVisible = true);
        }
        public Command CloseFlyout
        {
            get => new(() => IsFlyoutVisible = false);
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
        public Command RefreshPage
        {
            get => new(async () =>
            {
                IsRefreshing = true;
                await Task.Run(GetBooks);
                IsRefreshing = false;
            });
        }
        public override Task OnNavigatedTo()
        {
            CloseFlyout.Execute(null);
            RefreshPage.Execute(null);
            return base.OnNavigatedTo();
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

        private readonly ApiCallService _apiService;
        private readonly NavigationService _navigationService;
        public MainPageViewModel(ApiCallService apiService, NavigationService navigationService)
        {
            _apiService = apiService;
            _navigationService = navigationService;
            GetBooks();
        }
        private async void GetBooks()
        {
            List<Book> books = (List<Book>)await _apiService.GetAllBooks();
            PopularBooks = new ObservableCollection<Book>(books.OrderBy(b => b.Rating).Take(10));
            NewBooks = new ObservableCollection<Book>(books.OrderByDescending(b => b.PublicationDate).Take(10));
            MostLikedBooks = new ObservableCollection<Book>(books.OrderBy(b => b.Rating).Take(10));
        }
    }
}
