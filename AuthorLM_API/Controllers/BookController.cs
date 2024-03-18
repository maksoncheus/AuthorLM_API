using AuthorLM_API.Data;
using AuthorLM_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthorLM_API.Interfaces;
using AuthorLM_API.Data.Entities;
using AuthorLM_API.Data.Encription;
using Microsoft.EntityFrameworkCore;
namespace AuthorLM_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class BookController : ControllerBase
    {
        private readonly ILogger<BookController> _logger;
        private readonly ApplicationContext _context;
        private readonly IPublishBookService _publishBookService;
        public BookController(ILogger<BookController> logger, ApplicationContext context, IPublishBookService publishBookService)
        {
            _logger = logger;
            _context = context;

            _publishBookService = publishBookService;
        }
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
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PublishBook(PublishBookViewModel publishViewModel)
        {
            if (publishViewModel == null)
                return BadRequest();
            await _publishBookService.SaveBookCoverImageAsync(publishViewModel);
            await _publishBookService.SaveBookContentAsync(publishViewModel);
            string? username = HttpContext.User?.Identity?.Name;
            if (username != null)
            {
                try
                {
                    User? user = _context.Users.FirstOrDefault(u => u.Username == username);
                    Genre? genre = _context.Genres.FirstOrDefault(g => g.Id == publishViewModel.GenreId);

                    if (user != null && genre != null)
                    {
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
    }
}
