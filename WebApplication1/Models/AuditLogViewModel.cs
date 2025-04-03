using System.ComponentModel;

namespace WebApplication1.Models
{
    public class AuditLogViewModel
    {
        public int Id { get; set; }
        [DisplayName("Username")]
        public string? UserName { get; set; }
        [DisplayName("Action type")]
        public string Action { get; set; }
        [DisplayName("Timestamp")]
        public DateTime Timestamp { get; set; }
        [DisplayName("Ip")]
        public string IpAddress { get; set; }
        [DisplayName("Details")]
        public string Details { get; set; }

    }
}
