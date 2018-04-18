using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Threading;

namespace IOships
{
    /// <summary>
    /// Interaction logic for SummaryScreen.xaml
    /// </summary>
    public partial class SummaryScreen : Window
    {
        public ObservableString Message { get; set; }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public SummaryScreen()
        {
            InitializeComponent();
            Message = new ObservableString("Please wait");

            DataContext = Message;
        }

        public void AddToMessage(string msg)
        {
            Message.Add(msg);

            logger.Trace("Updated summary. New value: {0}", Message.Value);
        }

        private void btn_ship_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }

    public class ObservableString: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string value;
        public string Value
        {
            get { return value; }
            set { this.value = value; NotifyPropertyChanged("Value"); }
        }

        public ObservableString(string val)
        {
            Value = val;
        }

        public void Add(string msg)
        {
            Value += msg;
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
