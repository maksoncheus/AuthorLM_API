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
        private readonly HttpRequestHeaderService _headerService;
        private readonly FileResolveService _fileResolveService;
        public ApiCallService(HttpClient client, FileResolveService fileResolveService, HttpRequestHeaderService headerService)
        {
            _headerService = headerService;
            _fileResolveService = fileResolveService;
            _client = client;
            _client.BaseAddress = new Uri(FileResolveService.ApiAddress);
        }
        public async Task<IEnumerable<Book>> GetAllBooks()
        {
            using HttpResponseMessage response = await _client.GetAsync("Book/GetAllBooks");
            List<Book> books = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Book>>(await  response.Content.ReadAsStringAsync()).ToList();
            if(books != null && books.Count > 0)
                books.ForEach((b) =>
                {
                    b.ContentPath = _fileResolveService.ResolvePath(b.ContentPath);
                    b.CoverImagePath = _fileResolveService.ResolvePath(b.CoverImagePath);
                });
            return books;
        }
        public async Task<IEnumerable<Comment>> GetCommentsByBookId(int id)
        {
            string url = FileResolveService.ApiAddress + $"Comment/GetCommentsByBookId?bookId={id}";
            using HttpResponseMessage response = await _client.GetAsync(url);

            List<Comment> comments = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Comment>>(await response.Content.ReadAsStringAsync()).ToList();
            foreach (Comment comment in comments)
                comment.Author.PathToPhoto = _fileResolveService.ResolvePath(comment.Author.PathToPhoto);
            return comments;
        }
        public async Task<IEnumerable<Like>> GetLikesByBookId(int id)
        {
            string url = FileResolveService.ApiAddress + $"Likes/GetLikesByBookId?id={id}";
            using HttpResponseMessage response = await _client.GetAsync(url);

            List<Like> likes = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Like>>(await response.Content.ReadAsStringAsync()).ToList();
            return likes;
        }
        public async Task SetLike(int bookId)
        {
            string url = FileResolveService.ApiAddress + $"Likes/SetLike?bookId={bookId}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            _headerService.AddAuthorizationHeader(request);
            using HttpResponseMessage response = await _client.SendAsync(request);
            return;

        }
        public async Task UnsetLike(int bookId)
        {
            string url = FileResolveService.ApiAddress + $"Likes/UnsetLike?bookId={bookId}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
            _headerService.AddAuthorizationHeader(request);
            using HttpResponseMessage response = await _client.SendAsync(request);
            return;
        }
        public async Task<User?> GetUserById(int userId)
        {
            string url = FileResolveService.ApiAddress + $"Users/GetUserDetails?id={userId}";
            using HttpResponseMessage response = await _client.GetAsync(url);

            User? user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
            if (user == null) return null;
            user.PathToPhoto = _fileResolveService.ResolvePath(user.PathToPhoto);
            return user;
        }
        public async Task<HttpResponseMessage> Authenticate(string authString, string password)
        {
            string url = FileResolveService.ApiAddress + $"Account/Authenticate?authString={authString}&password={password}";
            return await _client.GetAsync(url);
        }
        public async Task<HttpResponseMessage> Register(string username, string email, string password)
        {
            string url = FileResolveService.ApiAddress + $"Account/Registration?username={username}&email={email}&password={password}";
            return await _client.PostAsync(url,null);
        }
        public async Task<User?> GetDetails()
        {
            string url = FileResolveService.ApiAddress + "Account/GetDetails";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            _headerService.AddAuthorizationHeader(request);
            using HttpResponseMessage response = await _client.SendAsync(request);
            User? user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
            if (user == null) return null;
            user.PathToPhoto = _fileResolveService.ResolvePath(user.PathToPhoto);
            return user;
        }
        public async Task PostComment(int bookId, string comment)
        {
            string url = FileResolveService.ApiAddress + $"Comment/PostComment?bookId={bookId}&commentText={comment}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            _headerService.AddAuthorizationHeader(request);
            using HttpResponseMessage response = await _client.SendAsync(request);
        }
    }
}
