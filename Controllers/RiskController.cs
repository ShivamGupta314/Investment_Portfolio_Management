using InvestmentPortfolioManagement.Services;
using InvestmentPortfolioManagement.Models;
using InvestmentPortfolioManagement.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using InvestmentPortfolioManagement.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize]
    [ApiController] // Add this for better API conventions
    [Route("api/[controller]")]
    public class RiskController : ControllerBase
    {
        private readonly IRiskService _riskService;

        public RiskController(IRiskService riskService)
        {
            _riskService = riskService;
        }

        // --- Portfolio Risk Analysis Endpoints ---

        [HttpPost("calculatePortfolioRisk/{portfolioId}")]
        public async Task<IActionResult> CalculatePortfolioRisk(Guid portfolioId)
        {
            try
            {
                var riskAnalysis = await _riskService.CalculateAndStorePortfolioRisk(portfolioId);
                return Ok(riskAnalysis);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while calculating portfolio risk.", error = ex.Message });
            }
        }

        [HttpGet("viewPortfolioRiskAnalysis/{portfolioId}")]
        public async Task<IActionResult> ViewPortfolioRiskAnalysis(Guid portfolioId)
        {
            try
            {
                var riskAnalysis = await _riskService.GetPortfolioRiskAnalysis(portfolioId);
                if (riskAnalysis == null)
                {
                    return NotFound(new { message = $"No risk analysis found for portfolio ID {portfolioId}." });
                }
                return Ok(riskAnalysis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving portfolio risk analysis.", error = ex.Message });
            }
        }

        // --- NEW ENDPOINT FOR LIVE CHART ---
        /// <summary>
        /// Retrieves RECENT risk history for a specific portfolio over a given number of minutes.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <param name="minutes">Number of minutes for history (default: 5).</param>
        [HttpGet("viewRecentPortfolioRiskHistory/{portfolioId}")]
        public async Task<IActionResult> ViewRecentPortfolioRiskHistory(Guid portfolioId, [FromQuery] int minutes = 5)
        {
            try
            {
                var historyTime = DateTime.UtcNow.AddMinutes(-minutes);
                var riskHistory = await _riskService.GetPortfolioRiskHistory(portfolioId, historyTime);
                return Ok(riskHistory); // It's fine to return an empty list
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving recent portfolio risk history.", error = ex.Message });
            }
        }

        // This endpoint remains for fetching long-term history if needed elsewhere.
        [HttpGet("viewPortfolioRiskHistory/{portfolioId}")]
        public async Task<IActionResult> ViewPortfolioRiskHistory(Guid portfolioId, [FromQuery] int days = 30)
        {
            // ... (your existing code is fine) ...
            var riskHistory = await _riskService.GetPortfolioRiskHistory(portfolioId, days);
            if (!riskHistory.Any()) { /* return message */ }
            return Ok(riskHistory);
        }

        // --- User Risk Profile Endpoints (No changes needed) ---
        [HttpPost("setUserRiskProfile")]
        public async Task<IActionResult> SetUserRiskProfile([FromBody] UserRiskProfileCreateModel model) { /* ... */ return Ok(); }

        [HttpGet("getUserRiskProfile")]
        public async Task<IActionResult> GetUserRiskProfile() { /* ... */ return Ok(); }
    }

    public class UserRiskProfileCreateModel { /* ... */ }
}