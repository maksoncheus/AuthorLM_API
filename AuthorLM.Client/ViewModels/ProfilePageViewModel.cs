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
    public class ProfilePageViewModel : ViewModel
    {
        private readonly AccountService _accountService;
        private readonly ApiCallService _callService;
        private readonly NavigationService _navigationService;
        private bool _isRefreshing;
        private bool _isMyAccount;
        private User _user;
        private ObservableCollection<Book> _books;
        public bool IsMyAccount
        {
            get => _isMyAccount;
            set
            {
                _isMyAccount = value;
                OnPropertyChanged();
            }
        }
        public override Task OnNavigatedTo()
        {
            Refresh.Execute(null);
            return base.OnNavigatedTo();
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
                string path = _user.PathToPhoto;
                _user.PathToPhoto = "";
                OnPropertyChanged();
                _user.PathToPhoto = path;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Book> Books
        {
            get => _books;
            set
            {
                _books = value;
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
                await _init(_user.Id);
                IsRefreshing = false;
            }
            );
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
        public Command Edit
        {
            get => new(async() => await _navigationService.NavigateToEditProfilePage());
        }
        
        public Command Publish
        {
            get => new(async() => await _navigationService.NavigateToPublishPage());
        }

        public Command Back
        {
            get => new(async () => await _navigationService.NavigateBack());
        }
        public override Task OnNavigatingTo(object? parameter)
        {
            Task.Run(() => _init((int)parameter));
            return base.OnNavigatingTo(parameter);
        }
        public override Task OnNavigatedTo()
        {
            if(_user?.Id != null)
                Refresh.Execute(null);
            return base.OnNavigatedTo();
        }
        private async Task _init(int param)
        {
            User = await _callService.GetUserById(param);
            IEnumerable<Book> books = await _callService.GetAllBooks();
            Books = new(books.Where(b => b.Author.Id == User.Id));
            if(_accountService.IsLoggedIn)
            {
                if((await _callService.GetDetails()).Id == User.Id)
                {
                    IsMyAccount = true;
                    return;
                }
            }
            IsMyAccount = false;
        }
        public ProfilePageViewModel(AccountService accountService, ApiCallService callService, NavigationService navigationService)
        {
            _accountService = accountService;
            _callService = callService;
            _navigationService = navigationService;
        }
    }
}
