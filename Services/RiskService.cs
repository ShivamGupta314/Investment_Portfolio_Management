using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Models;
using InvestmentPortfolioManagement.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentPortfolioManagement.Interfaces;

namespace InvestmentPortfolioManagement.Services
{
    public class RiskService : IRiskService
    {
        private readonly ApplicationDbContext _context;

        private static readonly Dictionary<string, decimal> AssetBaseRiskScores = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            { "Stock", 80M },
            { "Bond", 20M },
            { "Mutual Fund", 50M }
        };

        private const decimal LOW_RISK_THRESHOLD = 35M;
        private const decimal MEDIUM_RISK_THRESHOLD = 70M;

        public RiskService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calculates risk and stores it as a NEW time-stamped record. Designed for high-frequency updates.
        /// </summary>
        public async Task<PortfolioRiskAnalysis> CalculateAndStorePortfolioRisk(Guid portfolioId)
        {
            var portfolio = await _context.Portfolios
                .Include(p => p.Investments)
                    .ThenInclude(inv => inv.Asset)
                .FirstOrDefaultAsync(p => p.PortfolioId == portfolioId);

            if (portfolio == null)
            {
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");
            }

            // MODIFIED: This method now ALWAYS creates a new record by calling a dedicated helper.
            // The logic for calculation remains the same.
            if (!portfolio.Investments.Any() || portfolio.Investments.Sum(i => i.Quantity) == 0)
            {
                var noInvestmentRisk = CreateNewRiskRecord(portfolioId, 0M, RiskLevel.Low, "No investments in portfolio.");
                await _context.PortfolioRiskAnalyses.AddAsync(noInvestmentRisk);
                await _context.SaveChangesAsync();
                return noInvestmentRisk;
            }

            decimal totalPortfolioValue = portfolio.Investments.Sum(inv => (inv.Asset?.CurrentPrice ?? 0M) * inv.Quantity);

            if (totalPortfolioValue == 0)
            {
                var zeroValueRisk = CreateNewRiskRecord(portfolioId, 0M, RiskLevel.Low, "Portfolio total value is zero.");
                await _context.PortfolioRiskAnalyses.AddAsync(zeroValueRisk);
                await _context.SaveChangesAsync();
                return zeroValueRisk;
            }

            decimal weightedRiskScore = 0M;
            var analysisDetails = new List<string>();

            foreach (var investment in portfolio.Investments)
            {
                string assetType = investment.Asset?.AssetType ?? "Unknown";
                decimal assetCurrentPrice = investment.Asset?.CurrentPrice ?? (decimal)investment.PurchasePrice;
                decimal investmentValue = assetCurrentPrice * investment.Quantity;
                decimal weight = investmentValue / totalPortfolioValue;

                if (RiskConstants.AssetBaseRiskScores.TryGetValue(assetType, out decimal baseRisk))
                {
                    weightedRiskScore += weight * baseRisk;
                    analysisDetails.Add($"{investment.Asset.Name} ({assetType}): Weight {weight:P1}, Contribution: {(weight * baseRisk):F2}");
                }
                else
                {
                    const decimal defaultUnknownRisk = 60M;
                    weightedRiskScore += weight * defaultUnknownRisk;
                    analysisDetails.Add($"{investment.Asset.Name} (Unknown Type): Weight {weight:P1}, Contribution: {(weight * defaultUnknownRisk):F2}");
                }
            }

            RiskLevel riskLevel = GetPortfolioRiskLevelFromScore(weightedRiskScore);
            string finalAnalysisDetails = $"Calculated based on {portfolio.Investments.Count} investments. Total Value: {totalPortfolioValue:C}.\n" +
                                          string.Join("\n", analysisDetails);

            var finalRiskRecord = CreateNewRiskRecord(portfolioId, weightedRiskScore, riskLevel, finalAnalysisDetails);
            await _context.PortfolioRiskAnalyses.AddAsync(finalRiskRecord);
            await _context.SaveChangesAsync();
            return finalRiskRecord;
        }

        /// <summary>
        /// Retrieves the single latest risk analysis for a specific portfolio.
        /// </summary>
        public async Task<PortfolioRiskAnalysis?> GetPortfolioRiskAnalysis(Guid portfolioId)
        {
            return await _context.PortfolioRiskAnalyses
                .Where(r => r.PortfolioId == portfolioId)
                .OrderByDescending(r => r.AnalysisDate)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves risk history by a number of days (for long-term trend).
        /// </summary>
        public async Task<List<PortfolioRiskAnalysis>> GetPortfolioRiskHistory(Guid portfolioId, int days = 30)
        {
            var historyDate = DateTime.UtcNow.AddDays(-days);
            return await GetPortfolioRiskHistory(portfolioId, historyDate);
        }

        /// <summary>
        /// NEW METHOD: Retrieves risk history since a specific DateTime (for live charts).
        /// </summary>
        public async Task<List<PortfolioRiskAnalysis>> GetPortfolioRiskHistory(Guid portfolioId, DateTime since)
        {
            return await _context.PortfolioRiskAnalyses
                .Where(r => r.PortfolioId == portfolioId && r.AnalysisDate >= since)
                .OrderBy(r => r.AnalysisDate)
                .ToListAsync();
        }

        // Helper to create a new risk record object.
        private PortfolioRiskAnalysis CreateNewRiskRecord(Guid portfolioId, decimal riskScore, RiskLevel riskLevel, string analysisDetails)
        {
            return new PortfolioRiskAnalysis
            {
                PortfolioRiskAnalysisId = Guid.NewGuid(),
                PortfolioId = portfolioId,
                RiskScore = riskScore,
                RiskLevel = riskLevel,
                AnalysisDetails = analysisDetails,
                AnalysisDate = DateTime.UtcNow
            };
        }

        private RiskLevel GetPortfolioRiskLevelFromScore(decimal score)
        {
            if (score <= LOW_RISK_THRESHOLD) return RiskLevel.Low;
            if (score <= MEDIUM_RISK_THRESHOLD) return RiskLevel.Medium;
            return RiskLevel.High;
        }

        // --- User Risk Profile Methods (No changes needed here) ---
        public async Task<RiskProfile> CreateOrUpdateUserRiskProfile(Guid userId, RiskLevel riskLevel, string description)
        {
            // ... (your existing code is fine) ...
            var existingProfile = await _context.RiskProfiles.FirstOrDefaultAsync(rp => rp.UserId == userId);
            if (existingProfile != null) { /* update */ } else { /* create */ }
            await _context.SaveChangesAsync();
            return (await _context.RiskProfiles.FirstOrDefaultAsync(rp => rp.UserId == userId))!;
        }

        public async Task<RiskProfile?> GetUserRiskProfile(Guid userId)
        {
            // ... (your existing code is fine) ...
            return await _context.RiskProfiles.FirstOrDefaultAsync(rp => rp.UserId == userId);
        }
    }
}