using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Enums;

namespace WebApplication1.Entities
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public int SourceAccountId { get; set; }

        [ForeignKey("SourceAccountId")]
        public BankAccount SourceAccount { get; set; }


        public int? TargetAccountId { get; set; }

        [ForeignKey("TargetAccountId")]
        public BankAccount? TargetAccount { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public string? PerformedBy { get; set; }
    }
}
