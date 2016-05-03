using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETrading.Domain.Entities;

namespace ETrading.Tests.IntegrationTests
{
    public class SenarioOne:IntegrationTestBase
    {
        private readonly Dictionary<Product, int> _trades = new Dictionary<Product, int>();

        public Product[] Products
        {
            get;
            set;
        }

        private void SampleTest()
        {
            _trades[Products[0]] = -10;
            _trades[Products[1]] = 5;
        }

    }
}
