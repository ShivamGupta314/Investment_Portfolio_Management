// Controllers/RiskController.cs
using InvestmentPortfolioManagement.Services;
using InvestmentPortfolioManagement.Models;
using InvestmentPortfolioManagement.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Security.Claims; // For getting UserId from claims if authenticated
using Microsoft.AspNetCore.Authorization;
using InvestmentPortfolioManagement.Interfaces;
using System.ComponentModel.DataAnnotations; // If you have authentication/authorization

namespace InvestmentPortfolioManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Apply authorization at controller level if all actions require it
    public class RiskController : ControllerBase
    {
        private readonly IRiskService _riskService;

        public RiskController(IRiskService riskService)
        {
            _riskService = riskService;
        }

        // --- Portfolio Risk Analysis Endpoints ---

        /// <summary>
        /// Calculates the risk for a specific portfolio and stores it.
        /// Requires the user to own the portfolio or be an Admin.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <returns>The calculated PortfolioRiskAnalysis object.</returns>
        [HttpPost("calculatePortfolioRisk/{portfolioId}")]
        // [Authorize(Roles = "Investor")] // Or roles that can calculate risk
        public async Task<IActionResult> CalculatePortfolioRisk(Guid portfolioId)
        {
            // Optional: Implement authorization check here
            // Guid currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            // You would need to fetch the portfolio and check its UserId against currentUserId

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
                // Log the exception (e.g., using ILogger)
                return StatusCode(500, new { message = "An error occurred while calculating portfolio risk.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves the latest risk analysis for a specific portfolio.
        /// Requires the user to own the portfolio or be an Admin.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <returns>The latest PortfolioRiskAnalysis object.</returns>
        [HttpGet("viewPortfolioRiskAnalysis/{portfolioId}")]
        // [Authorize(Roles = "Investor")]
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
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while retrieving portfolio risk analysis.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves the risk history for a specific portfolio over a given number of days.
        /// Requires the user to own the portfolio or be an Admin.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <param name="days">Number of days for history (default: 30).</param>
        /// <returns>A list of PortfolioRiskAnalysis objects.</returns>
        [HttpGet("viewPortfolioRiskHistory/{portfolioId}")]
        // [Authorize(Roles = "Investor")]
        public async Task<IActionResult> ViewPortfolioRiskHistory(Guid portfolioId, [FromQuery] int days = 30)
        {
            try
            {
                var riskHistory = await _riskService.GetPortfolioRiskHistory(portfolioId, days);
                if (!riskHistory.Any())
                {
                    return Ok(new { message = $"No risk history found for portfolio ID {portfolioId} in the last {days} days." });
                }
                return Ok(riskHistory);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while retrieving portfolio risk history.", error = ex.Message });
            }
        }

        // --- User Risk Profile Endpoints ---

        /// <summary>
        /// Sets or updates the authenticated user's personal risk profile.
        /// </summary>
        /// <param name="model">The RiskProfile details (RiskLevel and Description).</param>
        /// <returns>The created or updated RiskProfile object.</returns>
        [HttpPost("setUserRiskProfile")]
        [Authorize(Roles = "Investor")] // Only authenticated investors can set their profile
        public async Task<IActionResult> SetUserRiskProfile([FromBody] UserRiskProfileCreateModel model)
        {
            // Ensure a user is authenticated
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User not authenticated." });
            }
            Guid userId = Guid.Parse(userIdClaim.Value);

            try
            {
                var riskProfile = await _riskService.CreateOrUpdateUserRiskProfile(userId, model.RiskLevel, model.Description);
                return Ok(riskProfile);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while setting user risk profile.", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves the authenticated user's personal risk profile.
        /// </summary>
        /// <returns>The user's RiskProfile object.</returns>
        [HttpGet("getUserRiskProfile")]
        [Authorize(Roles = "Investor")] // Only authenticated investors can view their profile
        public async Task<IActionResult> GetUserRiskProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User not authenticated." });
            }
            Guid userId = Guid.Parse(userIdClaim.Value);

            try
            {
                var riskProfile = await _riskService.GetUserRiskProfile(userId);
                if (riskProfile == null)
                {
                    return NotFound(new { message = "No risk profile found for this user." });
                }
                return Ok(riskProfile);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while retrieving user risk profile.", error = ex.Message });
            }
        }
    }

    // DTO for setting user risk profile to only accept what's needed from client
    public class UserRiskProfileCreateModel
    {
        [Required]
        public RiskLevel RiskLevel { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}