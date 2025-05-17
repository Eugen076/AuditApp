using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Entities;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class BankAccountsController : Controller
    {
        private readonly AuditDbContext _context;
        private readonly IAuditService _auditService;

        public BankAccountsController(AuditDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }
        public async Task<IActionResult> Index(int? customerId)
        {
            var query = _context.BankAccounts
                .Include(b => b.Customer)
                .AsQueryable();

            if (customerId.HasValue)
            {
                query = query.Where(b => b.CustomerId == customerId);
                ViewBag.CustomerId = customerId; 
            }

            var accounts = await query.ToListAsync();
            return View(accounts);
        }

        // GET: BankAccounts/Create
        public IActionResult Create(int customerId)
        {
            var bankAccount = new BankAccount
            {
                CustomerId = customerId,  
                IBAN = GenerateIBAN(),    
                CreatedAt = DateTime.Now, 
                Currency = "RON"          
            };

            return View(bankAccount);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BankAccount bankAccount)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bankAccount);
                await _context.SaveChangesAsync();

                var customer = await _context.Customers.FindAsync(bankAccount.CustomerId);
                var customerName = customer?.FullName ?? "Unknown";

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = User.Identity?.Name;
                var details = $"Created bank account for customer '{customerName}' with IBAN: {bankAccount.IBAN}";

                await _auditService.LogAsync(userId, userName, "BankAccountCreated", details);

                TempData["SuccessMessage"] = "Contul bancar a fost creat cu succes!";
                return RedirectToAction("Index", new { customerId = bankAccount.CustomerId });
            }

            return View(bankAccount);
        }


        private string GenerateIBAN()
        {
            var random = new Random();
            var uniqueNumber = string.Concat(Enumerable.Range(0, 16).Select(_ => random.Next(0, 10)));
            return $"RO49AAAA{uniqueNumber}";
        }

        // GET: BankAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankAccount = await _context.BankAccounts
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bankAccount == null)
            {
                return NotFound();
            }

            return View(bankAccount);
        }

        // POST: BankAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bankAccount = await _context.BankAccounts.FindAsync(id);

            if (bankAccount == null)
            {
                TempData["ErrorMessage"] = "Contul bancar nu a fost găsit.";
                return RedirectToAction("Index");
            }

            var customerId = bankAccount.CustomerId;
       
            _context.BankAccounts.Remove(bankAccount);
            await _context.SaveChangesAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name;

            var customer = await _context.Customers.FindAsync(customerId);
            var customerName = customer?.FullName ?? "Necunoscut";

            var details = $"Deleted bank account for customer '{customerName}' with IBAN: {bankAccount.IBAN}";

            await _auditService.LogAsync(userId, userName, "BankAccountDeleted", details);

            TempData["SuccessMessage"] = "Contul bancar a fost șters cu succes!";
            return RedirectToAction("Index", new { customerId = customerId });
        }


        // GET: BankAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankAccount = await _context.BankAccounts.FindAsync(id);
            if (bankAccount == null)
            {
                return NotFound();
            }

            return View(bankAccount);
        }

        // POST: BankAccounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BankAccount bankAccount)
        {
            if (id != bankAccount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bankAccount);
                    await _context.SaveChangesAsync();

                    var customerId = bankAccount.CustomerId;

                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var userName = User.Identity?.Name;

                    var customer = await _context.Customers.FindAsync(customerId);
                    var customerName = customer?.FullName ?? "Necunoscut";

                    var details = $"Edited bank account for customer '{customerName}' with IBAN: {bankAccount.IBAN}";

                    await _auditService.LogAsync(userId, userName, "BankAccountEdited", details);

                    TempData["SuccessMessage"] = "Contul bancar a fost actualizat cu succes!";
                    return RedirectToAction("Index", new { customerId = bankAccount.CustomerId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BankAccountExists(bankAccount.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(bankAccount);
        }

        // Helper method (privată)
        private bool BankAccountExists(int id)
        {
            return _context.BankAccounts.Any(e => e.Id == id);
        }

    }
}
