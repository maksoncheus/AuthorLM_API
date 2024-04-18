using AuthorLM_API.Data;
using DbLibrary.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthorLM_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class LikesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        public LikesController(ApplicationContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SetLike(int bookId)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == HttpContext.User.Identity.Name);
            Book? book = await _context.Books.FindAsync(bookId);
            if (user == null || book == null)
            {
                return NotFound();
            }
            if (await _context.Likes.FirstOrDefaultAsync(l => l.Liker == user && l.Book == book) != null)
                return BadRequest();
            await _context.Likes.AddAsync(new Like { Liker = user, Book = book});
            book.Rating += 1;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> UnsetLike(int bookId)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == HttpContext.User.Identity.Name);
            Book? book = await _context.Books.FindAsync(bookId);
            if (user == null || book == null)
            {
                return NotFound();
            }
            Like? like = await _context.Likes.FirstOrDefaultAsync(l => l.Liker == user && l.Book == book);
            if (like == null)
                return BadRequest();
            _context.Likes.Remove(like);
            book.Rating -= 1;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> GetLikesByBookId(int id)
        {
            Book? b = await _context.Books.FindAsync(id); 
            if(b == null)
                return NotFound();
            return Ok(_context.Likes.Where(l => l.Book.Id == id).ToList());
        }
    }
}
