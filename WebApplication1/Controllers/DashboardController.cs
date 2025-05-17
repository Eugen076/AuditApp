using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AuditDbContext _context;

        public DashboardController(AuditDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            var totalEmployees = await _context.Users.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var failedLogins = await _context.AuditLogs
                .CountAsync(log => log.Action == "FailedLogin");
            var loginAlerts = await _context.Users
                .CountAsync(u => u.LockoutEnd != null && u.LockoutEnd > DateTime.UtcNow);


            var loginLogsRaw = await _context.AuditLogs
                .Where(a => a.Action == "Login")
                .ToListAsync();

            var loginLogs = loginLogsRaw
                .GroupBy(a => a.Timestamp.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("dd.MM"),
                    Count = g.Count()
                })
                .OrderBy(g => g.Date)
                .ToList();


            var model = new
            {
                TotalEmployees = totalEmployees,
                TotalCustomers = totalCustomers,
                FailedLogins = failedLogins,
                LoginAlerts = loginAlerts,
                LoginsLogs = loginLogs.Cast<dynamic>().ToList()
            };


            return View(model);
        }

    }
}
