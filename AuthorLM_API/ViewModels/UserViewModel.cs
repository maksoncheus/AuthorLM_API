using System.ComponentModel.DataAnnotations;

namespace AuthorLM_API.ViewModels
{
    public class UserViewModel
    {
        [MinLength(6, ErrorMessage = "Минимальная длина имени - 6 символов")]
        [MaxLength(30, ErrorMessage = "Максимальная длина имени - 30 символов")]
        public string Username { get; set; } = null!;
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string EmailAddress { get; set; } = null!;
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Минимальная длина пароля - 8 символов")]
        [MaxLength(20, ErrorMessage = "Максимальная длина пароля - 20 символов")]
        public string Password { get; set; } = null!;
    }
}
