using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.ComponentModel;

namespace IOships
{
    /// <summary>
    /// Objects of this class will represent ships in docking ports. 
    /// They will be responsible for generating ways to pack containers and holding historical data.
    /// Each will be displayed as a row in bound grid.
    /// </summary>
    public class Ship : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public int ID;
        public int width;
        public int depth;

        private CartesianChart ship;
        private Label average;
        public String avg { get; set; }
        public SeriesCollection shipHistory { get; set; }
        public SeriesCollection shipData { get; set; }
        public List<int> containersHistory;

        public Ship(int ID, int width, int depth, Grid shipDataGrid)
        {
            this.ID = ID;
            this.width = width;
            this.depth = depth;

            setupLabel(ID, shipDataGrid);
            setupPlot(ID, shipDataGrid);
        }

        #region initializing ship
        private void setupPlot(int id, Grid shipDataGrid)
        {
            Random r = new Random();

            logger.Info("Initializing ship's plot");
            shipData = new SeriesCollection { new LineSeries { Values = new ChartValues<int>() } };
            shipHistory = new SeriesCollection { new LineSeries { Values = new ChartValues<int>() } };
            shipHistory[0].Values.CollectionChanged += UpdateDisplay;
            shipHistory[0].Values.CollectionChanged += RecalculateAverage;

            ship = new CartesianChart();
            ship.Hoverable = false;
            ship.Margin = new Thickness(0, 0, 0, 5);
            shipDataGrid.Children.Add(ship);
            Grid.SetRow(ship, id + 1);
            Grid.SetColumn(ship, 1);

            Binding dataBinding = new Binding("shipData");
            dataBinding.Source = this;

            ship.SetBinding(CartesianChart.SeriesProperty, dataBinding);

            logger.Info("Initializing ship's plot finished");
        }

        private void setupLabel(int id, Grid shipDataGrid)
        {
            logger.Info("Initializing ship's label");

            avg = "0";
            average = new Label();
            average.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            average.Foreground = new SolidColorBrush(Colors.White);
            average.VerticalAlignment = VerticalAlignment.Bottom;
            average.HorizontalAlignment = HorizontalAlignment.Center;
            average.FontSize = 24;
            shipDataGrid.Children.Add(average);
            Grid.SetRow(average, id + 1);
            Grid.SetColumn(average, 2);

            Binding textBinding = new Binding("avg");
            textBinding.Source = this;

            average.SetBinding(Label.ContentProperty, textBinding);

            logger.Info("Initializing ship's label finished");
        }
        #endregion

        /// <summary>
        /// Recalculates average for how many containers has ship recently taken
        /// </summary>
        private void RecalculateAverage(object sender, EventArgs args)
        {
            double mean = 0;

            foreach (int num in shipHistory[0].Values)
                mean += num;

            mean /= shipHistory[0].Values.Count;
            avg = $"Avg: {(int)mean}";

            logger.Trace($"New mean calculated for ID {ID}: {mean}");

            logger.Trace($"Updating label if {ID} to \"{avg}\"");
            OnPropertyChanged("avg");
        }

        /// <summary>
        /// Updates object bound to ship's graph
        /// </summary>
        private void UpdateDisplay(object sender, EventArgs args)
        {
            this.shipData[0].Values.Add(containersHistory.Count);

            if (shipData[0].Values.Count == 8)
                shipData[0].Values.RemoveAt(0);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Provides wrapper for List<Ship> so that new ships can be added seamlessly
    /// </summary>
    public class CargoShipCollection : List<Ship>
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Grid boundGrid;
        private int iterator = 0;
        public IStrategy dataGenStrategy;

        /// <summary>
        /// Binds collection to grid
        /// </summary>
        /// <param name="boundGrid">Grid in which ships' statistics will be displayed</param>
        public CargoShipCollection(Grid boundGrid)
        {
            this.boundGrid = boundGrid;
        }

        /// <summary>
        /// Adds new ship to collection
        /// </summary>
        /// <param name="width">Width of ship's hold</param>
        /// <param name="height">Height of ship's hold</param>
        /// <param name="depth">Depth of ship's hold</param>
        public void Add(int width, int depth)
        {
            if (iterator == 5)
                throw new IndexOutOfRangeException("Trying to add too many ships!");

            this.Add(new Ship(iterator, width, depth, boundGrid));
            iterator++;
        }

        /// <summary>
        /// Sets loading strategy for this collection
        /// </summary>
        /// <param name="strategy">Collectionwise strategy that will be used to load containers onto ship</param>
        public void SetStrategy(IStrategy strategy)
        {
            dataGenStrategy = strategy;
        }

        public async Task<Dictionary<int, InstructionsHelper>> LoadContainers(LoadingMode mode, ContainersCollection containers)
        {
            if (dataGenStrategy is null)
                throw new NullReferenceException("Loading strategy not chosen");

            logger.Debug("Loading containers collectionwise");
            Dictionary<int, InstructionsHelper> res = dataGenStrategy.GenerateData(this, containers);

            foreach (int i in res.Keys)
            {
                await Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                    this[i].containersHistory = res[i].Instructions.Values.ToList<int>();
                    this[i].shipHistory[0].Values.Add(res[i].Instructions.Count);
                }));
            }

            logger.Trace("Updated ships data");

            return res;
        }
    }
}
