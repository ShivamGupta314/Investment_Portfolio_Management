using InvestmentPortfolioManagement.Models;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IInvestmentService
    {
        Task<Investment> CreateInvestmentAsync(Investment investment);
        Task<List<Investment>> GetUserInvestmentsAsync(Guid userId);
    }
}
