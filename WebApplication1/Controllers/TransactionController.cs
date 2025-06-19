using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Entities;
using WebApplication1.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly AuditDbContext _context;

        public TransactionsController(AuditDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? customerName, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Customers = await _context.Customers
                .Select(c => new { c.Id, c.FullName })
                .ToListAsync();

            var query = _context.Transactions
                .Include(t => t.SourceAccount)
                .ThenInclude(a => a.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(customerName))
            {
                query = query.Where(t => t.SourceAccount.Customer.FullName.Contains(customerName));
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Date <= endDate.Value);
            }

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            return View(transactions);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _context.Customers
                .OrderBy(c => c.FullName)
                .ToListAsync();

            ViewBag.Accounts = new List<BankAccount>(); // inițial gol

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int customerId, int sourceAccountId, decimal amount, TransactionType type)
        {
            var sourceAccount = await _context.BankAccounts.FindAsync(sourceAccountId);

            if (sourceAccount == null || !sourceAccount.IsActive)
            {
                TempData["Error"] = "Contul nu este activ sau nu există.";
                return RedirectToAction("Create");
            }

            if (amount <= 0)
            {
                TempData["Error"] = "Suma trebuie să fie mai mare ca 0.";
                return RedirectToAction("Create");
            }

            if (type == TransactionType.Retragere && sourceAccount.Balance < amount)
            {
                TempData["Error"] = "Fonduri insuficiente.";
                return RedirectToAction("Create");
            }

            if (type == TransactionType.Retragere)
                sourceAccount.Balance -= amount;
            else if (type == TransactionType.Depunere)
                sourceAccount.Balance += amount;

            var transaction = new Transaction
            {
                SourceAccountId = sourceAccountId,
                Amount = amount,
                Type = type,
                Date = DateTime.Now,
                PerformedBy = User.Identity?.Name
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Tranzacția a fost înregistrată cu succes!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<JsonResult> GetAccountsByCustomer(int customerId)
        {
            var accounts = await _context.BankAccounts
                .Where(a => a.CustomerId == customerId && a.IsActive)
                .Select(a => new {
                    a.Id,
                    a.IBAN,
                    a.Currency
                }).ToListAsync();

            return Json(accounts);
        }
    }
}
