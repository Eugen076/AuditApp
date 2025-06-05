namespace WebApplication1.Models
{
    public class EmployeeDashboardViewModel
    {
        public int TotalClients { get; set; }
        public int TotalAccounts { get; set; }
        public int TotalTransactions { get; set; } = 0;

        public List<ClientSummary> ClientSummaries { get; set; } = new();
    }

    public class ClientSummary
    {
        public string ClientName { get; set; } = "";
        public int AccountCount { get; set; }
    }
}
