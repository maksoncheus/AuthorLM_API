using AuthorLM_API.Data.Encryption;
using DbLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthorLM_API.Data
{
    public class DBInitializer
    {
        private static readonly List<string> _roles = new List<string>()
            {
                "User",
                "Admin",
                "Guest"
            };
        private static readonly string _adminUsername = "SuperMegaAdmin";
        private static readonly string _adminEmail = "lyubaAdmin@mail.ru";
        private static readonly string _adminPassword = "adminSecurePassword";

        public static async Task InitializeAsync(ApplicationContext context, IWebHostEnvironment environment)
        {
            await AddRolesAsync(context);
            await AddAdminUserAsync(context);
            await DeleteFoldersWithNoBooks(context, environment);
        }
        private static async Task AddRolesAsync(ApplicationContext context)
        {
            foreach (string role in _roles)
            {
                if (!context.Roles.Any(r => r.Name == role))
                {
                    await context.Roles.AddAsync(new Role() { Name = role });
                }
            }
            await context.SaveChangesAsync();
        }
        private static async Task AddAdminUserAsync(ApplicationContext context)
        {
            if (!context.Users.Any(x => x.Username == _adminUsername))
            {
                Role role = context.Roles.First(r => r.Name == "Admin");
                string adminpassword = PasswordCipher.Encrypt(_adminPassword);
                context.Users.Add(new User()
                {
                    Username = _adminUsername,
                    Password = adminpassword,
                    Role = role,
                    EmailAddress = _adminEmail
                });
            }
            await context.SaveChangesAsync();
        }
        private static async Task DeleteFoldersWithNoBooks(ApplicationContext context, IWebHostEnvironment _environment)
        {
            List<string> directories = Directory.EnumerateDirectories(Path.Combine(_environment.WebRootPath, "books")).ToList();
            foreach (string directory in directories)
            {
                if (await context.Books.FirstOrDefaultAsync(b => b.Title == directory.Replace(Path.Combine(_environment.WebRootPath, "books") + "\\", "")) == null)
                {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}
