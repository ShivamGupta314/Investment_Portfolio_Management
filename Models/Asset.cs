
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentPortfolioManagement.Models
{
    public class Asset
    {
        [Key]
        public Guid AssetId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        public string AssetType { get; set; } = string.Empty;

        [Required]
        public decimal BasePrice { get; set; }

        public decimal CurrentPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        public decimal TotalValue { get; set; }

        public Guid? PortfolioId { get; set; } // null means Admin-created asset

        [ForeignKey("PortfolioId")]
        public Portfolio? Portfolio { get; set; }

        public DateTime PurchasedDate { get; set; }

        [NotMapped]
        public double Momentum { get; set; } = 0;
    }
}
