using AuthorLM_API.Data;
using DbLibrary.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthorLM_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GenreController : ControllerBase
    {
        private readonly ILogger<GenreController> _logger;
        private ApplicationContext _context;
        public GenreController(ILogger<GenreController> logger, ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpGet]
        public IActionResult GetGenres()
        {
            try
            {
                return Ok(_context.Genres);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddGenre(string name)
        {
            try
            {
                await _context.Genres.AddAsync(new DbLibrary.Entities.Genre() { Name = name });
                await _context.SaveChangesAsync();
                return Ok("Жанр добавлен");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            try
            {
                Genre? genre = await _context.Genres.FindAsync(id);
                _context.Genres.Remove(genre);
                await _context.SaveChangesAsync();
                return Ok("Жанр удален");
            }
            catch (Exception ex)
            {
                return BadRequest("Не удалось удалить жанр");
            }
        }
    }
}
