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
        public IActionResult Create(decimal? loanAmount, string loanType, int? loanTermYears, string returnUrl = null)
        {
            var isIpotecar = loanType == "Imobiliar-Ipotecar";

            var model = new CreateLoanViewModel
            {
                Customers = _context.Customers.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.FullName
                }).ToList(),

                Amount = loanAmount ?? 0,
                LoanType = loanType ?? "Nevoi personale",
                LoanTermYears = loanTermYears ?? 1,
                ReturnUrl = returnUrl ?? Url.Action("Index", "Credit"),

                MinAmount = isIpotecar ? 7000 : 5000,
                MaxAmount = isIpotecar ? 1200000 : 250000,
                MinYears = isIpotecar ? 6 : 1,
                MaxYears = isIpotecar ? 30 : 5
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLoanViewModel model)
        {
            var isIpotecar = model.LoanType == "Imobiliar-Ipotecar";
            model.MinAmount = isIpotecar ? 7000 : 5000;
            model.MaxAmount = isIpotecar ? 1200000 : 250000;
            model.MinYears = isIpotecar ? 6 : 1;
            model.MaxYears = isIpotecar ? 30 : 5;

            if (model.Amount < model.MinAmount || model.Amount > model.MaxAmount)
            {
                ModelState.AddModelError("Amount", $"Suma trebuie să fie între {model.MinAmount:N0} și {model.MaxAmount:N0} LEI.");
            }

            if (model.LoanTermYears < model.MinYears || model.LoanTermYears > model.MaxYears)
            {
                ModelState.AddModelError("LoanTermYears", $"Durata trebuie să fie între {model.MinYears} și {model.MaxYears} ani.");
            }

            if (!ModelState.IsValid)
            {
                model.Customers = _context.Customers.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.FullName,
                    Selected = (c.Id == model.CustomerId)
                }).ToList();

                return View(model);
            }

            var customer = await _context.Customers.FindAsync(model.CustomerId);
            if (customer == null)
            {
                ModelState.AddModelError("CustomerId", "Clientul selectat nu există.");
                model.Customers = _context.Customers.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.FullName
                }).ToList();

                return View(model);
            }

            model.InterestRate = isIpotecar ? 0.055m : 0.085m;
            var loanTermMonths = model.LoanTermYears * 12;
            model.MonthlyPayment = CalculateMonthlyPayment(model.Amount, loanTermMonths, model.InterestRate);
            model.TotalAmountPaid = Math.Round(model.MonthlyPayment * loanTermMonths, 2);

            var loan = new Loan
            {
                Amount = model.Amount,
                CustomerId = model.CustomerId.Value,
                LoanType = model.LoanType,
                InterestRate = model.InterestRate,
                LoanTermMonths = loanTermMonths,
                MonthlyPayment = model.MonthlyPayment,
                TotalAmountPaid = model.TotalAmountPaid,
                StartDate = model.StartDate
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

/*            TempData["SuccessMessage"] = "Creditul a fost creat cu succes!";
            return Redirect(model.ReturnUrl ?? Url.Action("Index", "Credit"));*/

            return Redirect(model.ReturnUrl ?? Url.Action("Index", "Credit"));
        }

        private decimal CalculateMonthlyPayment(decimal principal, int months, decimal annualRate)
        {
            var monthlyRate = annualRate / 12;
            var factor = (decimal)Math.Pow(1 + (double)monthlyRate, months);
            var monthly = (principal * monthlyRate * factor) / (factor - 1);
            return Math.Round(monthly, 2);
        }
        [HttpGet]
        public IActionResult SearchClients(string term)
        {
            var results = _context.Customers
                .Where(c => c.FullName.Contains(term))
                .Select(c => new { id = c.Id, name = c.FullName })
                .Take(10)
                .ToList();

            return Json(results);
        }

    }
}
