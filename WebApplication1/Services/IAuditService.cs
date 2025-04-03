using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public interface IAuditService
    {
        Task LogAsync(int? userId,string userName, string action, string details);
    }
}
