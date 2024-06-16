using AuthorLM_API.Data;
using AuthorLM_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthorLM_API.Interfaces;
using DbLibrary.Entities;
using AuthorLM_API.Data.Encryption;
using Microsoft.EntityFrameworkCore;
using System.IO;
using DbLibrary.Helpers;
using static System.Collections.Specialized.BitVector32;
namespace AuthorLM_API.Controllers
{
    /// <summary>
    /// Контроллер "Книги". Содержит действия для работы с контентом книг, их добавлением/удалением/редактированием.
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class BookController : ControllerBase
    {
        /// <summary>
        /// Доступные расширения для контента книг.
        /// </summary>
        /// ToDo: расширить поддержку различных форматов
        private readonly List<string> _bookContentAllowedExtensions = new()
        {
            ".fb2",
            ".txt"
        };
        /// <summary>
        /// Доступные расширения для обложек книг.
        /// </summary>
        private readonly List<string> _bookCoverAllowedExtensions = new()
        {
            ".png",
            ".jpeg"
        };
        private readonly List<string> _possibleLibraryEntries = new()
        {
            "read",
            "reading",
            "favorite"
        };
        private readonly ILogger<BookController> _logger;
        private readonly ApplicationContext _context;
        private readonly IPublishBookService _publishBookService;
        public BookController(ILogger<BookController> logger, ApplicationContext context, IPublishBookService publishBookService)
        {
            _logger = logger;
            _context = context;

            _publishBookService = publishBookService;
        }
        /// <summary>
        /// Получить список всех книг.
        /// </summary>
        /// <returns>Список книг</returns>
        [HttpGet]
        public IActionResult GetAllBooks()
        {
            try
            {
                return Ok(_context.Books);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetBooksWithFilter(
            string? searchString,
            int? genreId,
            bool? needToReSearch,
            int? sortIndex,
            int? pageNumber)
        {
            if (needToReSearch == null || needToReSearch.Value == true)
            {
                pageNumber = 1;
            }
            var books = await _context.Books.ToListAsync();
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(
                    b => b.Title.Contains(searchString)
                    ||
                    b.Description.Contains(searchString)
                    ||
                    b.Author.Username.Contains(searchString)
                    ||
                    b.Author.EmailAddress.Contains(searchString))
                    .ToList();
            }
            if(genreId != null)
                books = books.Where(b => b.Genre.Id == genreId.Value).ToList();
            if(sortIndex != null)
                switch(sortIndex.Value)
                {
                    case 2:
                        books = books.OrderBy(b => b.Rating).ToList();
                        break;
                    case 1:
                        books = books.OrderBy(b => b.PublicationDate).ToList();
                        break;
                }
            int pageSize = 2;
            return Ok(await PaginatedList<Book>.CreateAsync(books, pageNumber ?? 1, pageSize));
        }
        /// <summary>
        /// Опубликовать книгу. Требуется авторизация.
        /// </summary>
        /// <param name="publishViewModel">Модель представления публикации книги.</param>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PublishBook([FromForm] PublishBookViewModel publishViewModel)
        {
            if (publishViewModel == null)
                return BadRequest("Form is null");
            if (_context.Books.Any(b => b.Title == publishViewModel.Title))
                return BadRequest("This book is already exists");
            if (!_bookContentAllowedExtensions.Any(x => x == Path.GetExtension(publishViewModel.Content.FileName)))
                return BadRequest($"Only {string.Join(", ", _bookContentAllowedExtensions)} extensions allowed for book content");
            if (publishViewModel.CoverImage != null)
                if (!_bookCoverAllowedExtensions.Any(x => x == Path.GetExtension(publishViewModel.CoverImage.FileName)))
                    return BadRequest($"Only {string.Join(", ", _bookCoverAllowedExtensions)} extensions allowed for cover image");
            string? username = HttpContext.User?.Identity?.Name;
            if (username != null)
            {
                try
                {
                    User? user = _context.Users.FirstOrDefault(u => u.Username == username);
                    Genre? genre = _context.Genres.FirstOrDefault(g => g.Id == publishViewModel.GenreId);

                    if (user != null && genre != null)
                    {
                        //await _publishBookService.SaveBookCoverImageAsync(publishViewModel);
                        //await _publishBookService.SaveBookContentAsync(publishViewModel);
                        Book book = await _publishBookService.PublishBookAsync(publishViewModel, user, genre);
                        await _context.AddAsync(book);
                        await _context.SaveChangesAsync();
                        return Ok(book);
                    }
                    return BadRequest();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest();
        }
        /// <summary>
        /// Скачать книгу. Требуется авторизация
        /// </summary>
        /// <param name="id">ID книги</param>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBookContent(int id)
        {
            Book? book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book != null)
            {
                byte[] content = await System.IO.File.ReadAllBytesAsync(book.ContentPath);
                string mimeType = MimeKit.MimeTypes.GetMimeType(book.ContentPath);

                System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = string.Join("/", book.ContentPath.Split("/").Select(s => System.Net.WebUtility.UrlEncode(s))),
                    Inline = false
                };
                var stream = new FileStream(book.ContentPath, FileMode.Open, FileAccess.Read);
                Response.Headers.Append("Content-Disposition", cd.ToString());
                return File(stream, mimeType, Path.GetFileName(book.ContentPath));
            }
            return NotFound();
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SetProgress(int bookId, int section, double scroll)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == HttpContext.User.Identity.Name);
            Book? book = await _context.Books.FindAsync(bookId);
            if(user != null && book != null)
            {
                bool isExist = false;
                Progress? progress = await _context.Progresses.FirstOrDefaultAsync(p => p.User.Id == user.Id && p.Book.Id == book.Id);
                if (progress == null) progress = new() { User = user, Book = book };
                else isExist = true;
                progress.Section = section;
                progress.Scroll = scroll;
                if (isExist)
                    _context.Entry(progress).State = EntityState.Modified;
                else 
                    _context.Progresses.Add(progress);
                await _context.SaveChangesAsync();
                return Ok("Прогресс успешно сохранен");
            }
            return BadRequest("Не удалось установить прогресс");
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProgress(int bookId)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == HttpContext.User.Identity.Name);
            Book? book = await _context.Books.FindAsync(bookId);
            if (user != null && book != null)
            {
                Progress? progress = await _context.Progresses.FirstOrDefaultAsync(p => p.User.Id == user.Id && p.Book.Id == book.Id);
                if (progress == null)
                {
                    return Ok(new Progress() { User = user, Book = book, Section = -1, Scroll = 0 });
                }
                else return Ok(progress);
            }
            return BadRequest("Не удалось восстановить прогресс");
        }
        /// <summary>
        /// Удалить книгу. Требуется авторизация под профилем автора или администратора.
        /// </summary>
        /// <param name="id">ID книги</param>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteBook(int id)
        {
            Book? book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book != null)
            {
                string? username = HttpContext.User?.Identity?.Name;
                if (username != null)
                {
                    if (username == book.Author.Username || HttpContext.User.IsInRole("Admin"))
                    {
                        try
                        {
                            Directory.Delete(Path.GetDirectoryName(book.ContentPath), true);
                            _context.Books.Remove(book);
                            await _context.SaveChangesAsync();
                            return Ok("Book removed succesfully");
                        }
                        catch (Exception ex)
                        {
                            return BadRequest(ex.Message);
                        }
                    }
                }
            }
            return NotFound("Book with this ID is not found");
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetLibraryEntry(int bookId)
        {
            Book? book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
            if (book != null)
            {
                string? username = HttpContext.User?.Identity?.Name;
                if (username != null)
                {
                    User? user = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (_context.UserReadBooks.Any(urb => urb.User.Username == username && urb.Book.Id == bookId))
                        return Ok("read");
                    if (_context.UserReadingBooks.Any(urb => urb.User.Username == username && urb.Book.Id == bookId))
                        return Ok("reading");
                    if (_context.UserFavoriteBooks.Any(ufb => ufb.User.Username == username && ufb.Book.Id == bookId))
                        return Ok("favorite");
                    return Ok("none");
                }
            }
            return NotFound("Book with this ID is not found");
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetReadBooks()
        {
            string? username = HttpContext.User?.Identity?.Name;
            if (username != null)
            {
                User? user = _context.Users.FirstOrDefault(u => u.Username == username);
                return Ok(await _context.UserReadBooks.Where(urb => urb.User.Id == user.Id).ToListAsync());
            }
            return NotFound();
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetReadingBooks()
        {
            string? username = HttpContext.User?.Identity?.Name;
            if (username != null)
            {
                User? user = _context.Users.FirstOrDefault(u => u.Username == username);
                return Ok(await _context.UserReadingBooks.Where(urb => urb.User.Id == user.Id).ToListAsync());
            }
            return NotFound();
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetFavoriteBooks()
        {
            string? username = HttpContext.User?.Identity?.Name;
            if (username != null)
            {
                User? user = _context.Users.FirstOrDefault(u => u.Username == username);
                return Ok(await _context.UserFavoriteBooks.Where(ufb => ufb.User.Id == user.Id).ToListAsync());

            }
            return NotFound();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddBookToLibrary(int bookId, string entry)
        {
            if (!_possibleLibraryEntries.Contains(entry)) return BadRequest();
            Book? book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);

            string? username = HttpContext.User?.Identity?.Name;
            if (username != null && book != null)
            {
                User? user = _context.Users.FirstOrDefault(u => u.Username == username);
                RemoveBookFromUserLibrary(user, book);
                switch (entry)
                {
                    case "read":
                        _context.UserReadBooks.Add(new UserReadBooks() { UserId = user.Id, BookId = book.Id, User = user, Book = book });
                        break;
                    case "reading":
                        _context.UserReadingBooks.Add(new UserReadingBooks() { UserId = user.Id, BookId = book.Id, User = user, Book = book });
                        break;
                    case "favorite":
                        _context.UserFavoriteBooks.Add(new UserFavoriteBooks() { UserId = user.Id, BookId = book.Id, User = user, Book = book });
                        break;
                }
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveBookFromLibrary(int bookId)
        {
            Book? book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);

            string? username = HttpContext.User?.Identity?.Name;
            if (username != null && book != null)
            {
                User? user = _context.Users.FirstOrDefault(u => u.Username == username);
                RemoveBookFromUserLibrary(user, book);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }
        private void RemoveBookFromUserLibrary(User user, Book book)
        {
            if (_context.UserReadBooks.Any(urb => urb.User.Username == user.Username && urb.Book.Id == book.Id))
                _context.UserReadBooks.Remove(_context.UserReadBooks.First(urb => urb.User.Username == user.Username && urb.Book.Id == book.Id));
            if (_context.UserReadingBooks.Any(urb => urb.User.Username == user.Username && urb.Book.Id == book.Id))
                _context.UserReadingBooks.Remove(_context.UserReadingBooks.First(urb => urb.User.Username == user.Username && urb.Book.Id == book.Id));
            if (_context.UserFavoriteBooks.Any(ufb => ufb.User.Username == user.Username && ufb.Book.Id == book.Id))
                _context.UserFavoriteBooks.Remove(_context.UserFavoriteBooks.First(ufb => ufb.User.Username == user.Username && ufb.Book.Id == book.Id));
            _context.SaveChanges();
        }
    }
}
