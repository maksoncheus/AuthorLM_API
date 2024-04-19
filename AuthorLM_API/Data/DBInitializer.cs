using AuthorLM_API.Data.Encription;
using AuthorLM_API.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System;

namespace AuthorLM_API.Data
{
    public class DBInitializer
    {
        public static async Task InitializeAsync(ApplicationContext context)
        {
            List<string> roles = new List<string>()
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
            await SetPhotoPaths(context, environment);
        }
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
            if (!context.Users.Any(x => x.Username == "SuperMegaAdmin"))
            {
                Role role = context.Roles.First(r => r.Name == "Admin");
                string adminpassword = PasswordCipher.Encrypt("adminSecurePassword");
                context.Users.Add(new User()
                {
                    Username = "SuperMegaAdmin",
                    Password = adminpassword,
                    Role = role,
                    EmailAddress = "lyubaAdmin@mail.ru"
                });
            }
            await context.SaveChangesAsync();
        }
    }
}
