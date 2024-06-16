using AuthorLM.Client.Services;
using DbLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.ViewModels
{
    public class MyLibraryViewModel : ViewModel
    {
        private readonly NavigationService _navigation;
        private readonly ApiCallService _callService;
        private ObservableCollection<Book> _readingBooks;
        private ObservableCollection<Book> _readBooks;
        private ObservableCollection<Book> _favoriteBooks;
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
        public ObservableCollection<Book> ReadingBooks
        {
            get => _readingBooks;
            set
            {
                _readingBooks = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Book> ReadBooks
        {
            get => _readBooks;
            set
            {
                _readBooks = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Book> FavoriteBooks
        {
            get => _favoriteBooks;
            set
            {
                _favoriteBooks = value;
                OnPropertyChanged();
            }
        }
        public Command NavigateToBook
        {
            get => new(async (o) =>
            {
                if (o == null) return;
                Book b = (Book)o;
                await _navigation.NavigateToBookPage(b.Id);
            });
        }
        public override async Task OnNavigatedTo()
        {
            Task.Run(_init);
        }
        public override async Task OnNavigatedFrom(bool isForwardNavigation)
        {
            Refresh.Execute(null);
        }
        private Command _refresh;
        public Command Refresh
        {
            get => _refresh ??= new Command(async() =>
            {
                IsRefreshing = true;
                await Task.Run(_init);
                IsRefreshing = false;
            });
        }
        private async Task _init()
        {
            await Task.Run(async () =>
            {
                ReadingBooks = new(await _callService.GetReadingBooks());
                ReadBooks = new(await _callService.GetReadBooks());
                FavoriteBooks = new(await _callService.GetFavoriteBooks());
            });
        }
        public MyLibraryViewModel(NavigationService navigation, ApiCallService callService)
        {
            _navigation = navigation;
            _callService = callService;
        }
    }
}
