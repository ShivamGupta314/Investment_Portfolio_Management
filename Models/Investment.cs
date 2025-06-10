using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentPortfolioManagement.Models
{
    public class Investment
    {
        [Key]
        public Guid InvestmentId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public Guid AssetId { get; set; }

        [ForeignKey("AssetId")]
        public Asset Asset { get; set; }

        
        public Guid? PortfolioId { get; set; }

        [ForeignKey("PortfolioId")]
        public Portfolio Portfolio { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        
        public string AssetName { get; set; }
        public double PurchasePrice { get; set; }
        public double CurrentPrice { get; set; }
        
public DateTime InvestmentDate { get; set; }
    }
}
