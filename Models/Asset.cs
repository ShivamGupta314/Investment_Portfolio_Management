using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentPortfolioManagement.Models
{
    // Represents an individual asset within a portfolio
    public class Asset
    {
        [Key]
        public Guid AssetId { get; set; }

        [Required]
        public string AssetName { get; set; }

        public string AssetType { get; set; } // e.g., Stock, Mutual Fund, Crypto

        [Range(0, double.MaxValue)]
        public int Quantity { get; set; }

        [DataType(DataType.Currency)]
        public decimal PurchasePrice { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        // Foreign key to Portfolio
        public Guid PortfolioId { get; set; }

        [ForeignKey("PortfolioId")]
        public Portfolio Portfolio { get; set; } // Navigation property
    }
}
