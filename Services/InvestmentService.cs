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
    public class InvestmentService :IInvestmentService
    {
        private readonly ApplicationDbContext _context;

        public InvestmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Investment> CreateInvestmentAsync(Investment investment)
        {
            var asset = await _context.Assets.FirstOrDefaultAsync(a => a.AssetId == investment.AssetId);
            //var portfolio = await _context.Portfolios.FirstOrDefaultAsync(p => p.PortfolioId == investment.PortfolioId);

            if (asset == null)
            {
                throw new Exception("Asset not found.");
            }

            // Assign asset data before saving
            investment.AssetName = asset.Name;
            investment.PurchasePrice = (double)asset.BasePrice;
            investment.CurrentPrice = (double)asset.CurrentPrice;

            await _context.Investments.AddAsync(investment);
            await _context.SaveChangesAsync();

            return investment;
        }



        public async Task<List<Investment>> GetUserInvestmentsAsync(Guid userId)
        {
            return await _context.Investments
                .Include(i => i.Asset)
                .Where(i => i.UserId == userId)
                .ToListAsync();
        }
    }
}
