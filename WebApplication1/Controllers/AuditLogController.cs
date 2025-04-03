using Microsoft.AspNetCore.Mvc;

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

        // GET: /AuditLog
        public IActionResult Index()
        {
            var logs = _context.AuditLogs
                .OrderByDescending(log => log.Timestamp)
                .ToList();

            return View(logs); 
        }
    }
}

