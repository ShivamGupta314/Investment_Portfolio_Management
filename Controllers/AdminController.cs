using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioManagement.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var data = await _context.Performances
                .Include(p => p.Portfolio)
                .ThenInclude(p => p.User)
                .GroupBy(p => new { p.Portfolio.User.Username, p.Portfolio.User.Email })
                .Select(g => new
                {
                    Username = g.Key.Username,
                    Email = g.Key.Email,
                    TotalInvestment = g.Sum(x => x.TotalInvestment),
                    TotalValue = g.Sum(x => x.CurrentValue),
                    ProfitOrLoss = g.Sum(x => x.CurrentValue - x.TotalInvestment)
                })
                .ToListAsync();

            return View(data);
        }
    }
}
