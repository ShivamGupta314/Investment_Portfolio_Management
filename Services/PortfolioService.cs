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
            var existingPortfolio = await _context.Portfolios.FindAsync(portfolio.PortfolioId);
            if (existingPortfolio != null)
            {
                existingPortfolio.Name = portfolio.Name;
                existingPortfolio.Description = portfolio.Description;
                existingPortfolio.TotalValue = portfolio.TotalValue;
                existingPortfolio.Type = portfolio.Type;
                existingPortfolio.UserId = portfolio.UserId;
                await _context.SaveChangesAsync();
                return existingPortfolio;
            }
            
           
            return null;
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


        public async Task<List<Investment>> GetUnassignedInvestmentsAsync(Guid userId)
        {
            return await _context.Investments
                .Where(i => i.UserId == userId && i.PortfolioId == null)
                .ToListAsync();
        }

        public async Task AssignInvestmentsToPortfolioAsync(Guid portfolioId, List<Guid> investmentIds)
        {
            var portfolioExists = await _context.Portfolios.AnyAsync(p => p.PortfolioId == portfolioId);
            if (!portfolioExists)
            {
                throw new ArgumentException("The specified portfolio does not exist.");
            }

            var investments = await _context.Investments
                .Where(i => investmentIds.Contains(i.InvestmentId))
                .ToListAsync();

            foreach (var investment in investments)
            {
                investment.PortfolioId = portfolioId;
            }

            await _context.SaveChangesAsync();
        }

    }
}
