using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentPortfolioManagement.Models
{
    // Represents an investment portfolio linked to a user.
    public class Portfolio
    {
        [Key]
        public Guid PortfolioId { get; set; }

        [Required]
        public string PortfolioName { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Type { get; set; } 

        [DataType(DataType.Currency)]
        public decimal TotalValue { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Foreign Key for User
        [ForeignKey("UserId")]
        public Guid UserId { get; set; }


        public User User { get; set; } // Navigation Property
        public ICollection<Asset>? Assets { get; set; }

    }
}
