using System.Threading.Tasks;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IReportService
    {
        byte[] GeneratePortfolioPdf(Guid userId);
        string GenerateAssetCsv(Guid userId);
    }
}
