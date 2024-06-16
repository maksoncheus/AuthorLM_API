using AuthorLM.Client.Services;
using DbLibrary.Entities;
using DbLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.ViewModels
{
    public class CatalogPageViewModel : ViewModel
    {
        private const int PAGE_SIZE = 4;
        private readonly ApiCallService _callService;
        private readonly NavigationService _navigationService;
        private bool _needToReSearch = false;
        private bool _hasPrev = false;
        private bool _hasNext = false;
        private ObservableCollection<Genre> _genreList;
        private ObservableCollection<string> _sordDesc;
        private IEnumerable<Book> booksRaw;
        private IEnumerable<Book> booksFiltered;
        private Command _search;
        private Command _previous;
        private Command _next;
        private string _userSearchString;
        private Genre? _userSelectedGenre;
        private int _sortIndex;
        private int _pageNumber;
        public override Task OnNavigatingTo(object? parameter)
        {
            string? search = (string?)parameter;
            
            Task.Run(() => _init(search));
            return base.OnNavigatingTo(parameter);
        }
        private PaginatedList<Book> _books;
        public PaginatedList<Book> Books
        {
            get => _books;
            set
            {
                _books = value;
                OnPropertyChanged();
            }
        }
        private async void _init(string? param)
        {
            booksRaw = await _callService.GetAllBooks();
            booksFiltered = booksRaw.ToList();
            if(param != null)
            {
                booksFiltered = booksFiltered.Where(
                    b => b.Title.ToLower().Contains(param.ToLower())
                    ||
                    b.Description.ToLower().Contains(param.ToLower())
                    ||
                    b.Author.Username.ToLower().Contains(param.ToLower())
                    ||
                    b.Author.EmailAddress.ToLower().Contains(param.ToLower())).ToList();
            }
            Books = await PaginatedList<Book>.CreateAsync(booksFiltered, 1, PAGE_SIZE);
            Genres = new(await _callService.GetGenres());
            SortDesc = new() { "Не сортировать", "Нравится", "Дата публикации"};
            _userSearchString = param ?? string.Empty;
            _userSelectedGenre = null;
            _sortIndex = 0;
            _pageNumber = 1;
            HasPrev = Books.HasPreviousPage;
            HasNext = Books.HasNextPage;
            OnPropertyChanged(nameof(UserSearchString));
            OnPropertyChanged(nameof(UserSelectedGenre));
            OnPropertyChanged(nameof(SortIndex));
            OnPropertyChanged(nameof(PageNumber));
        }
        public ObservableCollection<Genre> Genres
        {
            get => _genreList;
            set
            {
                _genreList = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> SortDesc
        {
            get => _sordDesc;
            set
            {
                _sordDesc = value;
                OnPropertyChanged();
            }
        }
        public bool HasPrev
        {
            get => _hasPrev;
            set
            {
                _hasPrev = value;
                OnPropertyChanged();
            }
        }
        public bool HasNext
        {
            get => _hasNext;
            set
            {
                _hasNext = value;
                OnPropertyChanged();
            }
        }
        public string UserSearchString
        {
            get => _userSearchString;
            set
            {
                if(_userSearchString != value)
                {
                    _userSearchString = value;
                    _needToReSearch = true;
                    OnPropertyChanged();
                }
            }
        }
        public Genre? UserSelectedGenre
        {
            get => _userSelectedGenre;
            set
            {
                if(_userSelectedGenre  != value)
                {
                    _userSelectedGenre = value;
                    _needToReSearch = true;
                    OnPropertyChanged();
                }
            }
        }
        public int SortIndex
        {
            get => _sortIndex;
            set
            {
                if(_sortIndex != value)
                {
                    _sortIndex = value;
                    _needToReSearch = true;
                    OnPropertyChanged();
                }
            }
        }
        public int PageNumber
        {
            get => _pageNumber;
            set
            {
                if(_pageNumber != value)
                {
                    _pageNumber = value;
                    _needToReSearch = true;
                    OnPropertyChanged();
                }
            }
        }
        public Command Search
        {
            get => _search ??= new(async () =>
            {
                booksFiltered = booksRaw.ToList();
                if(!string.IsNullOrEmpty(_userSearchString))
                {
                    booksFiltered = booksFiltered.Where(
                        b => b.Title.ToLower().Contains(_userSearchString.ToLower())
                        ||
                        b.Description.ToLower().Contains(_userSearchString.ToLower())
                        ||
                        b.Author.Username.ToLower().Contains(_userSearchString.ToLower())
                        ||
                        b.Author.EmailAddress.ToLower().Contains(_userSearchString.ToLower())).ToList();
                }
                if(_userSelectedGenre != null)
                {
                    booksFiltered = booksFiltered.Where(b => b.Genre.Id == _userSelectedGenre.Id);
                }
                if (_sortIndex != 0)
                {
                    switch(_sortIndex)
                    {
                        case 1:
                            booksFiltered = booksFiltered.OrderByDescending(b => b.Rating);
                            break;
                        case 2:
                            booksFiltered = booksFiltered.OrderByDescending(b => b.PublicationDate);
                            break;
                    }
                }
                Books = await PaginatedList<Book>.CreateAsync(booksFiltered, 1, PAGE_SIZE);
                HasPrev = Books.HasPreviousPage;
                HasNext = Books.HasNextPage;
            });
        }
        public Command Previous
        {
            get => _previous ??= new(async () =>
            {
                Books = await PaginatedList<Book>.CreateAsync(booksFiltered, --_pageNumber, PAGE_SIZE);
                HasPrev = Books.HasPreviousPage;
                HasNext = Books.HasNextPage;
            });
        }
        public Command Next
        {
            get => _next ??= new(async () =>
            {
                Books = await PaginatedList<Book>.CreateAsync(booksFiltered, ++_pageNumber, PAGE_SIZE);
                HasPrev = Books.HasPreviousPage;
                HasNext = Books.HasNextPage;
            });
        }
        public Command Back
        {
            get => new(async () => await _navigationService.NavigateBack());
        }
        public Command NavigateToBook
        {
            get => new(async (o) =>
            {
                if (o == null) return;
                Book b = (Book)o;
                await _navigationService.NavigateToBookPage(b.Id);
            });
        }
        public CatalogPageViewModel(ApiCallService callService, NavigationService navigationService)
        {
            _callService = callService;
            _navigationService = navigationService;
        }
    }
}
