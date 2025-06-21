using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InvestmentPortfolioManagement.Common; 

namespace InvestmentPortfolioManagement.Models
{
    public class RiskProfile
    {
        [Key]
        public Guid RiskProfileId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; } 

        [Required]
        public RiskLevel RiskLevel { get; set; } 

        public string Description { get; set; } = string.Empty; 

        [Required]
        public DateTime AssessedOn { get; set; } = DateTime.UtcNow; 
    }
}