using DbLibrary.Entities;
using DbLibrary.Helpers;
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
        public async Task<Stream> GetBookContent(int id)
        {
            string url = FileResolveService.ApiAddress + $"Book/GetBookContent?id={id}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            _headerService.AddAuthorizationHeader(request);
            var response = await _client.SendAsync(request);
            return response.Content.ReadAsStream();
        }
        public async Task<string> SetProgress(int bookId, int section, double scroll)
        {
            string url = FileResolveService.ApiAddress + $"Book/SetProgress?bookId={bookId}&section={section}&scroll={scroll}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            _headerService.AddAuthorizationHeader(request);
            var response = await _client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<Progress> GetProgress(int bookId)
        {
            string url = FileResolveService.ApiAddress + $"Book/GetProgress?bookId={bookId}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            _headerService.AddAuthorizationHeader(request);
            var response = await _client.SendAsync(request);
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Progress>(await response.Content.ReadAsStringAsync());
            }
            catch
            {
                return new();
            }
        }
        public async Task<PaginatedList<Book>> GetBooksWithFilter(string? searchString = null,
            int? genreId = null,
            bool? needToReSearch = null,
            int? sortIndex = null,
            int? pageNumber = null)
        {
            string url = FileResolveService.ApiAddress + $"Book/GetBooksWithFilter";
            
            if (genreId != null || needToReSearch != null || searchString != null || sortIndex != null || pageNumber != null)
                url += "?";
            if (genreId != null)
                url += $"genreId={genreId.Value}";
            if (needToReSearch != null)
            {
                if (!url.EndsWith("?"))
                    url += "&";
                url += $"needToReSearch={needToReSearch.Value}";
            }
            if (searchString != null)
            {
                if (!url.EndsWith("?"))
                    url += "&";
                url += $"searchString={searchString}";
            }
            if (sortIndex != null)
            {
                if (!url.EndsWith("?"))
                    url += "&";
                url += $"sortIndex={sortIndex.Value}";
            }
            if (pageNumber != null)
            {
                if (!url.EndsWith("?"))
                    url += "&";
                url += $"pageNumber={pageNumber.Value}";
            }
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            _headerService.AddAuthorizationHeader(request);
            HttpResponseMessage response = await _client.SendAsync(request);
            PaginatedList<Book> books = Newtonsoft.Json.JsonConvert.DeserializeObject<PaginatedList<Book>>(await response.Content.ReadAsStringAsync());
            if (books != null && books.Count > 0)
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
        public async Task<HttpResponseMessage> RemoveComment(int commentId)
        {
            string url = FileResolveService.ApiAddress + $"Comment/RemoveComment?commentId={commentId}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
            _headerService.AddAuthorizationHeader(request);
            return await _client.SendAsync(request);
        }
        public async Task<HttpResponseMessage> RemoveProfile(int id)
        {
            string url = FileResolveService.ApiAddress + $"Account/RemoveProfile?id={id}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
            _headerService.AddAuthorizationHeader(request);
            return await _client.SendAsync(request);
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
        public async Task<HttpResponseMessage> ChangeDetails(string username, string email, string status)
        {
            string url = FileResolveService.ApiAddress + $"Account/ChangeDetails?username={username}&email={email}&status={status}";
            HttpRequestMessage request = new(HttpMethod.Put, url);
            _headerService.AddAuthorizationHeader(request);
            return await _client.SendAsync(request);
        }
        public async Task<IEnumerable<Genre>> GetGenres()
        {
            string url = FileResolveService.ApiAddress + "Genre/GetGenres";
            using HttpResponseMessage response = await _client.GetAsync(url);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Genre>>(await response.Content.ReadAsStringAsync());
        }
        public async Task<HttpResponseMessage> ChangePassword(string oldp, string newp)
        {
            string url = FileResolveService.ApiAddress + $"Account/ChangePassword?oldPassword={oldp}&newPassword={newp}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            _headerService.AddAuthorizationHeader(request);
            return await _client.SendAsync(request);
        }
        public async Task PostComment(int bookId, string comment)
        {
            string url = FileResolveService.ApiAddress + $"Comment/PostComment?bookId={bookId}&commentText={comment}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            _headerService.AddAuthorizationHeader(request);
            using HttpResponseMessage response = await _client.SendAsync(request);
        }
        public async Task<HttpResponseMessage> ChangePhoto(FileResult photo)
        {
            string url = FileResolveService.ApiAddress + $"Account/ChangePhoto";
            var form = new MultipartFormDataContent();
            form.Add(new StreamContent(await photo.OpenReadAsync()), "userPhoto", photo.FileName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
            _headerService.AddAuthorizationHeader(request);
            request.Content = form;
            _headerService.AddAuthorizationHeader(request);
            return await _client.SendAsync(request);
        }

        internal async Task<HttpResponseMessage> PublishBook(string title, string description, int id, FileResult? cover, FileResult content)
        {
            string url = FileResolveService.ApiAddress + $"Book/PublishBook";
            var form = new MultipartFormDataContent();
            form.Add(new StringContent(title), "title");
            form.Add(new StringContent(description), "description");
            form.Add(new StringContent(id.ToString()), "genreId");
            if (cover != null)
            {
                form.Add(new StreamContent(await cover.OpenReadAsync()), "coverImage", cover.FileName);
            }
            form.Add(new StreamContent(await content.OpenReadAsync()), "content", content.FileName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            _headerService.AddAuthorizationHeader(request);
            request.Content = form;
            _headerService.AddAuthorizationHeader(request);
            return await _client.SendAsync(request);
        }
        public async Task<string> GetLibraryEntry(int bookId)
        {
            string url = FileResolveService.ApiAddress + $"Book/GetLibraryEntry?bookId={bookId}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            _headerService.AddAuthorizationHeader(request);
            HttpResponseMessage response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return "none";
            return await response.Content.ReadAsStringAsync();

        }
        public async Task<bool> AddBookToLibrary(int bookId, string entry)
        {
            string url = FileResolveService.ApiAddress + $"Book/AddBookToLibrary?bookId={bookId}&entry={entry}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            _headerService.AddAuthorizationHeader(request);
            HttpResponseMessage response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;
            return true;
        }
        public async Task<bool> RemoveBookFromLibrary(int bookId)
        {
            string url = FileResolveService.ApiAddress + $"Book/RemoveBookFromLibrary?bookId={bookId}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            _headerService.AddAuthorizationHeader(request);
            HttpResponseMessage response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;
            return true;
        }
        public async Task<IEnumerable<Book>> GetReadBooks()
        {
            
            string url = FileResolveService.ApiAddress + $"Book/GetReadBooks";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            _headerService.AddAuthorizationHeader(request);
            HttpResponseMessage response = await _client.SendAsync(request);
            List<Book> books = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<UserReadBooks>>(await response.Content.ReadAsStringAsync()).Select(x => x.Book).ToList();
            if (books != null && books.Count > 0)
                books.ForEach((b) =>
                {
                    b.ContentPath = _fileResolveService.ResolvePath(b.ContentPath);
                    b.CoverImagePath = _fileResolveService.ResolvePath(b.CoverImagePath);
                });
            return books;
        }
        public async Task<IEnumerable<Book>> GetReadingBooks()
        {
            string url = FileResolveService.ApiAddress + $"Book/GetReadingBooks";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            _headerService.AddAuthorizationHeader(request);
            HttpResponseMessage response = await _client.SendAsync(request);
            List<Book> books = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<UserReadBooks>>(await response.Content.ReadAsStringAsync()).Select(x => x.Book).ToList();
            if (books != null && books.Count > 0)
                books.ForEach((b) =>
                {
                    b.ContentPath = _fileResolveService.ResolvePath(b.ContentPath);
                    b.CoverImagePath = _fileResolveService.ResolvePath(b.CoverImagePath);
                });
            return books;
        }
        public async Task<IEnumerable<Book>> GetFavoriteBooks()
        {
            string url = FileResolveService.ApiAddress + $"Book/GetFavoriteBooks";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            _headerService.AddAuthorizationHeader(request);
            HttpResponseMessage response = await _client.SendAsync(request);
            List<Book> books = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<UserReadBooks>>(await response.Content.ReadAsStringAsync()).Select(x => x.Book).ToList();
            if (books != null && books.Count > 0)
                books.ForEach((b) =>
                {
                    b.ContentPath = _fileResolveService.ResolvePath(b.ContentPath);
                    b.CoverImagePath = _fileResolveService.ResolvePath(b.CoverImagePath);
                });
            return books;
        }
    }
}
