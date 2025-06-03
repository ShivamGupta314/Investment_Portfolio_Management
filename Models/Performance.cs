using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentPortfolioManagement.Models
{
    // Represents performance summary of a portfolio
    public class Performance
    {
        [Key]
        public Guid PerformanceId { get; set; }

        public decimal TotalInvestment { get; set; }

        public decimal CurrentValue { get; set; }

        public decimal ProfitOrLoss => CurrentValue - TotalInvestment;

        public DateTime CalculatedOn { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public Guid PortfolioId { get; set; }

        [ForeignKey("PortfolioId")]
        public Portfolio Portfolio { get; set; }
    }
}
