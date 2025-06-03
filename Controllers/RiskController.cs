using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize]
    public class RiskController : Controller
    {
        private readonly IRiskService _riskService;

        public RiskController(IRiskService riskService)
        {
            _riskService = riskService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;

        }

        public async Task<IActionResult> Index()
        {
            Guid userId = GetCurrentUserId();
            var profile = await _riskService.GetRiskByUserIdAsync(userId);
            return View(profile);
        }

        public async Task<IActionResult> Assess()
        {
            var userId = GetCurrentUserId();
            var profile = await _riskService.GetRiskByUserIdAsync(userId) ?? new RiskProfile { UserId = userId };
            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Assess(RiskProfile model)
        {
            if (ModelState.IsValid)
            {
                await _riskService.AssessRiskAsync(model);
                TempData["Success"] = "Risk profile updated.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}
