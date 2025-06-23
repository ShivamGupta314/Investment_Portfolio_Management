using InvestmentPortfolioManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService; // Keeping the service for potential future general reports

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
        }

        // The Index action can remain if you intend to use this controller
        // for a general reports dashboard or other future reporting features.
        public IActionResult Index()
        {
            return View();
        }

        // The ExportPdf and ExportCsv methods have been removed from here
        // as portfolio-specific report generation is now handled in PortfolioController.
        // If you need to generate "all user portfolios" reports, you would
        // modify IReportService to accept userId again, or loop through portfolios
        // in a new dedicated method here.
    }
}