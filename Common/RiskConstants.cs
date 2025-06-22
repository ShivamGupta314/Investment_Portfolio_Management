using System;
using System.Collections.Generic;

namespace InvestmentPortfolioManagement.Common
{
    public static class RiskConstants
    {
        // Define base risk scores for asset types
        // This is now a central, shared resource for the whole application.
        public static readonly Dictionary<string, decimal> AssetBaseRiskScores = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            // AssetType, Base Risk Score (0-100)
            { "Stock", 80M },
            { "Bond", 20M },
            { "Mutual Fund", 50M }
            // Add more types here as needed
        };
    }
}