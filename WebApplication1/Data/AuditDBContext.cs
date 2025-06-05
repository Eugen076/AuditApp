using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Entities;
using WebApplication1.Enums; 

namespace WebApplication1.Data
{
    public class AuditDbContext : IdentityDbContext<UserAccount>
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Loan> Loans { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BankAccount>()
                .Property(b => b.Type)
                .HasConversion<string>();
        }

    }
}
