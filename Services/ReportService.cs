using DinkToPdf;
using DinkToPdf.Contracts;
using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using System.Text;

namespace InvestmentPortfolioManagement.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConverter _pdfConverter;

        public ReportService(ApplicationDbContext context, IConverter pdfConverter)
        {
            _context = context;
            _pdfConverter = pdfConverter;
        }

        public byte[] GeneratePortfolioPdf(Guid userId)
        {
            var portfolios = _context.Portfolios.Where(p => p.UserId == userId).ToList();

            var html = new StringBuilder();
            html.Append("<h1>Portfolio Summary</h1><table border='1'><tr><th>Name</th><th>Type</th><th>Value</th></tr>");
            foreach (var p in portfolios)
            {
                html.Append($"<tr><td>{p.PortfolioName}</td><td>{p.Type}</td><td>{p.TotalValue}</td></tr>");
            }
            html.Append("</table>");

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = { PaperSize = PaperKind.A4 },
                Objects = { new ObjectSettings { HtmlContent = html.ToString() } }
            };

            return _pdfConverter.Convert(doc);
        }

        public string GenerateAssetCsv(Guid userId)
        {
            var assets = _context.Assets.Where(a => a.Portfolio.UserId == userId).ToList();

            var csv = new StringBuilder();
            csv.AppendLine("AssetName,Type,Quantity,PurchasePrice,Portfolio");

            foreach (var a in assets)
            {
                csv.AppendLine($"{a.AssetName},{a.AssetType},{a.Quantity},{a.PurchasePrice},{a.Portfolio.PortfolioName}");
            }

            return csv.ToString();
        }
    }
}
