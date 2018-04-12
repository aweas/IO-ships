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
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

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

            Ship ship0 = new Ship(0, shipDataGrid);
            Ship ship1 = new Ship(1, shipDataGrid);

            DataContext = this;
        }

        public SeriesCollection SeriesCollection { get; set; }
        public SeriesCollection Ship1Data { get; set; }
        public SeriesCollection Ship2Data { get; set; }
        public string ship1text { get; set; }
    }

    public class Ship : INotifyPropertyChanged
    {
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
            shipData = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<int>{86, 43, 66, 43, 75 }
                }
            };

            ship = new CartesianChart();
            shipDataGrid.Children.Add(ship);
            Grid.SetRow(ship, id + 1);
            Grid.SetColumn(ship, 1);

            Binding dataBinding = new Binding("shipData");
            dataBinding.Source = this;

            ship.SetBinding(CartesianChart.SeriesProperty, dataBinding);
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
