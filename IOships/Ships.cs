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

        private CartesianChart ship;
        private Label average;
        public String avg { get; set; }
        public SeriesCollection shipData { get; set; }
        private ContainersCollection containersHistory;

        public IStrategy dataGenStrategy;

        public Ship(int ID, Grid shipDataGrid)
        {
            this.ID = ID;
            setupLabel(ID, shipDataGrid);
            setupPlot(ID, shipDataGrid);
        }

        #region initializing ship
        private void setupPlot(int id, Grid shipDataGrid)
        {
            Random r = new Random();

            logger.Info("Initializing ship's plot");
            shipData = new SeriesCollection { new LineSeries { Values = new ChartValues<int>() } };
            shipData[0].Values.CollectionChanged += RecalculateAverage;

            ship = new CartesianChart();
            ship.Hoverable = false;
            ship.Margin = new Thickness(0, 0, 0, 5);
            shipDataGrid.Children.Add(ship);
            Grid.SetRow(ship, id + 1);
            Grid.SetColumn(ship, 1);

            Binding dataBinding = new Binding("shipData");
            dataBinding.Source = this;

            ship.SetBinding(CartesianChart.SeriesProperty, dataBinding);

            shipData[0].Values.CollectionChanged += RecalculateAverage;
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
            foreach (int num in shipData[0].Values)
                mean += num;
            mean /= shipData[0].Values.Count;
            avg = $"Avg: {(int)mean}";

            logger.Trace($"New mean calculated for ID {ID}: {mean}");

            logger.Trace($"Updating label if {ID} to \"{avg}\"");
            OnPropertyChanged("avg");

        }

        /// <summary>
        /// Generates containers to be taken this turn according to given strategy
        /// </summary>
        public ContainersCollection GenerateContainers()
        {
            logger.Trace($"Starting strategy for ship {ID}");

            if (dataGenStrategy is null)
                throw new NullReferenceException("Loading strategy not chosen");

            containersHistory = dataGenStrategy.GenerateData(ID, null);
            logger.Trace($"Finished strategy for ID {ID}");

            shipData[0].Values.Add(containersHistory.Count);

            return containersHistory;
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
        public void Add(int width, int height, int depth)
        {
            if (iterator == 5)
                throw new IndexOutOfRangeException("Trying to add too many ships!");

            this.Add(new Ship(iterator, boundGrid));
            iterator++;
        }

        /// <summary>
        /// Sets loading strategy for all ships in collection
        /// </summary>
        /// <param name="strategy">Strategy that will be used to load containers onto ship</param>
        public void SetStrategy(IStrategy strategy)
        {
            foreach (Ship s in this)
                s.dataGenStrategy = strategy;
        }

        /// <summary>
        /// Asynchronously generates containers to be loaded this turn for all ships according to provided strategy
        /// </summary>
        /// <returns>
        /// Task resulting in dictionary of containers for each ship
        /// </returns>
        public async Task<Dictionary<int, ContainersCollection>> LoadContainers()
        {
            Dictionary<int, ContainersCollection> answers = new Dictionary<int, ContainersCollection>();
            List<Task<ContainersCollection>> results = new List<Task<ContainersCollection>>();

            logger.Debug("Starting data generation threads");
            foreach (Ship s in this)
            {
                Task<ContainersCollection> generateData = Task.Run(() => s.GenerateContainers());
                results.Add(generateData);
            }

            logger.Debug("Gathering data from threads");
            for (int i=0; i<this.Count; i++)
            {
                logger.Trace("Waiting for thread {0}", i);
                ContainersCollection res = await results[i];
                logger.Trace("Got result from thread {0}", i);
                answers.Add(this[i].ID, res);
            }

            return answers;
        }
    }
}
