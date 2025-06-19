using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
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

        [HttpGet]
        public async Task<IActionResult> ExportToCsv(string? userName, string? actionType, DateTime? fromDate, DateTime? toDate)
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

            var logs = await query.OrderByDescending(a => a.Timestamp).ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Username,Action,Timestamp,IP Address,Details");

            foreach (var log in logs)
            {
                var details = log.Details?.Replace("\"", "\"\""); // Escape pentru CSV
                csv.AppendLine($"\"{log.UserName}\",\"{log.Action}\",\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{log.IpAddress}\",\"{details}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "audit_logs.csv");
        }


        [HttpGet]
        public async Task<IActionResult> Index(string? userName, string? actionType, DateTime? fromDate, DateTime? toDate, int pageNumber = 1, int pageSize = 15)
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

            var totalCount = await query.CountAsync();

            var results = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogViewModel
                {
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
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Results = results
            };


            return View(viewModel);
        }

    }
}

