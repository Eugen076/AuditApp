using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Parolă Nouă")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmare Parolă")]
        [Compare("NewPassword", ErrorMessage = "Parolele nu se potrivesc.")]
        public string ConfirmPassword { get; set; }
    }

}
