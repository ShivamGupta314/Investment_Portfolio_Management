namespace InvestmentPortfolioManagement.Models
{
    // User entity representing application users.
    public class User
    {
        public int UserId { get; set; } // Primary Key
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // Investor or Admin

        public ICollection<Portfolio> Portfolios { get; set; }
    }
}
