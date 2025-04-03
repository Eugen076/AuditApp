using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Entities
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Account))]  // Specifică FK corect
        public int? UserId { get; set; }

        public string? UserName { get; set; }

        public string Action { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string IpAddress { get; set; }
        public string Details { get; set; }

        // Definim relația cu UserAccount
        public virtual UserAccount Account { get; set; }
    }

}
