using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManagement.Models
{
    // User entity representing application users.
    public class User
    {
        [Key]
        public int UserId { get; set; } // Primary Key
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } // Investor or Admin

        public ICollection<Portfolio>? Portfolios { get; set; }
    }
}
