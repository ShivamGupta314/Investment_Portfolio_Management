
using InvestmentPortfolioManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var totalUsers = _context.Users.Count();
            var totalPortfolios = _context.Portfolios.Count();
            var totalAssets = _context.Assets.Count();
            var avgPortfolioValue = _context.Portfolios.Any()
                ? _context.Portfolios.Average(p => p.TotalValue)
                : 0;

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalPortfolios = totalPortfolios;
            ViewBag.TotalAssets = totalAssets;
            ViewBag.AvgPortfolioValue = avgPortfolioValue;

            return View();
        }
    }
}
