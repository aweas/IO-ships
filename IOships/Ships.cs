using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.IO;

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
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public int ID;
        public int Width;
        public int Depth;

        private CartesianChart _cartesianChart;
        private Label _averageLabel;
        public String Avg { get; set; }
        public SeriesCollection ShipHistory { get; }
        public SeriesCollection ShipData { get; }
        public List<Guid> ContainersHistory;

        public Ship(int ID, int width, int depth, Grid shipDataGrid)
        {
            this.ID = ID;
            Width = width;
            Depth = depth;
            ShipData = new SeriesCollection {new LineSeries {Values = new ChartValues<int>()}};
            ShipHistory = new SeriesCollection {new LineSeries {Values = new ChartValues<int>()}};

            if (!(shipDataGrid is null))
            {
                SetupLabel(ID, shipDataGrid);
                SetupPlot(ID, shipDataGrid);
            }
        }

        #region initializing ship

        private void SetupPlot(int id, Grid shipDataGrid)
        {
            Logger.Info("Initializing ship's plot");

            ShipHistory[0].Values.CollectionChanged += UpdateDisplay;
            ShipHistory[0].Values.CollectionChanged += RecalculateAverage;

            _cartesianChart = new CartesianChart
            {
                Hoverable = false,
                Margin = new Thickness(0, 0, 0, 5)
            };
            shipDataGrid.Children.Add(_cartesianChart);
            Grid.SetRow(_cartesianChart, id + 1);
            Grid.SetColumn(_cartesianChart, 1);

            Binding dataBinding = new Binding("ShipData") {Source = this};

            _cartesianChart.SetBinding(CartesianChart.SeriesProperty, dataBinding);

            Logger.Info("Initializing ship's plot finished");
        }

        private void SetupLabel(int id, Grid shipDataGrid)
        {
            Logger.Info("Initializing ship's label");

            Avg = "0";
            _averageLabel = new Label
            {
                FontFamily = new FontFamily("Segoe UI Semibold"),
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 24
            };
            shipDataGrid.Children.Add(_averageLabel);
            Grid.SetRow(_averageLabel, id + 1);
            Grid.SetColumn(_averageLabel, 2);

            Binding textBinding = new Binding("Avg")
            {
                Source = this
            };

            _averageLabel.SetBinding(Label.ContentProperty, textBinding);

            Logger.Info("Initializing ship's label finished");
        }

        #endregion

        /// <summary>
        /// Recalculates average for how many containers has ship recently taken
        /// </summary>
        private void RecalculateAverage(object sender, EventArgs args)
        {
            double mean = 0;

            foreach (int num in ShipHistory[0].Values)
                mean += num;

            mean /= ShipHistory[0].Values.Count;
            Avg = $"Avg: {(int) mean}";

            Logger.Trace($"New mean calculated for ID {ID}: {mean}");

            Logger.Trace($"Updating label if {ID} to \"{Avg}\"");
            OnPropertyChanged("avg");
        }

        /// <summary>
        /// Updates object bound to ship's graph
        /// </summary>
        private void UpdateDisplay(object sender, EventArgs args)
        {
            ShipData[0].Values.Add(ContainersHistory.Count);

            if (ShipData[0].Values.Count == 8)
                ShipData[0].Values.RemoveAt(0);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Provides wrapper for List of Ship so that new ships can be added seamlessly
    /// </summary>
    public class CargoShipCollection : List<Ship>
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Grid _boundGrid;
        private int _iterator;
        public IStrategy DataGenStrategy;

        /// <summary>
        /// Binds collection to grid
        /// </summary>
        /// <param name="boundGrid">Grid in which ships' statistics will be displayed</param>
        public CargoShipCollection(Grid boundGrid)
        {
            if (!(boundGrid is null))
                _boundGrid = boundGrid;
        }

        /// <summary>
        /// Adds new ship to collection
        /// </summary>
        /// <param name="width">Width of ship's hold</param>
        /// <param name="depth">Depth of ship's hold</param>
        public void Add(int width, int depth)
        {
            if (_iterator == 5)
                throw new IndexOutOfRangeException("Trying to add too many ships!");

            Add(new Ship(_iterator, width, depth, _boundGrid));
            _iterator++;
        }

		/// <summary>
		/// Loads ships from .csv files,
		/// format: Width;Depth;
		/// </summary>
		/// <param name="filename">Path to .csv file</param>
		public void LoadCsv(string filename)
		{
			using (var data = new StreamReader(filename))
			{
				data.ReadLine();
				string line;
				while ((line = data.ReadLine()) != null)
				{
					List<string> aux = line.Split(';').ToList();

					var w  = int.Parse(aux[0]);
					var d  = int.Parse(aux[1]);

					Add(w, d);
				}
			}
		}

		public async Task<Dictionary<int, InstructionsHelper>> LoadContainers(ContainersCollection containers)
        {
            if (DataGenStrategy is null)
                throw new NullReferenceException("Loading strategy not chosen");

            Logger.Debug("Loading containers collectionwise");
            Dictionary<int, InstructionsHelper> res = DataGenStrategy.GenerateData(this, ref containers);

            foreach (var i in res.Keys)
            {
                await Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                    new Action(() =>
                    {
                        this[i].ContainersHistory = res[i].Instructions.Values.ToList();
                        this[i].ShipHistory[0].Values.Add(res[i].Instructions.Count);
                    }));
            }

            Logger.Trace("Updated ships data");

            return res;
        }
    }
}