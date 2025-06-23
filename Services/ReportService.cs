using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// --- iText 7 using statements ---
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace InvestmentPortfolioManagement.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<Portfolio> GetPortfolioData(Guid portfolioId)
        {
            var portfolio = await _context.Portfolios
                .Include(p => p.Investments)
                    .ThenInclude(i => i.Asset)
                .FirstOrDefaultAsync(p => p.PortfolioId == portfolioId);

            if (portfolio == null)
            {
                throw new ArgumentException("Portfolio not found.");
            }
            return portfolio;
        }

        // --- CSV Generation (No changes needed) ---
        // --- CSV Generation (No changes needed) ---
        public async Task<string> GeneratePortfolioCsvReport(Guid portfolioId)
        {
            var portfolio = await GetPortfolioData(portfolioId);
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("AssetName,AssetType,Quantity,PurchasePrice,CurrentPrice,TotalValue");
            foreach (var investment in portfolio.Investments)
            {
                csvBuilder.AppendLine($"{investment.Asset?.Name ?? "N/A"},{investment.Asset?.AssetType ?? "N/A"},{investment.Quantity},{investment.PurchasePrice},{(investment.Asset?.CurrentPrice ?? 0M)},{((investment.Asset?.CurrentPrice ?? 0M) * investment.Quantity)}");
            }
            return csvBuilder.ToString();
        }


        // --- CORRECTED PDF Generation using iText 7 ---
        public async Task<byte[]> GeneratePortfolioPdfReport(Guid portfolioId)
        {
            var portfolio = await GetPortfolioData(portfolioId);

            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // --- FONTS (with new bold font) ---
                var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var bodyFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var bodyBoldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD); // <-- Create a specific bold font

                // --- DOCUMENT CONTENT ---

                // 1. Title
                document.Add(new Paragraph($"Portfolio Report: {portfolio.Name}")
                    .SetFont(titleFont)
                    .SetFontSize(18)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20));

                // 2. Summary Information
                document.Add(new Paragraph($"Report Generated: {DateTime.Now:g}").SetFont(bodyFont));

                // === THE FIX IS HERE ===
                // Create a Paragraph and add two separate Text objects with different fonts
                var totalValueParagraph = new Paragraph()
                    .Add(new Text("Total Value: ").SetFont(bodyFont)) // Normal text
                    .Add(new Text(portfolio.TotalValue.ToString("C")).SetFont(bodyBoldFont)); // Bold text
                document.Add(totalValueParagraph);

                document.Add(new Paragraph("\n")); // Add a line break

                // 3. Table of Investments
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2, 1, 2, 2, 2 }))
                    .UseAllAvailableWidth();

                // Headers
                string[] headers = { "Asset Name", "Type", "Qty", "Purchase Price", "Current Price", "Total Value" };
                foreach (var header in headers)
                {
                    table.AddHeaderCell(new Cell().Add(new Paragraph(header))
                        .SetFont(headerFont)
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .SetTextAlignment(TextAlignment.CENTER));
                }

                // Data Rows
                foreach (var investment in portfolio.Investments)
                {
                    table.AddCell(new Cell().Add(new Paragraph(investment.Asset?.Name ?? "N/A").SetFont(bodyFont)));
                    table.AddCell(new Cell().Add(new Paragraph(investment.Asset?.AssetType ?? "N/A").SetFont(bodyFont)));
                    table.AddCell(new Cell().Add(new Paragraph(investment.Quantity.ToString()).SetFont(bodyFont)).SetTextAlignment(TextAlignment.CENTER));
                    table.AddCell(new Cell().Add(new Paragraph(investment.PurchasePrice.ToString("C")).SetFont(bodyFont)).SetTextAlignment(TextAlignment.RIGHT));
                    table.AddCell(new Cell().Add(new Paragraph((investment.Asset?.CurrentPrice ?? 0M).ToString("C")).SetFont(bodyFont)).SetTextAlignment(TextAlignment.RIGHT));
                    table.AddCell(new Cell().Add(new Paragraph(((investment.Asset?.CurrentPrice ?? 0M) * investment.Quantity).ToString("C")).SetFont(bodyFont)).SetTextAlignment(TextAlignment.RIGHT));
                }

                document.Add(table);
                document.Close();

                return memoryStream.ToArray();
            }
        }
    }
}