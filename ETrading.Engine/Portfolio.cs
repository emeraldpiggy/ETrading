using System;
using System.Collections.Generic;
using System.Linq;
using ETrading.Domain.Entities;
using ETrading.I.Interfaces;

namespace ETrading.Engine
{
    public class Portfolio : IPortfolio
    {
        private readonly Dictionary<Product, MarketCondition> _productMarketCondition = new Dictionary<Product, MarketCondition>();

        private readonly Dictionary<Product, int> _trades = new Dictionary<Product, int>();
        public Portfolio(Product[] products)
        {
            Products = products;
        }

        private const int Min = -100;

        private const int Max = 100;

        public Product[] Products
        {
            get;
            set;
        }

        public decimal Score { get; set; }
        public Dictionary<Product, int> Optimize(MarketCondition[] marketCondition)
        {
            Initialize(marketCondition);

            while (true)
            {
                RandomizeTrades();
                var mp = GetMarginalProfit();
                var mr = GetMarginalRisk();
                Score = mp - mr;
                if (Score > 0)
                {
                    foreach (var trade in _trades)
                    {
                        trade.Key.Quantity = trade.Key.Quantity + trade.Value;
                    }
                    return _trades;
                }
            }
        }

        private void Initialize(MarketCondition[] marketConditions)
        {
            foreach (var product in Products)
            {
                MarketCondition markdCondition;
                int trade;
                if (!_productMarketCondition.TryGetValue(product, out markdCondition))
                {
                    _productMarketCondition.Add(product, marketConditions.FirstOrDefault(m => m.ProductCode == product.ProductCode));
                }
                if (!_trades.TryGetValue(product, out trade))
                {
                    _trades.Add(product, 0);
                }
            }
        }

        private void RandomizeTrades()
        {
            Random randNum = new Random();

            foreach (var product in Products)
            {
                _trades[product] = randNum.Next(Min, Max);
            }
        }

        public decimal GetPortifolioRisk()
        {
            // all the products have same number of risk type, so it will have same number of risk factor
            return GetRisks(false);
        }

        private decimal GetRisks(bool isMarginalRisk)
        {
            // all the products have same number of risk type, so it will have same number of risk factor
            decimal[] riskFactors = GetRiskFactor();
            decimal marginalRisk = 0;
            for (int i = 0; i < riskFactors.Length; i++)
            {
                marginalRisk += GetRisk(isMarginalRisk, i);
            }

            return marginalRisk;
        }

        private double CalculateRisk(int i)
        {
            return Products.Sum(product => (double)(product.RiskRelationship[i] *
                                                     _productMarketCondition[product].Theo * product.Quantity));
        }
        private decimal GetRisk(bool calculateMarginalRisks = false, int i = 0)
        {
            decimal marginalResults = 0;
            if (calculateMarginalRisks)
            {
                var res = Products.Sum(product =>
                {
                    var p1 =
                        (double)
                            (product.RiskRelationship[i] * _productMarketCondition[product].Theo *
                             (product.Quantity + _trades[product]));
                    return p1;
                });
                marginalResults = (decimal)Math.Pow(res, 2);
                var differences = (decimal)Math.Pow(CalculateRisk(i), 2);

                marginalResults = marginalResults - differences;
            }
            else
            {
                marginalResults = (decimal)Math.Pow(CalculateRisk(i), 2);
            }

            return marginalResults;
        }

        private decimal[] GetRiskFactor()
        {
            // check for risk relationship
            return Products.First(p => p.RiskRelationship != null).RiskRelationship;
        }

        private decimal GetMarginalProfit()
        {
            return Products.Sum(product => _trades[product] * (_productMarketCondition[product].Theo - _productMarketCondition[product].Price));
        }

        private decimal GetMarginalRisk()
        {
            // all the products have same number of risk type, so it will have same number of risk factor
            return GetRisks(true);
        }
    }
}
