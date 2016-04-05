namespace ETrading.Domain.Entities
{
    public class Product         
    {
        public Product(string productCode, string name, decimal quantity, decimal[] riskRelationship)
        {
            ProductCode = productCode;
            Name = name;
            Quantity = quantity;
            RiskRelationship = riskRelationship;
        }

        /// <summary>
        /// ProductCoode such as Asx:bhp
        /// </summary>
        public string ProductCode { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }

        /// <summary>
        /// Assume all the products have the samme type of risks, 
        /// inflation = 1, interets rate =2, GDP =3, Policy = 4
        /// Return value is called Value at risk
        /// </summary>
        public decimal[] RiskRelationship { get; set; }
    }
}
