using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public interface IAuditService
    {
        Task LogAsync(string? userId, string username, string action, string description);

    }
}
