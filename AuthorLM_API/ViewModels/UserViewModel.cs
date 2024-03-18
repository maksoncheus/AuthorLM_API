using System.ComponentModel.DataAnnotations;

namespace AuthorLM_API.ViewModels
{
    public class UserViewModel
    {
        [MinLength(6, ErrorMessage = "The minimum length of the username is 6")]
        [MaxLength(30, ErrorMessage = "The maximum length of the username is 30")]
        public string Username { get; set; } = null!;
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string EmailAddress { get; set; } = null!;
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "The minimum length of the password is 8")]
        [MaxLength(20, ErrorMessage = "The maximum length of the password is 20")]
        public string Password { get; set; } = null!;
    }
}
