using System.ComponentModel.DataAnnotations;

namespace Hello.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Is required please")]
        [EmailAddress(ErrorMessage = "Not valid email form")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }


    }
}

