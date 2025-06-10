
using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize(Roles = "Investor")]
    public class InvestorController : BaseController
    {
        private readonly ApplicationDbContext _context;

        private readonly IPerformanceService _performanceService;

        private readonly IRiskService _riskService;

        private readonly IReportService _reportService;

        public InvestorController(ApplicationDbContext context, IPerformanceService performanceService, IRiskService riskService, IReportService reportService)
        {
            _context = context;
            _performanceService = performanceService;
            _riskService = riskService;
            _reportService = reportService;
        }

        public async Task<IActionResult> Portfolio()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var portfolios = await _context.Portfolios
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return View(portfolios);
        }

        [HttpGet]
        public IActionResult CreatePortfolio()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePortfolio(Portfolio model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.PortfolioId = Guid.NewGuid();
            model.UserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            model.CreatedDate = DateTime.Now;

            _context.Portfolios.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Portfolio");
        }

        public async Task<IActionResult> DeletePortfolio(Guid id)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio != null)
            {
                _context.Portfolios.Remove(portfolio);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Portfolio");
        }

        public IActionResult Performance(Guid portfolioId)
        {
            var allocation = _performanceService.GetAllocationByType(portfolioId);
            var trend = _performanceService.GetGainLossTrend(portfolioId);

            ViewBag.Allocation = allocation;
            ViewBag.Trend = trend;

            return View();
        }


        public IActionResult Risk(Guid portfolioId)
        {
            var level = _riskService.GetRiskLevel(portfolioId);
            ViewBag.RiskLevel = level;
            return View();
        }

        //public IActionResult ExportPdf(Guid portfolioId)
        //{
        //    var pdfBytes = _reportService.GeneratePdf(portfolioId);
        //    return File(pdfBytes, "application/pdf", $"Portfolio_{portfolioId}.pdf");
        //}

        //public IActionResult ExportCsv(Guid portfolioId)
        //{
        //    var csvBytes = _reportService.GenerateCsv(portfolioId);
        //    return File(csvBytes, "text/csv", $"Portfolio_{portfolioId}.csv");
        //}

    }
}
