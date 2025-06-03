using InvestmentPortfolioManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ExportPdf()
        {
            Guid userId = GetCurrentUserId();
            var file = _reportService.GeneratePortfolioPdf(userId);
            return File(file, "application/pdf", "PortfolioReport.pdf");
        }

        public IActionResult ExportCsv()
        {
            var userId = GetCurrentUserId();
            var csv = _reportService.GenerateAssetCsv(userId);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", "Assets.csv");
        }
    }
}
