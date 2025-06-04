using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManagement.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly ApplicationDbContext _context;

        public PortfolioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync(Guid userId)
        {
            return await _context.Portfolios
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<Portfolio> GetPortfolioByIdAsync(Guid id)
        {
            return await _context.Portfolios.FindAsync(id);
        }

        public async Task AddPortfolioAsync(Portfolio portfolio)
        {
            _context.Portfolios.Add(portfolio);
            await _context.SaveChangesAsync();
        }

        public async Task<Portfolio?> UpdatePortfolioAsync(Portfolio portfolio)
        {
            _context.Portfolios.Update(portfolio);
            await _context.SaveChangesAsync();
            return portfolio;
        }

        public async Task DeletePortfolioAsync(Guid id)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio != null)
            {
                _context.Portfolios.Remove(portfolio);
                await _context.SaveChangesAsync();
            }
        }
    }
}
