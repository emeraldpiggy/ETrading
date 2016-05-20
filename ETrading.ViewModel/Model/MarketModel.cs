using System;
using System.Globalization;
using ETrading.Domain.Entities;
using ETrading.Framework;

namespace ETrading.ViewModel.Model
{
    public class MarketModel : BaseViewModel
    {

        public MarketModel()
        {

        }

        public MarketModel(Product p)
        {
            ProductCode = p.ProductCode;
            PortifolioPosition = p.Quantity;
            Time = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        public MarketModel(Product p, decimal newpoistion)
        {

            ProductCode += p.ProductCode;
            PortifolioPosition = newpoistion;
            Time = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }



        public string ProductCode { get; set; }
        public decimal PortifolioPosition { get; set; }

        public int Trades { get; set; }

        public decimal PortifolioRisk { get; set; }  

        public decimal ProfitScore { get; set; }


        public string Time { get; set; }
    }
}