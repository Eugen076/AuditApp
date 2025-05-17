using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Entities
{
    public class BankAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(24)]
        public string IBAN { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        [ValidateNever]
        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "RON";

        [ValidateNever]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }

        [ValidateNever]
        public virtual Customer Customer { get; set; }
    }
}
