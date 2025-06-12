using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeDashboardController : Controller
    {
        private readonly AuditDbContext _context;

        public EmployeeDashboardController(AuditDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var clients = await _context.Customers
                .Include(c => c.BankAccounts)
                .ToListAsync();

    
            var ronAccounts = clients.SelectMany(c => c.BankAccounts).Count(b => b.Currency == "RON");
            var eurAccounts = clients.SelectMany(c => c.BankAccounts).Count(b => b.Currency == "EUR");
            var usdAccounts = clients.SelectMany(c => c.BankAccounts).Count(b => b.Currency == "USD");


            var model = new EmployeeDashboardViewModel
            {
                TotalClients = clients.Count,
                TotalAccounts = clients.SelectMany(c => c.BankAccounts).Count(),
                TotalTransactions = 0,
                RonAccountsCount = ronAccounts,
                EurAccountsCount = eurAccounts,
                UsdAccountsCount = usdAccounts,

                ClientSummaries = clients.Select(c => new ClientSummary
                {
                    ClientName = c.FullName,
                    AccountCount = c.BankAccounts.Count
                }).ToList()
            };

            return View(model);
        }


    }
}
