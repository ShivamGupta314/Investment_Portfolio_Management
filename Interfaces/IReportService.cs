using System;
using System.Threading.Tasks;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IReportService
    {
        // Changed to accept portfolioId instead of userId
        byte[] GeneratePortfolioPdf(Guid portfolioId);
        byte[] GenerateAssetCsv(Guid portfolioId); // Changed to accept portfolioId
    }
}