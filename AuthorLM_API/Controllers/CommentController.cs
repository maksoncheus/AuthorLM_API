using AuthorLM_API.Data;
using DbLibrary.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AuthorLM_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CommentController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<CommentController> _logger;
        public CommentController(ApplicationContext context, ILogger<CommentController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetCommentsByBookId(int bookId)
        {
            try
            {
                Book? book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
                if (book != null)
                {
                    return Ok(_context.Comments.Where(c => c.Book == book));
                }
                return NotFound("Book not found");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostComment(int bookId, string commentText)
        {
            try
            {
                Book? book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
                if (book != null)
                {
                    string? username = HttpContext.User?.Identity?.Name;
                    if (username != null)
                    {
                        User? author = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                        if (author != null)
                        {
                            try
                            {
                                await _context.Comments.AddAsync(new Comment()
                                {
                                    Author = author,
                                    Book = book,
                                    Text = commentText,
                                    TimeStamp = DateTime.UtcNow
                                });
                                await _context.SaveChangesAsync();
                                return Ok("Comment posted succesfully");
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> RemoveComment(int commentId)
        {
            try
            {
                Comment? comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
                if (comment != null)
                {
                    string? username = HttpContext.User?.Identity?.Name;
                    if (username != null)
                    {
                        if (username == comment.Author.Username || HttpContext.User.IsInRole("Admin"))
                        {
                            try
                            {
                                _context.Comments.Remove(comment);
                                await _context.SaveChangesAsync();
                                return Ok("Comment removed succesfully");
                            }
                            catch (Exception ex)
                            {
                                return BadRequest(ex.Message);
                            }
                        }
                    }
                }
                return BadRequest("Comment is not found");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
