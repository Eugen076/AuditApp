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


            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.Account)  // AuditLog are UN UserAccount
                .WithMany(u => u.AuditLogs)  // UserAccount are MAI MULTE AuditLogs
                .HasForeignKey(a => a.UserId)  // FK-ul corect este UserId
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
