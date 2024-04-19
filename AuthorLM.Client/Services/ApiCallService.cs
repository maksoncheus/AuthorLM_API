using DbLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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
            List<Book> books = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Book>>(await response.Content.ReadAsStringAsync()).ToList();
            if (books != null && books.Count > 0)
                books.ForEach((b) =>
                {
                    b.ContentPath = _fileResolveService.ResolvePath(b.ContentPath);
                    b.CoverImagePath = _fileResolveService.ResolvePath(b.CoverImagePath);
                });
            return books;
        }
        public async Task<IEnumerable<Genre>> GetGenres()
        {
            using HttpResponseMessage response = await _client.GetAsync("Genre/GetGenres");
            List<Genre> genres = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Genre>>(await response.Content.ReadAsStringAsync()).ToList();
            return genres;
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
            return await _client.PostAsync(url, null);
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
        public async Task<HttpResponseMessage> ChangePassword(string oldp, string newp)
        {
            string url = FileResolveService.ApiAddress + $"Account/ChangePassword?oldPassword={oldp}&newPassword={newp}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
            _headerService.AddAuthorizationHeader(request);
            return await _client.SendAsync(request);
        }
        public async Task<HttpResponseMessage> ChangeDetails(string username, string email, string status)
        {
            string url = FileResolveService.ApiAddress + $"Account/ChangeDetails?username={username}&email={email}&status={status}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
            _headerService.AddAuthorizationHeader(request);
            return await _client.SendAsync(request);
        }
        public async Task<HttpResponseMessage> ChangePhoto(FileResult content)
        {
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue(content.ContentType);
            using MultipartFormDataContent form = new MultipartFormDataContent();
            var fileStreamContent = new StreamContent(await content.OpenReadAsync());
            form.Add(fileStreamContent, "userPhoto", content.FileName);
            string url = FileResolveService.ApiAddress + $"Account/ChangePhoto";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Content = form;
            _headerService.AddAuthorizationHeader(request);
            return await _client.SendAsync(request);
        }
        public async Task<HttpResponseMessage> PublishBook(string title, string desc, int genre, FileResult? cover, FileResult content)
        {
            //MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue(content.ContentType);
            using MultipartFormDataContent form = new MultipartFormDataContent();
            var fileStreamContent = new StreamContent(await content.OpenReadAsync());
            form.Add(new StringContent(title), "title");
            form.Add(new StringContent(desc), "description");
            form.Add(new StringContent(genre.ToString()), "genreId");
            form.Add(fileStreamContent, "content", content.FileName);
            if (cover != null)
            {
                var fileStreamCover = new StreamContent(await cover.OpenReadAsync());
                form.Add(fileStreamCover, "coverImage", cover.FileName);
            }
            string url = FileResolveService.ApiAddress + $"Book/PublishBook";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = form;
            _headerService.AddAuthorizationHeader(request);
            return await _client.SendAsync(request);
        }
    }
}
