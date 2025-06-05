using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class CreateLoanViewModel
    {
        public int CustomerId { get; set; }
        public List<SelectListItem> Customers { get; set; } = new();

        [Required]
        [Range(1000, 1000000)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Selectează un tip de credit.")]
        public string LoanType { get; set; }


        [Required]
        [Range(1, 30)]
        public int LoanTermYears { get; set; } 

        public decimal InterestRate { get; set; }

        public decimal MonthlyPayment { get; set; }

        public decimal TotalAmountPaid { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;
        public string ReturnUrl { get; set; }

    }
}
