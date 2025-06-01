using System.Threading.Tasks;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IReportService
    {
        byte[] GeneratePortfolioPdf(int userId);
        string GenerateAssetCsv(int userId);
    }
}
