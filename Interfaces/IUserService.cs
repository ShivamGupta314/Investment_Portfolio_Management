using InvestmentPortfolioManagement.Models;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task<User> RegisterAsync(User user);
    }
}
