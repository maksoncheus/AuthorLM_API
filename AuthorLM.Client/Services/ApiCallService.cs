using DbLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.Services
{
    public class ApiCallService
    {
        private readonly HttpClient _client;
        private const string ApiAddress = "http://192.168.1.101:5020/";
        private const string contentWebRoot = "wwwroot";
        public ApiCallService(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri(ApiAddress);
        }
        public async Task<IEnumerable<Book>> GetAllBooks()
        {
            using HttpResponseMessage response = await _client.GetAsync("Book/GetAllBooks");
            List<Book> books = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Book>>(await  response.Content.ReadAsStringAsync()).ToList();
            if(books != null && books.Count > 0)
                books.ForEach(ChangePathInBook);
            return books;
        }
        public async Task<IEnumerable<Comment>> GetCommentsByBookId(int id)
        {
            string url = ApiAddress + $"Comment/GetCommentsByBookId?bookId={id}";
            using HttpResponseMessage response = await _client.GetAsync(url);

            List<Comment> comments = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Comment>>(await response.Content.ReadAsStringAsync()).ToList();
            return comments;
        }
        public async Task<IEnumerable<Like>> GetLikesByBookId(int id)
        {
            string url = ApiAddress + $"Likes/GetLikesByBookId?id={id}";
            using HttpResponseMessage response = await _client.GetAsync(url);

            List<Like> likes = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Like>>(await response.Content.ReadAsStringAsync()).ToList();
            return likes;
        }
        public async Task SetLike(int userId, int bookId)
        {
            string url = ApiAddress + $"Likes/SetLike?userId={userId}&bookId={bookId}";
            using HttpResponseMessage response = await _client.PostAsync(url, null);
            return;

        }
        public async Task UnsetLike(int userId, int bookId)
        {
            string url = ApiAddress + $"Likes/UnsetLike?userId={userId}&bookId={bookId}";
            using HttpResponseMessage response = await _client.DeleteAsync(url);
            return;
        }
        public async Task<User> GetUserById(int userId)
        {
            string url = ApiAddress + $"Users/GetUserDetails?id={userId}";
            using HttpResponseMessage response = await _client.GetAsync(url);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
        }
        private void ChangePathInBook(Book book)
        {
            if (book == null) throw new ArgumentException(nameof(book));
            string pathToEdit = book.CoverImagePath;
            pathToEdit = pathToEdit.Remove(0, pathToEdit.IndexOf(contentWebRoot));
            pathToEdit = pathToEdit.Replace(contentWebRoot + "\\", "");
            pathToEdit = pathToEdit.Replace("\\", "/");
            pathToEdit = Path.Combine(ApiAddress, pathToEdit);
            book.CoverImagePath = pathToEdit;
        }
    }
}
