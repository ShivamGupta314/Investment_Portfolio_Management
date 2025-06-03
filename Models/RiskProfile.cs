using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManagement.Models
{
    public class RiskProfile
    {
        [Key]
        public Guid RiskProfileId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        public string RiskLevel { get; set; } = string.Empty;
        public string Description { get; set; }

        public DateTime AssessedOn { get; set; } = DateTime.UtcNow;
    }
}
