// Models/CreditViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class CreditViewModel
    {

/*        public string FullName { get; set; }*/


        public decimal LoanAmount { get; set; }

        public int LoanTermYears { get; set; }

        public int LoanTermMonths { get; set; }

        public decimal MonthlyPayment { get; set; }

        public string Currency { get; set; } = "LEI";

        public string CreditType { get; set; } = "Nevoi personale";
 /*       public bool IsSimulation { get; set; } = true;*/

    }
}
