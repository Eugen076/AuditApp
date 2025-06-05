namespace WebApplication1.Models
{
    public class DetailedCalculationViewModel
    {
        public decimal LoanAmount { get; set; }
        public int LoanTermMonths { get; set; }
        public decimal MonthlyPayment { get; set; }
        public string CreditType { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalAmountPaid { get; set; }
     
    }
}