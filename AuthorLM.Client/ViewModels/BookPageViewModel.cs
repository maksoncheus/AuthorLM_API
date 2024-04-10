using AuthorLM.Client.Services;
using DbLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.ViewModels
{
    public class BookPageViewModel : ViewModel
    {
        private readonly NavigationService _navigationService;
        private readonly ApiCallService _apiCallService;
        private int bookId;
        public override Task OnNavigatingTo(object? parameter)
        {
            bookId = (int)parameter;
            FindBookById(bookId);
            LoadComments(bookId);
            return base.OnNavigatingTo(parameter);
        }
        public Command Back
        {
            get => new(async () => await _navigationService.NavigateBack()); 
        }
        private async void FindBookById(int bookId)
        {
            List<Book> b = (List<Book>) await _apiCallService.GetAllBooks();
            Book = b.FirstOrDefault(_book => _book.Id == bookId);
        }
        private async void LoadComments(int bookId)
        {
            Comments = new ObservableCollection<Comment>(await _apiCallService.GetCommentsByBookId(bookId));
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
