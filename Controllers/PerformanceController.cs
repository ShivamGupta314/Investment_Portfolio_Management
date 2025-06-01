using InvestmentPortfolioManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize]
    public class PerformanceController : Controller
    {
        private readonly IPerformanceService _performanceService;

        public PerformanceController(IPerformanceService performanceService)
        {
            _performanceService = performanceService;
        }

        // GET: /Performance/Portfolio/{portfolioId}
        public async Task<IActionResult> Portfolio(int portfolioId)
        {
            var result = await _performanceService.CalculatePerformanceAsync(portfolioId);
            return View(result);
        }

        public async Task<IActionResult> History(int portfolioId)
        {
            var history = await _context.Performances
                .Where(p => p.PortfolioId == portfolioId)
                .OrderBy(p => p.CalculatedOn)
                .ToListAsync();

            ViewBag.PortfolioId = portfolioId;
            return View(history);
        }

    }
}
