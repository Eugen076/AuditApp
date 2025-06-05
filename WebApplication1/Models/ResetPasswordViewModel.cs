using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Parola trebuie să aibă cel puțin 6 caractere.")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Parolele nu coincid.")]
        public string ConfirmPassword { get; set; }
    }
}
