using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class CustomerEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele complet este obligatoriu.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email-ul este obligatoriu.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefonul este obligatoriu.")]
        [RegularExpression(@"^07\d{8}$", ErrorMessage = "Telefonul trebuie să înceapă cu '07' și să aibă 10 cifre.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Adresa este obligatorie.")]
        public string Address { get; set; }

        public bool IsActive { get; set; }

     //   public DateTime DateOfBirth { get; set; }
    }

}
