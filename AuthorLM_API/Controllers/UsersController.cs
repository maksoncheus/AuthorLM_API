using AuthorLM_API.Data;
using AuthorLM_API.Data.Encryption;
using AuthorLM_API.Data.Entities;
using AuthorLM_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace AuthorLM_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class UsersController : ControllerBase
    {

        private readonly ILogger<UsersController> _logger;
        private ApplicationContext _context;
        public UsersController(ILogger<UsersController> logger, ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            try
            {
                List<User> users = _context.Users.ToList();
                foreach (User user in users)
                {
                    user.Password = PasswordCipher.Decrypt(user.Password);
                }
                return Ok(users);
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpGet(Name = "GetUserDetails")]
        public async Task<IActionResult> GetUserDetails(int id)
        {
            try
            {
                User? user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound();
                return Ok(user);
            }
            catch
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
        [HttpPost(Name = "AddNewUser")]
        public async Task<IActionResult> AddUser(string username, string email, string password)
        {
            UserViewModel userVM = new()
            {
                Username = username,
                EmailAddress = email,
                Password = password
            };
            List<ValidationResult> validationErrors = new();
            if (Validator.TryValidateObject(userVM, new ValidationContext(userVM), validationErrors, true))
                try
                {
                    User user = new()
                    {
                        Username = userVM.Username,
                        EmailAddress = userVM.EmailAddress,
                        Password = PasswordCipher.Encrypt(userVM.Password)
                    };
                    await _context.Users.AddAsync(user);
                    await _context.SaveChangesAsync();
                    return Ok("User added succesfully");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            else
            {
                return BadRequest(validationErrors);
            }
        }

    }
}
