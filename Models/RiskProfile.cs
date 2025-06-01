namespace InvestmentPortfolioManagement.Models
{
    public class RiskProfile
    {
        public int RiskProfileId { get; set; }

        public int UserId { get; set; }
        public string RiskLevel { get; set; } // Low, Medium, High
        public string Description { get; set; }

        public DateTime AssessedOn { get; set; } = DateTime.UtcNow;
    }
}
