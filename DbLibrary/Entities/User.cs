using System.ComponentModel.DataAnnotations;

namespace DbLibrary.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [MinLength(6, ErrorMessage = "Минимальная длина логина - 6 символов")]
        [MaxLength(30, ErrorMessage = "Максимальная длина логина - 6 символов")]
        public string Username { get; set; } = null!;
        [DataType(DataType.EmailAddress, ErrorMessage = "Введенный адрес не является корректным")]
        [EmailAddress]
        public string EmailAddress { get; set; } = null!;
        public string Status { get; set;} = null!;
        public string PathToPhoto { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}
