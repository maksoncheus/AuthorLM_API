using AuthorLM_API.Data.Encryption;
using DbLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthorLM_API.Data
{
    /// <summary>
    /// Ининциализатор базы данных. Установка ролей, пользователя-администратора.
    /// </summary>
    public class DBInitializer
    {
        private static readonly List<string> _roles = new List<string>()
            {
                "User",
                "Admin"
            };
        private static readonly string _adminUsername = "Администратор";
        private static readonly string _adminEmail = "lyubaAdmin@mail.ru";
        private static readonly string _adminPassword = "adminPassword";
        /// <summary>
        /// Основной метод инициализации
        /// </summary>
        /// <param name="context">Контекст приложения</param>
        /// <param name="environment">Веб-хост</param>
        public static async Task InitializeAsync(ApplicationContext context, IWebHostEnvironment environment)
        {
            await AddRolesAsync(context);
            await AddAdminUserAsync(context);
            await DeleteFoldersWithNoBooks(context, environment);
            await SetPhotoPaths(context, environment);
        }
        /// <summary>
        /// Установка путей к "аватару" профиля у пользователей, у которых это поле пустое.
        /// </summary>
        /// <param name="context">Контекст приложения</param>
        /// <param name="environment">Веб-хост</param>
        private static async Task SetPhotoPaths(ApplicationContext context, IWebHostEnvironment environment)
        {
            foreach(User user in context.Users)
            {
                if (string.IsNullOrEmpty(user.PathToPhoto))
                    user.PathToPhoto = Path.Combine(environment.WebRootPath, "src", "images") + "\\reader.png";
                context.Entry(user).State = EntityState.Modified;
            }
            await context.SaveChangesAsync();
        }
        /// <summary>
        /// Добавить роли в БД, если они ещё не созданы.
        /// </summary>
        /// <param name="context">Контекст приложения</param>
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
        /// <summary>
        /// Добавить пользователя-администратора, если его ещё нет.
        /// </summary>
        /// <param name="context">Контекст приложения</param>
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
                    PathToPhoto = string.Empty,
                    Role = role,
                    Status = string.Empty,
                    EmailAddress = _adminEmail
                });
            }
            await context.SaveChangesAsync();
        }
        /// <summary>
        /// Удалить папки, в которых нет книг.
        /// </summary>
        /// <param name="context">Контекст приложения</param>
        /// <param name="environment">Веб-хост</param>
        private static async Task DeleteFoldersWithNoBooks(ApplicationContext context, IWebHostEnvironment environment)
        {
            List<string> directories = Directory.EnumerateDirectories(Path.Combine(environment.WebRootPath, "books")).ToList();
            foreach (string directory in directories)
            {
                if (await context.Books.FirstOrDefaultAsync(b => b.Title == directory.Replace(Path.Combine(environment.WebRootPath, "books") + "\\", "")) == null)
                {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}
