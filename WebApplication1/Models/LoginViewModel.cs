using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = " Username or Email is required")]
        [MaxLength(40, ErrorMessage = "Max 40 charcacter allowed.")]
        [DisplayName("Username or Email")]
        public string UserNameorEmail { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
