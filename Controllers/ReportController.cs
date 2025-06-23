using InvestmentPortfolioManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Text;

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





        [HttpGet]
        public async Task<IActionResult> DownloadPortfolioReportPdf(Guid id)
        {
            try
            {
                // Optional: Add authorization check to ensure user owns this portfolio

                var pdfBytes = await _reportService.GeneratePortfolioPdfReport(id);
                return File(pdfBytes, "application/pdf", $"Portfolio_Report_{id}.pdf");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPortfolioReportCsv(Guid id)
        {
            try
            {
                // Optional: Add authorization check

                var csvString = await _reportService.GeneratePortfolioCsvReport(id);
                var fileName = $"Portfolio_Report_{id}.csv";
                return File(Encoding.UTF8.GetBytes(csvString), "text/csv", fileName);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}