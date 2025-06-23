using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InvestmentPortfolioManagement.Common; 

namespace InvestmentPortfolioManagement.Models
{
    public class PortfolioRiskAnalysis
    {
        [Key]
        public Guid PortfolioRiskAnalysisId { get; set; } 

        [Required]
        public Guid PortfolioId { get; set; }

        [ForeignKey("PortfolioId")]
        public Portfolio? Portfolio { get; set; } 

        [Required]
        [Column(TypeName = "decimal(18, 2)")] 
        public decimal RiskScore { get; set; } 

        [Required]
        public RiskLevel RiskLevel { get; set; } 

        public string? AnalysisDetails { get; set; }

        [Required]
        public DateTime AnalysisDate { get; set; }
    }
}