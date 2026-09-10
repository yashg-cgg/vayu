using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace Vayu.CommonControls
{
    /// <summary>
    /// Interaction logic for ASDatePicker.xaml
    /// </summary>
    public partial class ASDatePicker : UserControl, INotifyPropertyChanged
    {
        //private DateTime _displaydate = DateTime.Today;
        //public DateTime displaydate
        //{
        //    get { return _displaydate; }
        //    set
        //    {
        //        _displaydate = value;
        //        RaisePropertyChanged("displaydate");
        //    }
        //}

        public static readonly DependencyProperty TimeProperty =
        DependencyProperty.Register("displaydate", typeof(DateTime),
        typeof(ASDatePicker), new FrameworkPropertyMetadata(DateTime.Today,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public DateTime displaydate
        {
            get { return (DateTime)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public ASDatePicker()
        {
            InitializeComponent();
            forward_button.Click +=new RoutedEventHandler(forward_button_Click);
            backward_button.Click += new RoutedEventHandler(backward_button_Click);
            current_button.Click +=new RoutedEventHandler(current_button_Click);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void forward_button_Click(object sender, RoutedEventArgs e)
        {
            displaydate = displaydate.AddDays(1);
        }

        private void backward_button_Click(object sender, RoutedEventArgs e)
        {
            displaydate = displaydate.AddDays(-1);
        }

        private void current_button_Click(object sender, RoutedEventArgs e)
        {
            displaydate = DateTime.Today;
        }
    }
}
