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

        public async Task<IActionResult> Index(int days = 7)
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-days + 1);

            var totalEmployees = await _context.Users.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var failedLogins = await _context.AuditLogs
                .CountAsync(log => log.Action == "FailedLogin");
            var loginAlerts = await _context.Users
                .CountAsync(u => u.LockoutEnd != null && u.LockoutEnd > DateTime.UtcNow);

            var loginLogsRaw = await _context.AuditLogs
                .Where(a => a.Action == "Login" && a.Timestamp.Date >= startDate && a.Timestamp.Date <= endDate)
                .ToListAsync();

            var loginLogsGrouped = loginLogsRaw
                .GroupBy(a => a.Timestamp.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset));

            var loginLogs = allDates
                .Select(date => new
                {
                    Date = date.ToString("dd.MM.yyyy"),
                    Count = loginLogsGrouped.ContainsKey(date) ? loginLogsGrouped[date] : 0
                })
                .ToList();

            var model = new
            {
                TotalEmployees = totalEmployees,
                TotalCustomers = totalCustomers,
                FailedLogins = failedLogins,
                LoginAlerts = loginAlerts,
                LoginsLogs = loginLogs.Cast<dynamic>().ToList(),
                Days = days
            };

            return View(model);
        }


    }
}
