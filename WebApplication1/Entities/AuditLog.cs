using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Entities
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }  
        public int UserId { get; set; }  
        public string Action { get; set; } 
        public DateTime Timestamp { get; set; }  
        public string IpAddress { get; set; }  
        public string Details { get; set; }  

        public UserAccount Account { get; set; }
       
    }

}
