using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ETrading.Domain.Entities;
using ETrading.Engine;
using ETrading.I.Interfaces;
using ETrading.ViewModel.Model;

namespace ETrading
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly IPortfolio _portfolio;
        public ObservableCollection<MarketModel> MarketModels { get; set; }

        private Product _p1;
        private Product _p2;
        private readonly Product[] products;
        public MainWindow()
        {
            InitializeComponent();
            products = PopulateData();
            _portfolio = new Portfolio(products);
            MarketModels = new ObservableCollection<MarketModel>();
            var mm1 = new MarketModel(_p1);
            MarketModels.Add(mm1);
            var mm2 = new MarketModel(_p2);
            MarketModels.Add(mm2);

            this.DataContext = this;

        }

        private decimal sampleRisk1 = (decimal)0.01;
        private decimal sampleRisk2 = 0;

        private MarketCondition[] _marketConditions;
        public Product[] PopulateData()
        {
            _p1 = new Product("P1", "P1", 0, new decimal[] { sampleRisk1, sampleRisk1 });
            MarketCondition mp1 = new MarketCondition("P1", 130, 100);

            _p2 = new Product("P2", "P2", 0, new decimal[] { sampleRisk2, sampleRisk1 });
            MarketCondition mp2 = new MarketCondition("P2", 110, 100);

            _marketConditions = new[] { mp1, mp2 };

            return new[] { _p1, _p2 };

        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            var result = _portfolio.Optimize(_marketConditions);
            BuildModel(result);
        }

        private void BuildModel(Dictionary<Product, int> result)
        {
            foreach (var i in result)
            {
                var mm1 = new MarketModel(i.Key, i.Key.Quantity)
                {
                    ProfitScore = _portfolio.Score,
                    Trades = i.Value,
                    PortifolioRisk =  _portfolio.GetPortifolioRisk()

                };
                MarketModels.Add(mm1);
            }
        }

        private void control_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var datagrid = sender as DataGrid;
            // If the entire contents fit on the screen, ignore this event
            if (e.ExtentHeight < e.ViewportHeight)
                return;

            // If no items are available to display, ignore this event
            if (this.MarketModels.Count <= 0)
                return;

            // If the ExtentHeight and ViewportHeight haven't changed, ignore this event
            if (e.ExtentHeightChange == 0.0 && e.ViewportHeightChange == 0.0)
                return;

            // If we were close to the bottom when a new item appeared,
            // scroll the new item into view.  We pick a threshold of 5
            // items since issues were seen when resizing the window with
            // smaller threshold values.
            var oldExtentHeight = e.ExtentHeight - e.ExtentHeightChange;
            var oldVerticalOffset = e.VerticalOffset - e.VerticalChange;
            var oldViewportHeight = e.ViewportHeight - e.ViewportHeightChange;
            if (oldVerticalOffset + oldViewportHeight + 5 >= oldExtentHeight)
            {
                if (datagrid != null) datagrid.ScrollIntoView(this.MarketModels[this.MarketModels.Count - 1]);
            }
        }
    }
}
