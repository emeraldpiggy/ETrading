using System.Collections.Generic;
using ETrading.Domain.Entities;

namespace ETrading.I.Interfaces
{
    public interface IPortfolio
    {
        Product[] Products { get; }
        Dictionary<Product, int> Optimize(MarketCondition[] marketCondition);
        decimal GetPortifolioRisk();
        decimal Score { get; }
    }
}