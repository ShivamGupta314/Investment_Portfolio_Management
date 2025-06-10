using System.ComponentModel.DataAnnotations;
using InvestmentPortfolioManagement.Models;

namespace InvestmentPortfolioManagement.ViewModels
{
    public class AddInvestmentsToPortfolioViewModel
    {
        [Required]
        public Guid PortfolioId { get; set; }

        [Display(Name = "Select Investments")]
        public List<Guid> SelectedInvestmentIds { get; set; } = new List<Guid>();

        public List<Investment>? UnassignedInvestments { get; set; }
    }


    public class InvestmentOption
    {
        public Guid InvestmentId { get; set; }
        public string DisplayName { get; set; } // e.g., "Tesla - 5 Shares"
    }

}
