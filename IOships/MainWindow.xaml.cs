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
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace IOships
{
    public partial class MainWindow : Window
    {
        SummaryScreen ss;

        CargoShipCollection cargoShips;
        public SeriesCollection SeriesCollection { get; set; }
        public ContainersCollection containers;
        public int turn;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            InitializeComponent();
            cargoShips = new CargoShipCollection(shipDataGrid);
            containers = new ContainersCollection();
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

            containers.AddRandom(100, turn);

            for(int i=0; i<5; i++)
                cargoShips.Add(90, 160);

            cargoShips.SetStrategy(new RandomShipStrategy());
            cargoShips.dataGenStrategy = new RandomCollectionStrategy();

            DataContext = this;
        }

        private void UpdatePie()
        {
            SeriesCollection.Clear();
            Dictionary<int, int> data = containers.getAgeAndCount(turn);

            foreach (int key in data.Keys)
            {
                string title;
                if (key < 3)
                {
                    if (key == 0)
                        title = "Newly arrived";
                    else if (key == 1)
                        title = "Waiting for 1 round";
                    else
                        title = $"Waiting for {key} rounds";

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
            if (ss != null)
                ss.Close();
            int mode = cb_mode.SelectedIndex;

            (sender as Button).IsEnabled = false;

            new Task(() => GenerateLoadingInstructions(mode)).Start();

            lbl_status.Content = "Instructions generation in process";

            ss = new SummaryScreen();
            ss.Show();

            turn++;
            containers.AddRandom(100, turn);
        }

        /// <summary>
        /// Function that generates instruction for each ship and displays it in summary screen
        /// </summary>
        private async void GenerateLoadingInstructions(int mode)
        {
            logger.Info("Loading instructions generation started");
            Task<Dictionary<int, Dictionary<Coords, int>>> res = cargoShips.LoadContainers((LoadingMode)mode, containers);

            Dictionary<int, Dictionary<Coords, int>> results = await res;

            foreach (int ID in results.Keys)
            {
                ss.AddToMessage($"\n\n-----------------{ID}-----------------");

                foreach(Coords coords in results[ID].Keys)
                    ss.AddToMessage($"\n{coords.x}, {coords.y}:\t{results[ID][coords]}");
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                lbl_status.Content = "Cargo ready to be shipped";
                btn_ship.IsEnabled = true;
                UpdatePie();
            }));
            logger.Info("Loading instructions generation finished");
        }
    }
}
