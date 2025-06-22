using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Models;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace InvestmentPortfolioManagement.Services
{
    public class LivePriceUpdaterService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random = new();

        // New properties for Market Trend simulation
        private double _marketTrend = 0; // -1 (Bearish) to +1 (Bullish)
        private DateTime _nextTrendChange = DateTime.UtcNow;

        public LivePriceUpdaterService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // --- 1. UPDATE THE OVERALL MARKET TREND ---
                // The market trend will change periodically (e.g., every 1-5 minutes).
                if (DateTime.UtcNow >= _nextTrendChange)
                {
                    // Generate a new trend: -0.5 (slight bear) to +0.5 (slight bull) is a good range
                    _marketTrend = (_random.NextDouble() * 1.0) - 0.5;
                    // Set the next time the trend will change
                    _nextTrendChange = DateTime.UtcNow.AddSeconds(_random.Next(10, 20));
                    Console.WriteLine($"--- Market Trend updated to: {_marketTrend:F2} ---");
                }


                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var assetService = scope.ServiceProvider.GetRequiredService<IAssetService>();

                    // We need to fetch the assets fresh each time to get their current state
                    var assetsToUpdate = await context.Assets.ToListAsync(stoppingToken);
                    // Create a dictionary to track momentum across iterations if not using a [NotMapped] property.
                    // For simplicity, we'll assume the [NotMapped] property exists on the Asset model.

                    foreach (var asset in assetsToUpdate)
                    {
                        // --- 2. CALCULATE RISK-BASED VOLATILITY (Same as before) ---
                        decimal riskScore = RiskConstants.AssetBaseRiskScores.GetValueOrDefault(asset.AssetType ?? "", 50M);
                        double maxVolatilityPercent;
                        if (riskScore > 75) maxVolatilityPercent = 2.5;      // High Risk
                        else if (riskScore > 40) maxVolatilityPercent = 1.0; // Medium Risk
                        else maxVolatilityPercent = 0.25;                    // Low Risk

                        // --- 3. APPLY MARKET TREND AND MOMENTUM ---
                        // A. Start with the base market trend
                        double trendFactor = _marketTrend;

                        // B. Add the asset's personal momentum.
                        // The 'Momentum' property is on the Asset model but not saved to the DB.
                        // It "remembers" the last direction of movement.
                        trendFactor += asset.Momentum;

                        // C. Generate the random component (the "noise")
                        var randomIntMax = (int)(maxVolatilityPercent * 100);
                        double randomComponent = _random.Next(-randomIntMax, randomIntMax + 1) / 100.0;

                        // D. Combine them: The final change is a mix of trend, momentum, and randomness
                        // We give more weight to randomness to keep it unpredictable.
                        var percentChange = (trendFactor * 0.25) + (randomComponent * 0.75);


                        // --- 4. CALCULATE NEW PRICE (Same as before) ---
                        var changeFactor = 1 + (percentChange / 100.0);
                        var priceToChange = (asset.CurrentPrice > 0) ? asset.CurrentPrice : asset.BasePrice;
                        var newPrice = Math.Round(priceToChange * (decimal)changeFactor, 2);

                        // Prevent price from going to zero
                        if (newPrice < asset.BasePrice * 0.01M) { newPrice = asset.BasePrice * 0.01M; }


                        // --- 5. UPDATE MOMENTUM FOR THE NEXT ITERATION ---
                        // If price went up, momentum becomes positive. If down, negative.
                        // We decay the old momentum so it doesn't get stuck.
                        asset.Momentum = (asset.Momentum * 0.5) + (percentChange > 0 ? 0.1 : -0.1);
                        // Clamp momentum to a reasonable range to prevent runaway feedback loops
                        asset.Momentum = Math.Clamp(asset.Momentum, -0.5, 0.5);


                        // --- 6. UPDATE THE PRICE IN THE DATABASE ---
                        try
                        {
                            await assetService.UpdateAssetCurrentPriceAsync(asset.AssetId, newPrice);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error updating price for asset {asset.Name} (ID: {asset.AssetId}): {ex.Message}");
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}