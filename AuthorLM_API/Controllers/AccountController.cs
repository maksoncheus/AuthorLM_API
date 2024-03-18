using AuthorLM_API.Data.Encription;
using AuthorLM_API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using AuthorLM_API.Models.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using AuthorLM_API.Data.Entities;
using AuthorLM_API.ViewModels;

namespace AuthorLM_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private ApplicationContext _context;
        public AccountController(ApplicationContext context)
        {
            _context = context;
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
                        Role = _context.Roles.First(r => r.Name == "User")
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
                return BadRequest(validationErrors);
            }
        }
        /// <summary>
        /// Authentification method
        /// </summary>
        /// <param name="authString">Username or E-Mail : both accepted</param>
        /// <param name="password">Password</param>
        /// <returns>JWT Token for calls to API</returns>
        [HttpGet]
        public IActionResult Authentificate(string authString, string password)
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

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };
            return Ok(response);
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
                        string decryptedPassword = PasswordCipher.Decrypt(user.Password);
                        return Ok(new { user.Id, user.Username, user.EmailAddress, decryptedPassword });
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
        public async Task<IActionResult> ChangePassword(string newPassword)
        {
            string? username = HttpContext.User?.Identity?.Name;
            if (username != null && newPassword.Length >= 8 && newPassword.Length <= 20)
            {
                try
                {
                    string encryptedPassword = PasswordCipher.Encrypt(newPassword);
                    User? user = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        user.Password = encryptedPassword;
                        _context.Entry(user).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                        return Ok();
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
