using InvestmentPortfolioManagement.Data;
using System.Text;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Hosting;
using InvestmentPortfolioManagement.Interfaces;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Required for Include and ThenInclude
using InvestmentPortfolioManagement.Models; // Required for Portfolio, Investment, Asset

namespace InvestmentPortfolioManagement.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConverter _pdfConverter;
        private readonly IWebHostEnvironment _env;

        public ReportService(ApplicationDbContext context, IConverter pdfConverter, IWebHostEnvironment env)
        {
            _context = context;
            _pdfConverter = pdfConverter;
            _env = env;
        }

        // Helper to calculate P/L for an investment
        private decimal CalculateProfitLoss(Investment investment)
        {
            if (investment.Asset == null) return 0.0M;
            // The error "Operator '-' cannot be applied to operands of type 'decimal' and 'double'"
            // indicates a type mismatch. To fix this, explicitly cast both CurrentPrice and
            // PurchasePrice to decimal before performing the subtraction. This ensures that
            // the operation is performed between two decimal values, maintaining precision.
            // Also, ensure Quantity is treated as decimal for the multiplication.
            return ((decimal)investment.Asset.CurrentPrice - (decimal)investment.PurchasePrice) * (decimal)investment.Quantity;
        }

        // Helper to determine risk level based on portfolio type (simple placeholder logic)
        //private string DetermineRiskLevel(string portfolioType)
        //{
        //    return portfolioType switch
        //    {
        //        "High Growth" => "High",
        //        "Aggressive" => "High",
        //        "Moderate" => "Medium",
        //        "Balanced" => "Medium",
        //        "Conservative" => "Low",
        //        _ => "Undefined",
        //    };
        //}

        // Renamed and updated: Generate PDF report for a specific portfolio
        public byte[] GeneratePortfolioPdf(Guid portfolioId)
        {
            // Load portfolio with all related investments and their assets
            var portfolio = _context.Portfolios
                                    .Include(p => p.Investments)
                                        .ThenInclude(i => i.Asset)
                                    .FirstOrDefault(p => p.PortfolioId == portfolioId);

            if (portfolio == null)
            {
                // Handle case where portfolio is not found
                return Encoding.UTF8.GetBytes("<html><body><h2>Error: Portfolio not found.</h2></body></html>");
            }

            var html = new StringBuilder();
            html.Append("<!DOCTYPE html>");
            html.Append("<html><head>");
            html.Append("<style>");
            html.Append("body { font-family: 'Roboto', sans-serif; margin: 20px; color: #333; }");
            html.Append("h2 { color: #0056b3; border-bottom: 2px solid #eee; padding-bottom: 10px; margin-bottom: 20px; }");
            html.Append("h3 { color: #0056b3; margin-top: 30px; border-bottom: 1px solid #eee; padding-bottom: 5px; }");
            html.Append("p { margin-bottom: 5px; line-height: 1.5; }");
            html.Append("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            html.Append("th, td { border: 1px solid #ddd; padding: 10px; text-align: left; }");
            html.Append("th { background-color: #f2f2f2; color: #555; }");
            html.Append(".profit { color: #28a745; font-weight: bold; }"); // Green for profit
            html.Append(".loss { color: #dc3545; font-weight: bold; }");   // Red for loss
            html.Append("</style>");
            html.Append("</head><body>");

            html.Append($"<h2>Portfolio Report: {portfolio.Name}</h2>");
            html.Append($"<p><strong>Description:</strong> {portfolio.Description}</p>");
            //html.Append($"<p><strong>Type:</strong> {portfolio.Type}</p>");
            html.Append($"<p><strong>Total Value:</strong> ₹{portfolio.TotalValue:N2}</p>");
            html.Append($"<p><strong>Generated Date:</strong> {DateTime.Now.ToShortDateString()}</p>");

            // Add Risk Level
            //var riskLevel = DetermineRiskLevel(portfolio.Type);
            //html.Append($"<p><strong>Estimated Risk Level:</strong> {riskLevel}</p>");


            html.Append("<h3>Investments</h3>");
            if (portfolio.Investments != null && portfolio.Investments.Any())
            {
                html.Append("<table><thead><tr><th>Asset Name</th><th>Type</th><th>Quantity</th><th>Purchase Price</th><th>Current Price</th><th>P/L</th></tr></thead><tbody>");

                foreach (var inv in portfolio.Investments)
                {
                    // Ensure Asset is loaded for the investment
                    if (inv.Asset != null)
                    {
                        var pl = CalculateProfitLoss(inv);
                        var plClass = pl >= 0 ? "profit" : "loss"; // Apply class based on P/L
                        html.Append($"<tr>");
                        html.Append($"<td>{inv.Asset.Name}</td>");
                        html.Append($"<td>{inv.Asset.AssetType}</td>");
                        html.Append($"<td>{inv.Quantity}</td>");
                        html.Append($"<td>₹{inv.PurchasePrice:N2}</td>");
                        html.Append($"<td>₹{inv.Asset.CurrentPrice:N2}</td>");
                        html.Append($"<td class='{plClass}'>₹{pl:N2}</td>");
                        html.Append($"</tr>");
                    }
                }
                html.Append("</tbody></table>");
            }
            else
            {
                html.Append("<p>No investments found in this portfolio.</p>");
            }

            html.Append("</body></html>");

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = { PaperSize = PaperKind.A4, Orientation = Orientation.Portrait },
                Objects = { new ObjectSettings { HtmlContent = html.ToString() } }
            };

            return _pdfConverter.Convert(doc);
        }

        // Renamed and updated: Generate CSV report for a specific portfolio's assets/investments
        public byte[] GenerateAssetCsv(Guid portfolioId)
        {
            var portfolio = _context.Portfolios
                                    .Include(p => p.Investments)
                                        .ThenInclude(i => i.Asset)
                                    .FirstOrDefault(p => p.PortfolioId == portfolioId);

            if (portfolio == null)
            {
                // Return an empty CSV or an error message in CSV format
                return Encoding.UTF8.GetBytes("Error: Portfolio not found.");
            }

            var sb = new StringBuilder();
            // Header row with new columns for P/L and Risk Level
            sb.AppendLine("Portfolio Name,Portfolio Type,Portfolio Total Value,Estimated Risk Level,Asset Name,Asset Type,Quantity,Purchase Price,Current Price,P/L");

            //var riskLevel = DetermineRiskLevel(portfolio.Type);

            if (portfolio.Investments != null && portfolio.Investments.Any())
            {
                foreach (var inv in portfolio.Investments)
                {
                    if (inv.Asset != null)
                    {
                        var pl = CalculateProfitLoss(inv);
                        sb.AppendLine($"\"{portfolio.Name}\",\"{portfolio.TotalValue:N2}\",\"{inv.Asset.Name}\",\"{inv.Asset.AssetType}\",\"{inv.Quantity}\",\"{inv.PurchasePrice:N2}\",\"{inv.Asset.CurrentPrice:N2}\",\"{pl:N2}\"");
                    }
                }
            }
            else
            {
                sb.AppendLine($"\"{portfolio.Name}\",\"{portfolio.TotalValue:N2}\",No investments,N/A,0,0,0,0");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
