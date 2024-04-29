using AuthorLM_API.Data.Encryption;
using AuthorLM_API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using AuthorLM_API.Models.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DbLibrary.Entities;
using AuthorLM_API.ViewModels;
using AuthorLM_API.Helpers;

namespace AuthorLM_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private ApplicationContext _context;
        private IWebHostEnvironment _webHostEnvironment;
        public AccountController(ApplicationContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _webHostEnvironment = environment;
        }
        [HttpPost]
        public async Task<IActionResult> Registration(string username, string email, string password)
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
                        Password = PasswordCipher.Encrypt(userVM.Password),
                        Role = _context.Roles.First(r => r.Name == "User"),
                        PathToPhoto = Path.Combine(_webHostEnvironment.WebRootPath, "src", "images") + "\\reader.png"
                };
                    await _context.Users.AddAsync(user);
                    await _context.SaveChangesAsync();
                    return Ok("Registration succesful!");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            else
            {
                List<string?> errors = new List<string?>();
                foreach (var error in validationErrors)
                {
                    errors.Add(error.ErrorMessage);
                }
                return BadRequest(errors);
            }
        }
        /// <summary>
        /// Authentification method
        /// </summary>
        /// <param name="authString">Username or E-Mail : both accepted</param>
        /// <param name="password">Password</param>
        /// <returns>JWT Token for calls to API</returns>
        [HttpGet]
        public IActionResult Authenticate(string authString, string password)
        {
            var identity = GetIdentity(authString, password);
            if (identity == null)
            {
                return BadRequest(new { errorText = "Invalid username or password." });
            }

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return Ok(encodedJwt);
        }
        private ClaimsIdentity? GetIdentity(string authString, string password)
        {
            User? user = _context.Users.FirstOrDefault(
                x => (x.Username == authString || x.EmailAddress == authString));
            if (user != null)
            {
                string decryptedPassword = PasswordCipher.Decrypt(user.Password);
                if (decryptedPassword == password)
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.Name)
                };
                    ClaimsIdentity claimsIdentity =
                    new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                        ClaimsIdentity.DefaultRoleClaimType);
                    return claimsIdentity;
                }
            }

            // если пользователя не найдено
            return null;
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetDetails()
        {
            try
            {
                string? username = HttpContext.User?.Identity?.Name;
                if (username != null)
                {
                    User? user = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        return Ok(new { user.Id, user.Username, user.EmailAddress, user.Status, user.PathToPhoto });
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            
            string? username = HttpContext.User?.Identity?.Name;
            if (username != null && newPassword.Length >= 8 && newPassword.Length <= 20)
            {
                try
                {
                    string encryptedPassword = PasswordCipher.Encrypt(newPassword);
                    User? user = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (PasswordCipher.Decrypt(user.Password) != oldPassword) return BadRequest("");
                    if (user != null)
                    {
                        user.Password = encryptedPassword;
                        _context.Entry(user).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                        return Ok("Вы успешно изменили пароль!");
                    }
                    return BadRequest("Что-то пошло не так...");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest("Длина пароля - от 8 до 20 символов");
        }
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> ChangePhoto([FromForm] PhotoViewModel vm)
        {
            User user = _context.Users.First(u => u.Username == HttpContext.User.Identity.Name);
            var FileName = user.Username;

            var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "accounts","images");

            var filePath = Path.Combine(uploads, FileName + Path.GetExtension(vm.UserPhoto.FileName));
            foreach (string f in Directory.EnumerateFiles(uploads))
            {
                if(Path.GetFileNameWithoutExtension(f) == FileName)
                    System.IO.File.Delete(f);
            }
            using (FileStream stream = new(filePath, FileMode.Create))
            {
                await vm.UserPhoto.CopyToAsync(stream);
            }
            user.PathToPhoto = filePath;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok("Успешно!");
        }
    }
}
