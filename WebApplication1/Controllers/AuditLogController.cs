using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly AuditDbContext _context;

        public AuditLogController(AuditDbContext context)
        {
            _context = context;
        }

        /*        public IActionResult Index()
                {
                    var logs = _context.AuditLogs
                        .OrderByDescending(log => log.Timestamp)
                        .ToList();

                    return View(logs);
                }*/
         [HttpGet]
        public async Task<IActionResult> Index(string? userName, string? actionType, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userName))
                query = query.Where(a => a.UserName.Contains(userName));

            if (!string.IsNullOrEmpty(actionType))
                query = query.Where(a => a.Action == actionType);

            if (fromDate.HasValue)
                query = query.Where(a => a.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.Timestamp <= endOfDay);
            }

            var results = await query
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new AuditLogViewModel
                {
                    /*Id = a.Id,*/
                    UserName = a.UserName,
                    Action = a.Action,
                    Timestamp = a.Timestamp,
                    IpAddress = a.IpAddress,
                    Details = a.Details
                }).ToListAsync();

            var viewModel = new AuditLogFilterViewModel
            {
                UserName = userName,
                Action = actionType,
                FromDate = fromDate,
                ToDate = toDate,
                Results = results
            };

            return View(viewModel);
        }

    }
}

