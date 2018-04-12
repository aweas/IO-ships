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

namespace IOships
{
    public partial class MainWindow : Window
    {
        CargoShipCollection cargoShips;
        public SeriesCollection SeriesCollection { get; set; }

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

            cargoShips.Add(2, 1);

            DataContext = this;
        }

        private void btn_ship_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;

            lbl_status.Content = "Starting data generation";
            Random r = new Random();
            foreach (Ship ship in cargoShips)
                ship.shipData[0].Values.Add(r.Next(0, 100));

            lbl_status.Content = "Finished data generation";

            if (cargoShips.Count<5)
                cargoShips.Add(2, 1);

            lbl_status.Content = "Cargo ready to be shipped";

            (sender as Button).IsEnabled = true;
        }
    }

    public class Ship : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public CartesianChart ship;
        public Label average;
        public SeriesCollection shipData { get; set; }
        public String avg { get; set; }

        public Ship(int id, Grid shipDataGrid)
        {
            setupLabel(id, shipDataGrid);
            setupPlot(id, shipDataGrid);
        }

        private void setupPlot(int id, Grid shipDataGrid)
        {
            Random r = new Random();

            shipData = new SeriesCollection{ new LineSeries{Values = new ChartValues<int>()} };
            shipData[0].Values.CollectionChanged += recalculateAverage;

            for(int i=0; i<3; i++)
                shipData[0].Values.Add(r.Next(0, 100));

            ship = new CartesianChart();
            ship.Hoverable = false;
            ship.Padding = new Thickness(0, 0, 0, 5);
            shipDataGrid.Children.Add(ship);
            Grid.SetRow(ship, id + 1);
            Grid.SetColumn(ship, 1);

            Binding dataBinding = new Binding("shipData");
            dataBinding.Source = this;

            ship.SetBinding(CartesianChart.SeriesProperty, dataBinding);

            shipData[0].Values.CollectionChanged += recalculateAverage;
        }

        private void setupLabel(int id, Grid shipDataGrid)
        {
            avg = "dupa";
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
        }

        public void updateLabel(double mean)
        {
            avg = $"Avg: {(int)mean}";
            OnPropertyChanged("avg");
        }

        private void recalculateAverage(object sender, EventArgs args)
        {
            double mean = 0;
            foreach (int num in shipData[0].Values)
                mean += num;
            mean /= shipData[0].Values.Count;

            updateLabel(mean);
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

    public class CargoShipCollection : List<Ship>
    {
        private Grid boundGrid;
        private int iterator = 0;

        public CargoShipCollection(Grid boundGrid)
        {
            this.boundGrid = boundGrid;
        }

        public void Add(int width, int height)
        {
            if (iterator == 5)
                throw new IndexOutOfRangeException("Trying to add too many ships!");

            this.Add(new Ship(iterator, boundGrid));
            iterator++;
        }
    }
}
