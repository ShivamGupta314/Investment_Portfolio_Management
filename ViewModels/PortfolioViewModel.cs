using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManagement.ViewModels
{
    public class PortfolioViewModel
    {
        [Required(ErrorMessage = "Portfolio Name is required.")]
        public string PortfolioName { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Total Value is required.")]
        public decimal TotalValue { get; set; }

    }
}
