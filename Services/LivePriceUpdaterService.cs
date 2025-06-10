
using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Models;
using Microsoft.Extensions.DependencyInjection;

namespace InvestmentPortfolioManagement.Services
{
    public class LivePriceUpdaterService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random = new();

        public LivePriceUpdaterService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var adminAssets = context.Assets
                                             .Where(a => a.PortfolioId == null)
                                             .ToList();

                    foreach (var asset in adminAssets)
                    {
                        // Calculate new price
                        var percentChange = _random.Next(-20, 21); // -20% to +20%
                        var changeFactor = 1 + (percentChange / 100.0);
                        var newPrice = Math.Round(asset.BasePrice * (decimal)changeFactor, 2);

                        // Update asset price
                        asset.CurrentPrice = newPrice;

                        // Update investments tied to this asset
                        var relatedInvestments = context.Investments
                                                        .Where(inv => inv.AssetId == asset.AssetId)
                                                        .ToList();

                        foreach (var investment in relatedInvestments)
                        {
                            investment.CurrentPrice = (double)newPrice;
                        }
                    }

                    await context.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Update every 10 seconds
            }
        }

    }
}
