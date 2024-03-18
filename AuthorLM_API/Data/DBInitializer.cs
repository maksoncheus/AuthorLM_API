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
            foreach (string role in roles)
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
