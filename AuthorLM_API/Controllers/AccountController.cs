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
using Microsoft.AspNetCore.Http;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace AuthorLM_API.Controllers
{
    /// <summary>
    /// Контроллер "Профиль". Содержит действия для работы с профилями пользователей.
    /// </summary>
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
        /// <summary>
        /// Регистрация нового пользователя.
        /// </summary>
        /// <param name="username">Уникальное имя пользователя.</param>
        /// <param name="email">Уникальный адрес электронной почты.</param>
        /// <param name="password">Пароль.</param>
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
                        PathToPhoto = Path.Combine(_webHostEnvironment.WebRootPath, "src", "images") + "\\reader.png",
                        Status = ""
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
        /// Авторизация пользователя
        /// </summary>
        /// <param name="authString">Имя пользователя или адрес электронной почты</param>
        /// <param name="password">Пароль</param>
        /// <returns>JWT для авторизованных запросов к API</returns>
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
            bool isAdmin = _context.Users.FirstOrDefault(
                x => (x.Username == authString || x.EmailAddress == authString)).Role.Name == "Admin";
            return Ok(new { token = encodedJwt, isAdmin });
        }
        /// <summary>
        /// Метод получения информации о пользователе на основе клаймов.
        /// </summary>
        /// <param name="authString">Имя пользователя или адрес электронной почты</param>
        /// <param name="password">Пароль</param>
        /// <returns>Объект, содержащий информацию о пользователе (null, если пользователь не найден).</returns>
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
        /// <summary>
        /// Получить полную информацию о своем профиле. Требуется авторизация.
        /// </summary>
        /// <returns></returns>
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
                        return Ok(new { user.Id, user.Username, user.EmailAddress, user.Status, user.PathToPhoto, user.Role });
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// Сменить пароль.
        /// </summary>
        /// <param name="oldPassword">Старый пароль для подтверждения.</param>
        /// <param name="newPassword">Новый пароль.</param>
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
                    if (PasswordCipher.Decrypt(user.Password) != oldPassword) return BadRequest("Вы ввели неправильный пароль");
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
        /// <summary>
        /// Изменение основной информации в профиле.
        /// </summary>
        /// <param name="username">Новое имя пользователя.</param>
        /// <param name="email">Новый адрес электронной почты.</param>
        /// <param name="status">Новый статус.</param>
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> ChangeDetails(string username, string email, string status)
        {
            User? user = _context.Users.FirstOrDefault(u => u.Username == HttpContext.User.Identity.Name);
            if (user.Username != username)
            {
                if (await _context.Users.FirstOrDefaultAsync(u => u.Username == username) != null)
                    return BadRequest("Пользователь с таким именем уже зарегистрирован");
                user.Username = username;
                var FileName = user.Username;

                var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "accounts", "images");

                var filePath = Path.Combine(uploads, FileName + Path.GetExtension(user.PathToPhoto));
                System.IO.File.Move(user.PathToPhoto, filePath);
                user.PathToPhoto = filePath;
            }
            if (user.EmailAddress != email)
            {
                if (await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == email) != null)
                    return BadRequest("Пользователь с таким адресом почты уже зарегистрирован");
                user.EmailAddress = email;
            }
            if (user.Status != status)
            {
                user.Status = status;
            }
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var identity = GetIdentity(user.Username, PasswordCipher.Decrypt(user.Password));

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            bool isAdmin = _context.Users.FirstOrDefault(
                x => (x.Username == user.Username || x.EmailAddress == user.EmailAddress)).Role.Name == "Admin";
            return Ok(new { token = encodedJwt, isAdmin });
        }
        /// <summary>
        /// Изменить фотографию профиля.
        /// </summary>
        /// <param name="vm">Модель представления с фотографией.</param>
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> ChangePhoto([FromForm] PhotoViewModel vm)
        {
            User user = _context.Users.First(u => u.Username == HttpContext.User.Identity.Name);
            var FileName = user.Username;

            var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "accounts", "images");

            var filePath = Path.Combine(uploads, FileName + Path.GetExtension(vm.UserPhoto.FileName));
            foreach (string f in Directory.EnumerateFiles(uploads))
            {
                if (Path.GetFileNameWithoutExtension(f) == FileName)
                    System.IO.File.Delete(f);
            }
            int height = 300;
            int width = 300;
            System.Drawing.Image image = System.Drawing.Image.FromStream(vm.UserPhoto.OpenReadStream(), true, true);
            var newImage = new System.Drawing.Bitmap(width, height);
            using (var a = System.Drawing.Graphics.FromImage(newImage))
            {
                a.DrawImage(image, 0, 0, width, height);
                newImage.Save(filePath);
            }
            user.PathToPhoto = filePath;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok("Успешно!");
        }
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> RemoveProfile(int id)
        {
            User? userToDelete = await _context.Users.FindAsync(id);
            if (userToDelete != null)
            {
                if(HttpContext.User.Identity.Name == userToDelete.Username || HttpContext.User.IsInRole("Admin"))
                {
                    foreach(var book in _context.Books.Where(b => b.Author.Id == id))
                    {
                        _context.Books.Remove(book);
                    }
                    foreach (var comment in _context.Comments.Where(c => c.Author.Id == id))
                    {
                        _context.Comments.Remove(comment);
                    }
                    foreach (var like in _context.Likes.Where(l => l.Liker.Id == id))
                    {
                        _context.Likes.Remove(like);
                    }
                    foreach (var reading in _context.UserReadingBooks.Where(l => l.User.Id == id))
                    {
                        _context.UserReadingBooks.Remove(reading);
                    }
                    foreach (var read in _context.UserReadBooks.Where(l => l.User.Id == id))
                    {
                        _context.UserReadBooks.Remove(read);
                    }
                    foreach (var fav in _context.UserFavoriteBooks.Where(l => l.User.Id == id))
                    {
                        _context.UserFavoriteBooks.Remove(fav);
                    }
                    foreach (var p in _context.Progresses.Where(l => l.User.Id == id))
                    {
                        _context.Progresses.Remove(p);
                    }
                    _context.Users.Remove(userToDelete);
                    await _context.SaveChangesAsync();
                    return Ok("Профиль удален");
                }
                return BadRequest("У вас недостаточно прав для удаления профиля");
            }
            return BadRequest("Пользователь не найден");
        }
    }
}
