
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
                    var adminAssets = context.Assets.Where(a => a.PortfolioId == null).ToList();

                    foreach (var asset in adminAssets)
                    {
                        var percentChange = _random.Next(-20, 21); // -20% to +20%
                        var changeFactor = 1 + (percentChange / 100.0);
                        asset.CurrentPrice = Math.Round(asset.BasePrice * (decimal)changeFactor, 2);
                    }

                    await context.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Update every 5 minutes
            }
        }
    }
}
