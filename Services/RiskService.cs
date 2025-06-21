// Services/RiskService.cs
using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Models;
using InvestmentPortfolioManagement.Common; // For RiskLevel enum
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

        // Define base risk scores for asset types
        // These can be configured from app settings or a database table in a real app
        private static readonly Dictionary<string, decimal> AssetBaseRiskScores = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            { "Stock", 80M },
            { "Bond", 20M },
            { "Mutual Fund", 50M },
            //{ "Cryptocurrency", 95M },
            //{ "Real Estate", 30M },
            //{ "ETF", 65M }, // Example: Add more types
            //{ "Commodity", 70M },
            // Add more asset types as needed
        };

        // Define risk score thresholds for mapping to enum RiskLevel
        private const decimal LOW_RISK_THRESHOLD = 35M;    // Score <= 35 = Low
        private const decimal MEDIUM_RISK_THRESHOLD = 70M;  // Score > 35 and <= 70 = Medium
        // Scores > 70 = High

        public RiskService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Portfolio Risk Analysis Methods ---

        /// <summary>
        /// Calculates the risk score and level for a specific portfolio based on its asset allocation
        /// and stores/updates the result in the database.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio to assess.</param>
        /// <returns>The created or updated PortfolioRiskAnalysis record.</returns>
        /// <exception cref="ArgumentException">Thrown if the portfolio is not found.</exception>
        public async Task<PortfolioRiskAnalysis> CalculateAndStorePortfolioRisk(Guid portfolioId)
        {
            var portfolio = await _context.Portfolios
                .Include(p => p.Investments)
                    .ThenInclude(inv => inv.Asset) // Eager load assets for asset type and current price
                .FirstOrDefaultAsync(p => p.PortfolioId == portfolioId);

            if (portfolio == null)
            {
                throw new ArgumentException($"Portfolio with ID {portfolioId} not found.");
            }

            if (!portfolio.Investments.Any())
            {
                // No investments, consider it very low risk
                return await CreateOrUpdatePortfolioRiskRecord(portfolioId, 0M, RiskLevel.Low, "No investments in portfolio.");
            }

            decimal totalPortfolioValue = 0;
            // Calculate total portfolio value using current asset prices
            foreach (var investment in portfolio.Investments)
            {
                // Ensure CurrentPrice is accurate from the Asset, fallback to PurchasePrice if null
                decimal assetCurrentPrice = investment.Asset?.CurrentPrice ?? (decimal)investment.PurchasePrice;
                totalPortfolioValue += assetCurrentPrice * investment.Quantity;
            }

            if (totalPortfolioValue == 0)
            {
                // Handle case where total value is zero (e.g., all quantity is 0 or prices are 0)
                return await CreateOrUpdatePortfolioRiskRecord(portfolioId, 0M, RiskLevel.Low, "Portfolio total value is zero.");
            }

            decimal weightedRiskScore = 0;
            var analysisDetails = new List<string>();

            foreach (var investment in portfolio.Investments)
            {
                string assetType = investment.Asset?.AssetType ?? "Unknown";
                if (AssetBaseRiskScores.TryGetValue(assetType, out decimal baseRisk))
                {
                    decimal assetCurrentPrice = investment.Asset?.CurrentPrice ?? (decimal)investment.PurchasePrice;
                    decimal investmentValue = assetCurrentPrice * investment.Quantity;
                    decimal weight = totalPortfolioValue > 0 ? (investmentValue / totalPortfolioValue) : 0M;
                    weightedRiskScore += weight * baseRisk;
                    analysisDetails.Add($"{investment.AssetName} ({assetType}): Weight {weight:P1}, Base Risk {baseRisk}, Contribution: {(weight * baseRisk):F2}");
                }
                else
                {
                    // Handle unknown asset types - assign a default medium risk and log a warning
                    Console.WriteLine($"Warning: Unknown asset type '{assetType}' for asset ID {investment.AssetId}. Assigning default risk.");
                    decimal assetCurrentPrice = investment.Asset?.CurrentPrice ?? (decimal)investment.PurchasePrice;
                    decimal investmentValue = assetCurrentPrice * investment.Quantity;
                    decimal weight = totalPortfolioValue > 0 ? (investmentValue / totalPortfolioValue) : 0M;
                    decimal defaultUnknownRisk = 60M; // Default to medium-high for unknown
                    weightedRiskScore += weight * defaultUnknownRisk;
                    analysisDetails.Add($"{investment.AssetName} (Unknown Type): Weight {weight:P1}, Default Risk {defaultUnknownRisk}, Contribution: {(weight * defaultUnknownRisk):F2}");
                }
            }

            RiskLevel riskLevel = GetPortfolioRiskLevelFromScore(weightedRiskScore);
            string finalAnalysisDetails = $"Calculated based on {portfolio.Investments.Count} investments. Total Portfolio Value: {totalPortfolioValue:C}. Details:\n" + string.Join("\n", analysisDetails);

            return await CreateOrUpdatePortfolioRiskRecord(portfolioId, weightedRiskScore, riskLevel, finalAnalysisDetails);
        }

        /// <summary>
        /// Retrieves the latest risk analysis for a specific portfolio.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <returns>The latest PortfolioRiskAnalysis object, or null if not found.</returns>
        public async Task<PortfolioRiskAnalysis?> GetPortfolioRiskAnalysis(Guid portfolioId)
        {
            return await _context.PortfolioRiskAnalyses
                .Where(r => r.PortfolioId == portfolioId)
                .OrderByDescending(r => r.AnalysisDate)
                .FirstOrDefaultAsync(); // Get the latest risk analysis
        }

        /// <summary>
        /// Retrieves the risk history for a specific portfolio over a given number of days.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <param name="days">Number of days for history (default: 30).</param>
        /// <returns>A list of PortfolioRiskAnalysis objects.</returns>
        public async Task<List<PortfolioRiskAnalysis>> GetPortfolioRiskHistory(Guid portfolioId, int days = 30)
        {
            var historyDate = DateTime.UtcNow.AddDays(-days); // Use UtcNow for consistency
            return await _context.PortfolioRiskAnalyses
                .Where(r => r.PortfolioId == portfolioId && r.AnalysisDate >= historyDate)
                .OrderBy(r => r.AnalysisDate)
                .ToListAsync();
        }

        /// <summary>
        /// Maps a calculated numerical risk score to an enum RiskLevel.
        /// </summary>
        private RiskLevel GetPortfolioRiskLevelFromScore(decimal score)
        {
            if (score <= LOW_RISK_THRESHOLD)
            {
                return RiskLevel.Low;
            }
            else if (score <= MEDIUM_RISK_THRESHOLD)
            {
                return RiskLevel.Medium;
            }
            else
            {
                return RiskLevel.High;
            }
        }

        /// <summary>
        /// Creates a new PortfolioRiskAnalysis record or updates an existing one for the current day.
        /// </summary>
        private async Task<PortfolioRiskAnalysis> CreateOrUpdatePortfolioRiskRecord(Guid portfolioId, decimal riskScore, RiskLevel riskLevel, string analysisDetails)
        {
            // Check if a risk record for this portfolio exists for today (UTC date)
            var existingRisk = await _context.PortfolioRiskAnalyses
                .FirstOrDefaultAsync(r => r.PortfolioId == portfolioId &&
                                          r.AnalysisDate.Date == DateTime.UtcNow.Date);

            if (existingRisk != null)
            {
                existingRisk.RiskScore = riskScore;
                existingRisk.RiskLevel = riskLevel;
                existingRisk.AnalysisDetails = analysisDetails;
                existingRisk.AnalysisDate = DateTime.UtcNow; // Update timestamp
                _context.PortfolioRiskAnalyses.Update(existingRisk);
            }
            else
            {
                var newRisk = new PortfolioRiskAnalysis
                {
                    PortfolioRiskAnalysisId = Guid.NewGuid(),
                    PortfolioId = portfolioId,
                    RiskScore = riskScore,
                    RiskLevel = riskLevel,
                    AnalysisDetails = analysisDetails,
                    AnalysisDate = DateTime.UtcNow
                };
                await _context.PortfolioRiskAnalyses.AddAsync(newRisk);
            }

            await _context.SaveChangesAsync();
            return existingRisk ?? (await _context.PortfolioRiskAnalyses.FirstOrDefaultAsync(r => r.PortfolioId == portfolioId && r.AnalysisDate.Date == DateTime.UtcNow.Date))!;
        }

        // --- User Risk Profile Methods ---

        /// <summary>
        /// Creates or updates a user's personal risk profile.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="riskLevel">The user's declared/assessed risk level.</param>
        /// <param name="description">A description for the risk profile.</param>
        /// <returns>The created or updated RiskProfile record.</returns>
        public async Task<RiskProfile> CreateOrUpdateUserRiskProfile(Guid userId, RiskLevel riskLevel, string description)
        {
            var existingProfile = await _context.RiskProfiles
                .FirstOrDefaultAsync(rp => rp.UserId == userId);

            if (existingProfile != null)
            {
                existingProfile.RiskLevel = riskLevel;
                existingProfile.Description = description;
                existingProfile.AssessedOn = DateTime.UtcNow;
                _context.RiskProfiles.Update(existingProfile);
            }
            else
            {
                var newProfile = new RiskProfile
                {
                    RiskProfileId = Guid.NewGuid(),
                    UserId = userId,
                    RiskLevel = riskLevel,
                    Description = description,
                    AssessedOn = DateTime.UtcNow
                };
                await _context.RiskProfiles.AddAsync(newProfile);
            }
            await _context.SaveChangesAsync();
            return existingProfile ?? (await _context.RiskProfiles.FirstOrDefaultAsync(rp => rp.UserId == userId))!;
        }

        /// <summary>
        /// Retrieves a user's personal risk profile.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The user's RiskProfile, or null if not found.</returns>
        public async Task<RiskProfile?> GetUserRiskProfile(Guid userId)
        {
            return await _context.RiskProfiles
                .FirstOrDefaultAsync(rp => rp.UserId == userId);
        }
    }
}