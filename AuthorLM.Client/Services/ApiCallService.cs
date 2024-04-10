using DbLibrary.Data.Entities;
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
