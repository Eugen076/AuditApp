using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Controllers
{
    public class LoanController : Controller
    {
        private readonly AuditDbContext _context;

        public LoanController(AuditDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create(decimal? loanAmount, string loanType, int? loanTermYears, int? customerId, string returnUrl = null)
        {
            List<SelectListItem> customers;

            if (customerId.HasValue)
            {
                customers = _context.Customers
                    .Where(c => c.Id == customerId.Value)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.FullName,
                        Selected = true
                    })
                    .ToList();
            }
            else
            {
                customers = _context.Customers
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.FullName
                    })
                    .ToList();
            }

            var model = new CreateLoanViewModel
            {
                Customers = customers,
                CustomerId = customerId ?? 0,
                Amount = loanAmount ?? 0,
                LoanType = loanType ?? "Nevoi personale",
                LoanTermYears = loanTermYears ?? 1,
                ReturnUrl = returnUrl ?? Url.Action("Index", "Credit") // fallback
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateLoanViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var customer = await _context.Customers.FindAsync(model.CustomerId);
            if (customer == null)
            {
                ModelState.AddModelError("", "Clientul nu există.");
                return View(model);
            }

            
            model.InterestRate = model.LoanType == "Imobiliar-Ipotecar" ? 0.055m : 0.085m;


            var loanTermMonths = model.LoanTermYears * 12;

            model.MonthlyPayment = CalculateMonthlyPayment(model.Amount, loanTermMonths, model.InterestRate);
            model.TotalAmountPaid = Math.Round(model.MonthlyPayment * loanTermMonths, 2);


            var loan = new Loan
            {
                Amount = model.Amount,
                CustomerId = model.CustomerId,
                LoanType = model.LoanType,
                InterestRate = model.InterestRate,
                LoanTermMonths = loanTermMonths,
                MonthlyPayment = model.MonthlyPayment,
                TotalAmountPaid = model.TotalAmountPaid,
                StartDate = model.StartDate
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return Redirect(model.ReturnUrl ?? Url.Action("Index", "Credit"));
        }

        private decimal CalculateMonthlyPayment(decimal principal, int months, decimal annualRate)
        {
            var monthlyRate = annualRate / 12;
            var factor = (decimal)Math.Pow(1 + (double)monthlyRate, months);
            var monthly = (principal * monthlyRate * factor) / (factor - 1);
            return Math.Round(monthly, 2);
        }

    }

}
