using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Te rog introdu username-ul sau email-ul.")]
        public string UserNameOrEmail { get; set; }
    }

}
