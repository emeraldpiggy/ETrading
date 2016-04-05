namespace ETrading.Domain.Entities
{
    public class MarketCondition
    {
        public MarketCondition(string productCode, decimal price, decimal theo)
        {
            ProductCode = productCode;
            Price = price;
            Theo = theo;
        }

        /// <summary>
        /// E.g ASX:BHP
        /// </summary>
        public string ProductCode { get; set; }
        public decimal Price { get; set; }
        public decimal Theo { get; set; }
    }
}
