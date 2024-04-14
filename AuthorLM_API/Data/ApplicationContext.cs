using DbLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthorLM_API.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseLazyLoadingProxies();
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>(user =>
            {
                user.HasIndex(u => u.EmailAddress).IsUnique();
                user.HasIndex(u => u.Username).IsUnique();
            });
            builder.Entity<Book>(book =>
            {
                book.HasIndex(b => b.Title).IsUnique();
            });
        }
    }
}
