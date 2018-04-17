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

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public int turn;

        public MainWindow()
        {
            InitializeComponent();
            cargoShips = new CargoShipCollection(shipDataGrid);

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

            for(int i=0; i<5; i++)
                cargoShips.Add(90, 90, 160);

            cargoShips.SetStrategy(new RandomStrategy());

            DataContext = this;
        }

        private void btn_ship_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;

            new Task(() => GenerateLoadingInstructions()).Start();

            lbl_status.Content = "Instructions generation in process";

            ss = new SummaryScreen();
            ss.Show();
        }

        /// <summary>
        /// Function that generates instruction for each ship and TODO: displays it in summary screen
        /// </summary>
        private async void GenerateLoadingInstructions()
        {
            logger.Info("Loading instructions generation started");
            Task<Dictionary<int, ContainersCollection>> res = cargoShips.LoadContainers();

            Dictionary<int, ContainersCollection> results = await res;

            foreach (int ID in results.Keys)
                ss.AddToMessage("\n" + ID + ": " + results[ID].ToString());

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                lbl_status.Content = "Cargo ready to be shipped";
                btn_ship.IsEnabled = true;
            }));
            logger.Info("Loading instructions generation finished");
        }
    }
}
