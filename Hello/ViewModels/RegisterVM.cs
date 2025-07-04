using System.ComponentModel.DataAnnotations;

namespace Hello.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage ="Is required please")]
        [EmailAddress(ErrorMessage = "Not valid email form")]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Not the same as password created")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Name { get; set; }
    }
}

