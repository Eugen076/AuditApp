namespace WebApplication1.Services
{
    public interface ICreditService
    {
        decimal CalculateMonthlyPayment(decimal loanAmount, int loanTermMonths, decimal annualInterestRate);
    }
}