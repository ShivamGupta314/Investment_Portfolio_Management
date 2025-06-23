//using InvestmentPortfolioManagement.Data;
//using InvestmentPortfolioManagement.Interfaces;
//using InvestmentPortfolioManagement.Models;
//using Microsoft.EntityFrameworkCore;

//namespace InvestmentPortfolioManagement.Services
//{
//    public class PortfolioService : IPortfolioService
//    {
//        private readonly ApplicationDbContext _context;

//        public PortfolioService(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync(Guid userId)
//        {
//            return await _context.Portfolios
//                .Where(p => p.UserId == userId)
//                .ToListAsync();
//        }
//        public async Task<Portfolio> GetPortfolioByIdAsync(Guid id)
//        {
//            // This is the correct way to eager-load the related data
//            var result =  await _context.Portfolios
//                .Include(p => p.Investments)         // Eager load the Investments associated with the Portfolio
//                    .ThenInclude(i => i.Asset)       // Eager load the Asset for each Investment
//                .Include(p => p.Assets)              // Eager load the Assets directly associated with the Portfolio
//                .FirstOrDefaultAsync(p => p.PortfolioId == id);
//            Console.WriteLine($"Investments count : {result?.Investments?.Count}");


//            return result;
//        }



//        public async Task AddPortfolioAsync(Portfolio portfolio)
//        {
//            _context.Portfolios.Add(portfolio);
//            await _context.SaveChangesAsync();
//        }

//        public async Task<Portfolio?> UpdatePortfolioAsync(Portfolio portfolio)
//        {
//            var existingPortfolio = await _context.Portfolios.FindAsync(portfolio.PortfolioId);
//            if (existingPortfolio != null)
//            {
//                existingPortfolio.Name = portfolio.Name;
//                existingPortfolio.Description = portfolio.Description;
//                existingPortfolio.TotalValue = portfolio.TotalValue;
//                existingPortfolio.Type = portfolio.Type;
//                existingPortfolio.UserId = portfolio.UserId;
//                await _context.SaveChangesAsync();
//                return existingPortfolio;
//            }


//            return null;
//        }

//        public async Task DeletePortfolioAsync(Guid id)
//        {
//            var portfolio = await _context.Portfolios.FindAsync(id);
//            if (portfolio != null)
//            {
//                _context.Portfolios.Remove(portfolio);
//                await _context.SaveChangesAsync();
//            }
//        }


//        public async Task<List<Investment>> GetUnassignedInvestmentsAsync(Guid userId)
//        {
//            return await _context.Investments
//                .Where(i => i.UserId == userId && i.PortfolioId == null)
//                .ToListAsync();
//        }

//        public async Task AssignInvestmentsToPortfolioAsync(Guid portfolioId, List<Guid> investmentIds)
//        {
//            var portfolioExists = await _context.Portfolios.AnyAsync(p => p.PortfolioId == portfolioId);
//            if (!portfolioExists)
//            {
//                throw new ArgumentException("The specified portfolio does not exist.");
//            }

//            var investments = await _context.Investments
//                .Where(i => investmentIds.Contains(i.InvestmentId))
//                .ToListAsync();

//            foreach (var investment in investments)
//            {
//                investment.PortfolioId = portfolioId;
//            }

//            await _context.SaveChangesAsync();
//        }
//        public decimal CalculatePortfolioValue(Guid portfolioId)
//        {
//            var investments = _context.Investments
//                                      .Where(i => i.PortfolioId == portfolioId)
//                                      .ToList();
//            decimal totalValue = investments.Sum(i => i.Quantity * i.CurrentPrice);
//            return totalValue;
//        }

//    }
//}




