using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using WebApplication1.Entities;

namespace WebApplication1.Controllers
{
    public class AuditLogController : Controller
    {
        private readonly AuditDbContext _context;

        public AuditLogController(AuditDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var logs = _context.AuditLogs.OrderByDescending(l => l.Timestamp).ToList();
            return View(logs);
        }

        public void LogAuditEvent(int userId, string action, string details)
        {
            var logEntry = new AuditLog
            {
                UserId = userId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = details
            };

            _context.AuditLogs.Add(logEntry);
            _context.SaveChanges();
        }
    }
}