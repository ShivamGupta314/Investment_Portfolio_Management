using InvestmentPortfolioManagement.Models;
using System.Threading.Tasks;
using System;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> GeneratePortfolioPdfReport(Guid portfolioId);
        Task<string> GeneratePortfolioCsvReport(Guid portfolioId);

    }
}