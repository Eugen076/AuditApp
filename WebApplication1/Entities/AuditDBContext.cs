using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Entities
{
    public class AuditDbContext : DbContext 
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options){ }

        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
