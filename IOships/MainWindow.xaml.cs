// Enables minimum mode for testing algorithms
#define MINIMUM_MODE

using System;
using System.Collections.Generic;
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
        private SummaryScreen _ss;

        private readonly CargoShipCollection _cargoShips;
        public SeriesCollection SeriesCollection { get; set; }
        public ContainersCollection Containers;
        public int Turn;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
#if !MINIMUM_MODE
            InitializeComponent();
            _cargoShips = new CargoShipCollection(shipDataGrid);
            Containers = new ContainersCollection();
            SeriesCollection = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Newly arrived containers",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(250) },
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "Waiting for 1 round",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(130) },
                    DataLabels = true
                }
            };

            Containers.AddRandom(100, Turn);

            for(int i=0; i<5; i++)
                _cargoShips.Add(90, 160);

            _cargoShips.DataGenStrategy = new RandomCollectionStrategy();

            DataContext = this;
#else
            // Hide GUI
            Hide();

            // Initialize everything
            Containers = new ContainersCollection();                        
            SeriesCollection = new SeriesCollection();
            _cargoShips = new CargoShipCollection(null);
            for (var i = 0; i < 5; i++)
                _cargoShips.Add(90, 160);

            // Implement RandomcCollectionStrategy
            _cargoShips.DataGenStrategy = new RandomCollectionStrategy();    

            Containers.AddRandom(100, Turn);
            _ss = new SummaryScreen();
            _ss.Show();

            // Make sure program closes after SS is closed
            _ss.SyncronizationClosedEvent += delegate { Close(); } ;                      
            GenerateLoadingInstructions((int)LoadingMode.Random);           // Right now the argument does not change anything
#endif
        }

        private void UpdatePie()
        {
            SeriesCollection.Clear();
            Dictionary<int, int> data = Containers.GetAgeAndCount(Turn);

            foreach (var key in data.Keys)
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
                        Values = new ChartValues<ObservableValue> { new ObservableValue(data[key]) },
                        DataLabels = true
                    });
                }
            }

            int sum = 0;
            foreach (int key in data.Keys)
                if (key >= 3)
                    sum += data[key];

            if(sum>0)
                SeriesCollection.Add(new PieSeries
                {
                    Title = "Waiting for 3+ rounds",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(sum) },
                    DataLabels = true
                });

        }

        private void btn_ship_Click(object sender, RoutedEventArgs e)
        {
            //Make sure that only one summary window exists
            _ss?.Close();
            var mode = cb_mode.SelectedIndex;

            ((Button) sender).IsEnabled = false;

            new Task(() => GenerateLoadingInstructions(mode)).Start();

            lbl_status.Content = "Instructions generation in process";

            _ss = new SummaryScreen();
            _ss.Show();
            
            Turn++;
            Containers.AddRandom(100, Turn);
        }

        /// <summary>
        /// Function that generates instruction for each ship and displays it in summary screen
        /// </summary>
        private async void GenerateLoadingInstructions(int mode)
        {
            Logger.Info("Loading instructions generation started");
            Task<Dictionary<int, InstructionsHelper>> res = _cargoShips.LoadContainers((LoadingMode)mode, Containers);

            Dictionary<int, InstructionsHelper> results = await res;

            foreach (var ID in results.Keys)
            {
                _ss.AddToMessage($"\n\n-----------------{ID}-----------------");
                _ss.AddToMessage($"\nPercentage filled: {results[ID].GetPercentageFilled()}%");
                _ss.AddToMessage($"\nTiles filled: {results[ID].GetOccupiedTilesCount()}/{_cargoShips[ID].Width*_cargoShips[ID].Depth}\n");

                foreach (Coords coords in results[ID].Instructions.Keys)
                    _ss.AddToMessage($"\n{coords.X}, {coords.Y}:\t{results[ID].Instructions[coords]}");
            }
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                lbl_status.Content = "Cargo ready to be shipped";
                btn_ship.IsEnabled = true;
                UpdatePie();
            }));
            Logger.Info("Loading instructions generation finished");
        }
    }
}
