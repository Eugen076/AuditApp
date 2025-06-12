using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace WebApplication1.Entities
{

    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(13, MinimumLength = 13)]
        public string CNP { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        public bool IsActive { get; set; } = true;

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [ValidateNever]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(CreatedBy))]
        public string? CreatedById { get; set; }

        [ValidateNever]
        public virtual UserAccount CreatedBy { get; set; }

        [ValidateNever]
        public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    }

}
