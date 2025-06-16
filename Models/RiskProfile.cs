using System.ComponentModel.DataAnnotations;
using InvestmentPortfolioManagement.Common;

namespace InvestmentPortfolioManagement.Models
{
    public class RiskProfile
    {
        [Key]
        public Guid RiskProfileId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        public RiskLevel RiskLevel { get; set; }   
        public string Description { get; set; }

        public DateTime AssessedOn { get; set; } = DateTime.UtcNow;
    }
}
