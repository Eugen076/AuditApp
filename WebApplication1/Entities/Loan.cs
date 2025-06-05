using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Entities
{
    public class Loan
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        [Required]
        [StringLength(50)]
        public string LoanType { get; set; }  

        [Column(TypeName = "decimal(5,4)")]
        public decimal InterestRate { get; set; }  

        public int LoanTermMonths { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyPayment { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmountPaid { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;
    }
}
