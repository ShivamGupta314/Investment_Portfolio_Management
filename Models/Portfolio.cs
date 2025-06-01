using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentPortfolioManagement.Models
{
    // Represents an investment portfolio linked to a user.
    public class Portfolio
    {
        [Key]
        public int PortfolioId { get; set; }

        [Required]
        public string PortfolioName { get; set; }

        public string Type { get; set; } // e.g., Stocks, Mutual Funds, Crypto

        [DataType(DataType.Currency)]
        public decimal TotalValue { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Foreign Key for User
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } // Navigation Property
    }
}
