
using InvestmentPortfolioManagement.Data;
using System.Text;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Hosting;
using InvestmentPortfolioManagement.Interfaces;

namespace InvestmentPortfolioManagement.Services
{
    public class ReportService :IReportService
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

        public string GenerateAssetCsv(Guid userId)
        {
            throw new NotImplementedException();
        }

        public byte[] GenerateCsv(Guid portfolioId)
        {
            var assets = _context.Assets.Where(a => a.PortfolioId == portfolioId).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("Name,Type,Quantity,BasePrice,CurrentPrice,TotalValue");
            foreach (var a in assets)
            {
                sb.AppendLine($"\"{a.Name}\",\"{a.AssetType}\",\"{a.Quantity}\",\"{a.BasePrice}\",\"{a.CurrentPrice}\",\"{a.TotalValue}\"");
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public byte[] GeneratePdf(Guid portfolioId)
        {
            var portfolio = _context.Portfolios.FirstOrDefault(p => p.PortfolioId == portfolioId);
            var assets = _context.Assets.Where(a => a.PortfolioId == portfolioId).ToList();

            var html = new StringBuilder();
            html.Append($"<h2>Portfolio Report - {portfolio?.Name}</h2>");
            html.Append($"<p><strong>Type:</strong> {portfolio?.Type}</p>");
            html.Append($"<p><strong>Total Value:</strong> ₹{portfolio?.TotalValue}</p>");
            html.Append("<table border='1' cellpadding='5'><thead><tr><th>Name</th><th>Type</th><th>Qty</th><th>Base</th><th>Current</th><th>Total</th></tr></thead><tbody>");

            foreach (var a in assets)
            {
                html.Append($"<tr><td>{a.Name}</td><td>{a.AssetType}</td><td>{a.Quantity}</td><td>{a.BasePrice}</td><td>{a.CurrentPrice}</td><td>{a.TotalValue}</td></tr>");
            }

            html.Append("</tbody></table>");

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = { PaperSize = PaperKind.A4 },
                Objects = { new ObjectSettings { HtmlContent = html.ToString() } }
            };

            return _pdfConverter.Convert(doc);
        }

        public byte[] GeneratePortfolioPdf(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
