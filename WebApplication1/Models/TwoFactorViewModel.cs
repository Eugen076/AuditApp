using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class TwoFactorViewModel
    {
            public string UserId { get; set; }

            [Required(ErrorMessage = "Introduceți codul 2FA")]
            [Display(Name = "Cod 2FA")]
            public string Code { get; set; }
   
    }

}
