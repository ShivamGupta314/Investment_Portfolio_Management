using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManagement.ViewModels
{
    public class EditPortfolioRequest
    {
        public string? PortfolioName { get; set; }

        public string? Description { get; set; }

        public string? Type { get; set; }

        public decimal? TotalValue { get; set; }
    }
}