using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            // For GetAllPortfolios, consider if you want to recalculate all of them.
            // If the Index page just shows a list with TotalValue, you might want to recalculate
            // each one here, or have a background job keep them updated.
            // For now, let's keep it simple, and GetPortfolioByIdAsync will ensure individual portfolio values are current.
            return await _context.Portfolios
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<Portfolio> GetPortfolioByIdAsync(Guid id)
        {
            var portfolio = await _context.Portfolios
                .Include(p => p.Investments)         // Eager load the Investments associated with the Portfolio
                    .ThenInclude(i => i.Asset)       // Eager load the Asset for each Investment
                .Include(p => p.Assets)              // Eager load the Assets directly associated with the Portfolio
                .FirstOrDefaultAsync(p => p.PortfolioId == id);

            if (portfolio != null)
            {
                // **Crucial Change:** Recalculate and update TotalValue before returning the portfolio
                // This ensures the TotalValue is always current when fetched
                await CalculateAndSetPortfolioTotalValueAsync(portfolio.PortfolioId); // This call also saves changes

                // Re-fetch the portfolio to get the updated TotalValue if CalculateAndSetPortfolioTotalValueAsync
                // modifies it and the current 'portfolio' object is not tracking the change immediately.
                // However, since CalculateAndSetPortfolioTotalValueAsync is operating on a tracked entity,
                // the 'portfolio' object should already reflect the change after SaveChanges.
                // But if you want to be absolutely sure, or if your context might be disposed/recreated
                // between the calculation and return, a re-fetch might be considered (though usually not necessary).
                // For simplicity and efficiency, trust the tracking for now.
                // If you face issues where TotalValue isn't updated in the returned object, uncomment the line below:
                // portfolio = await _context.Portfolios.AsNoTracking().FirstOrDefaultAsync(p => p.PortfolioId == id);
            }

            return portfolio;
        }

        public async Task AddPortfolioAsync(Portfolio portfolio)
        {
            portfolio.TotalValue = 0.0M;
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
                //existingPortfolio.Type = portfolio.Type;
                existingPortfolio.UserId = portfolio.UserId;
                await _context.SaveChangesAsync();

                // After updating other details, ensure TotalValue is current
                await CalculateAndSetPortfolioTotalValueAsync(existingPortfolio.PortfolioId);
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
                .Include(i => i.Asset)
                .ToListAsync();
        }

        public async Task AssignInvestmentsToPortfolioAsync(Guid portfolioId, List<Guid> investmentIds)
        {
            var portfolio = await _context.Portfolios.FirstOrDefaultAsync(p => p.PortfolioId == portfolioId);
            if (portfolio == null)
            {
                throw new ArgumentException("The specified portfolio does not exist.");
            }

            // Fetch investments to be assigned
            var investmentsToAssign = await _context.Investments
                .Where(i => investmentIds.Contains(i.InvestmentId))
                .ToListAsync();

            foreach (var investment in investmentsToAssign)
            {
                investment.PortfolioId = portfolioId;
            }

            await _context.SaveChangesAsync();

            // After assigning, recalculate and update the portfolio's total value
            await CalculateAndSetPortfolioTotalValueAsync(portfolioId);
        }

        // This method will calculate and update the TotalValue of a specific portfolio
        public async Task CalculateAndSetPortfolioTotalValueAsync(Guid portfolioId)
        {
            var portfolio = await _context.Portfolios
                                    .Include(p => p.Investments)
                                        .ThenInclude(i => i.Asset) // Crucial to get CurrentPrice from Asset
                                    .FirstOrDefaultAsync(p => p.PortfolioId == portfolioId);

            if (portfolio == null)
            {
                // Log an error or handle the case where the portfolio is not found
                return;
            }

            decimal totalCalculatedValue = 0.0M;

            if (portfolio.Investments != null)
            {
                // Sum the current value of all investments
                totalCalculatedValue = portfolio.Investments
                                            .Where(i => i.Asset != null) // Ensure asset exists for calculation
                                            .Sum(i => (decimal)i.Quantity * i.Asset.CurrentPrice);
            }

            // Update the portfolio's TotalValue if it has changed
            if (portfolio.TotalValue != totalCalculatedValue)
            {
                portfolio.TotalValue = totalCalculatedValue;
                await _context.SaveChangesAsync();
            }
        }
    }
}