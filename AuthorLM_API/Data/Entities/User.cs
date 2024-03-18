using System.ComponentModel.DataAnnotations;

namespace AuthorLM_API.Data.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [MinLength(6, ErrorMessage = "The minimum length of the username is 6")]
        [MaxLength(30, ErrorMessage = "The maximum length of the username is 30")]
        public string Username { get; set; } = null!;
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string EmailAddress { get; set; } = null!;
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}
