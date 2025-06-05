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

            var model = new EmployeeDashboardViewModel
            {
                TotalClients = clients.Count,
                TotalAccounts = clients.SelectMany(c => c.BankAccounts).Count(),
                TotalTransactions = 0,

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
