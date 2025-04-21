using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Entities
{

    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

       /* [Required]
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

        public DateTime CreatedAt { get; set; } = DateTime.Now;*/

      
 /*       [ForeignKey("CreatedBy")]
        public string CreatedById { get; set; }
        public UserAccount CreatedBy { get; set; }*/

        // public ICollection<Account> Accounts { get; set; }
    }

}
