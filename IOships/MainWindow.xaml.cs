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
                cargoShips.Add(2, 1);

            cargoShips.SetStrategy(new RandomStrategy());

            DataContext = this;
        }

        private void btn_ship_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;

            new Task(() => GenerateLoadingInstructions()).Start();

            lbl_status.Content = "Instructions generation in process";
        }

        private async void GenerateLoadingInstructions()
        {
            logger.Info("Loading instructions generation started");
            Task<Dictionary<int, Containers>> res = cargoShips.LoadContainers();

            Dictionary<int, Containers> results = await res;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                lbl_status.Content = "Cargo ready to be shipped";
                btn_ship.IsEnabled = true;
            }));
            logger.Info("Loading instructions generation finished");
        }
    }

    public interface IStrategy
    {
        Containers GenerateData(int shipID);
    }

    public class RandomStrategy : IStrategy
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Container RandomContainer()
        {
            Random r = new Random();
            return new Container(r.Next(), r.Next(), r.Next(), r.Next(), r.Next());
        }

        public Containers GenerateData(int shipID)
        {
            logger.Trace("Starting data generation for container {0}", shipID);

            Containers data = new Containers();
            Random r = new Random();
            int cap = r.Next(1, 100);
            for (int i = 0; i < cap; i++)
                data.Add(RandomContainer());

            logger.Trace("Finished data generation for container {0}", shipID);

            int delay = r.Next(1, 200) * 100;
            System.Threading.Thread.Sleep(delay);
            return data;
        }
    }
}
