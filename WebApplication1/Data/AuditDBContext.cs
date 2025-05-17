using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using WebApplication1.Entities;

namespace WebApplication1.Data
{
    public class AuditDbContext : IdentityDbContext<UserAccount>
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
    }
}

/*namespace WebApplication1.Entities
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
                .HasOne(a => a.Account)  
                .WithMany(u => u.AuditLogs)  
                .HasForeignKey(a => a.UserId)  
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}*/
