using System.ComponentModel.DataAnnotations;
using InvestmentPortfolioManagement.Common;

namespace InvestmentPortfolioManagement.Models
{
    public class Report
    {
        [Key]
        public Guid ReportId { get; set; }
        public ReportType ReportType { get; set; }
        public DateTime GeneratedDate { get; set; }

    }
}
