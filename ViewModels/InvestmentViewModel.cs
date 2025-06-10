using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using InvestmentPortfolioManagement.Models;

namespace InvestmentPortfolioManagement.ViewModels
{
    public class InvestmentViewModel
    {
        public Guid? PortfolioId { get; set; }

        [Required]
        public Guid AssetId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public List<Asset> AvailableAssets { get; set; } = new List<Asset>();
    }
}
