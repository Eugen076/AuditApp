namespace WebApplication1.Models
{
    public class AuditLogFilterViewModel
    {
        
        public string? UserName { get; set; }
        public string? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalCount { get; set; }


        public List<AuditLogViewModel> Results { get; set; } = new();
    }

}
