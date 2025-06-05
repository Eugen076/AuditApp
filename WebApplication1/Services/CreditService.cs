namespace WebApplication1.Services
{
    public class CreditService : ICreditService
    {
        public decimal CalculateMonthlyPayment(decimal loanAmount, int loanTermMonths, decimal annualInterestRate)
        {
            if (loanAmount <= 0 || loanTermMonths <= 0)
                return 0;

            decimal monthlyInterestRate = annualInterestRate / 100 / 12;

            if (monthlyInterestRate == 0)
                return Math.Round(loanAmount / loanTermMonths, 2);

            decimal factor = (decimal)Math.Pow(1 + (double)monthlyInterestRate, loanTermMonths);
            decimal monthlyPayment = loanAmount * monthlyInterestRate * factor / (factor - 1);

            return Math.Round(monthlyPayment, 2);
        }

    }
}