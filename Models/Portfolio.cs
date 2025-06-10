
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentPortfolioManagement.Models
{
    public class Portfolio
    {
        [Key]
        public Guid PortfolioId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;

        public decimal TotalValue { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public ICollection<Asset>? Assets { get; set; }

        public ICollection<Investment>? Investments { get; set; }
    }
}
