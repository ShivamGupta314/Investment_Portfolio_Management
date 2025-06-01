using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize] // Requires JWT token
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
