// Enables minimum mode for testing algorithms

#define MINIMUM_MODE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace IOships
{
    // ReSharper disable once UnusedMember.Global
    public partial class MainWindow
    {
        public SeriesCollection SeriesCollection { get; set; }

        public List<IStrategy> AvailableStrategies { get; set; }

        private readonly CargoShipCollection _cargoShips;
        private readonly ContainersCollection _containers;
        private int _turn;

        private SummaryScreen _ss;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
#if !MINIMUM_MODE
            InitializeComponent();
            DataContext = this;
#endif

            _cargoShips = new CargoShipCollection(shipDataGrid);
            SeriesCollection = new SeriesCollection();
            _containers = new ContainersCollection();

            Tools.ContainerGenerator.Generate();
            _containers.LoadCsv("containers.csv", _turn);

            for (int i = 0; i < 5; i++)
                _cargoShips.Add(32, 30);

            _cargoShips.DataGenStrategy = new IterativeStrategy();

#if MINIMUM_MODE
            Hide();
            _ss = new SummaryScreen();
            _ss.Show();

            // Make sure program closes after SS is closed
            _ss.SyncronizationClosedEvent += delegate { Close(); };
            GenerateLoadingInstructions(); // Right now the argument does not change anything
#else
            UpdatePie();
#endif
        }

        private void UpdatePie()
        {
            SeriesCollection.Clear();
            Dictionary<int, int> data = _containers.GetAgeAndCount(_turn);

            foreach (var key in data.Keys.Reverse())
            {
                if (key < 3)
                {
                    string title;
                    switch (key)
                    {
                        case 0:
                            title = "Newly arrived";
                            break;
                        case 1:
                            title = "Waiting for 1 round";
                            break;
                        default:
                            title = $"Waiting for {key} rounds";
                            break;
                    }

                    SeriesCollection.Add(new PieSeries
                    {
                        Title = title,
                        Values = new ChartValues<ObservableValue> {new ObservableValue(data[key])},
                        DataLabels = true
                    });
                }
            }

            int sum = 0;
            foreach (int key in data.Keys)
                if (key >= 3)
                    sum += data[key];

            if (sum > 0)
                SeriesCollection.Add(new PieSeries
                {
                    Title = "Waiting for 3+ rounds",
                    Values = new ChartValues<ObservableValue> {new ObservableValue(sum)},
                    DataLabels = true
                });
        }

        private void btn_ship_Click(object sender, RoutedEventArgs e)
        {
            //Make sure that only one summary window exists
            _ss?.Close();

            ((Button) sender).IsEnabled = false;

            new Task(GenerateLoadingInstructions).Start();

            lbl_status.Content = "Instructions generation in process";

            _ss = new SummaryScreen();
            _ss.Show();

            _turn++;

            Tools.ContainerGenerator.Generate();
            _containers.LoadCsv("containers.csv", _turn);
        }

        /// <summary>
        /// Function that generates instruction for each ship and displays it in summary screen
        /// </summary>
        private async void GenerateLoadingInstructions()
        {
            Logger.Info("Loading instructions generation started");
            try
            {
                Task<Dictionary<int, InstructionsHelper>> res = _cargoShips.LoadContainers(_containers);

                Dictionary<int, InstructionsHelper> results = await res;

                foreach (var ID in results.Keys)
                {
                    _ss.AddToMessage($"\n\n-----------------{ID}-----------------");
                    _ss.AddToMessage($"\nPercentage filled: {results[ID].GetPercentageFilled()}%");
                    _ss.AddToMessage(
                        $"\nTiles filled: {results[ID].GetOccupiedTilesCount()}/{_cargoShips[ID].Width * _cargoShips[ID].Depth}\n");

                    foreach (Coords coords in results[ID].Instructions.Keys)
                        _ss.AddToMessage($"\n{coords.X}, {coords.Y}:\t{results[ID].Instructions[coords]}");

                    foreach (var row in results[ID].RowVisualisation())
                        _ss.AddToMessage($"\n{row}");
                }

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    lbl_status.Content = "Cargo ready to be shipped";
                    btn_ship.IsEnabled = true;
                    UpdatePie();
                }));
                Logger.Info("Loading instructions generation finished");
            }
            catch (NotImplementedException exc)
            {
                MessageBox.Show("Not yet implemented, sorry");
            }
        }

        private void cb_mode_DropDownClosed(object sender, EventArgs e)
        {
            _cargoShips.DataGenStrategy = (IStrategy) cb_mode.SelectedItem;
        }
    }
}