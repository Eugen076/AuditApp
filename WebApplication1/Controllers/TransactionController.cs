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

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Client"))
            {
                var allAccounts = await _context.BankAccounts
                    .ToListAsync();

                var accountIds = allAccounts.Select(a => a.Id).ToList();

                var clientTransactions = await _context.Transactions
                    .Include(t => t.SourceAccount)
                    .Where(t => accountIds.Contains(t.SourceAccountId))
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();


                return View(clientTransactions);
            }

            var transactions = await _context.Transactions
                .Include(t => t.SourceAccount)
                .Include(t => t.TargetAccount)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            return View(transactions);
        }

        public async Task<IActionResult> Create()
        {
                ViewBag.Accounts = await _context.BankAccounts
                    .Include(a => a.Customer)
                    .Where(a => a.IsActive)
                    .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int sourceAccountId, decimal amount, TransactionType type)
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

    }
}
