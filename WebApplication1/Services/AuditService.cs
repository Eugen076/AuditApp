using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using WebApplication1.Data;
using WebApplication1.Entities;

namespace WebApplication1.Services
{
    public class AuditService : IAuditService
    {
        private readonly AuditDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(AuditDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string? userId, string userName, string action, string details)

        {
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var logEntry = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow,
                IpAddress = ip
            };

            _context.AuditLogs.Add(logEntry);
            await _context.SaveChangesAsync();
        }
    }
}
