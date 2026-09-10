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
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls.Primitives;

namespace Vayu
{
    /// <summary>
    /// Interaction logic for CalenderSlider.xaml
    /// </summary>
    public partial class CalendarSlider : UserControl, INotifyPropertyChanged
    {
        public CalendarSlider()
        {
            InitializeComponent();
            button1.Click += new RoutedEventHandler(button1_Click);
            button2.Click += new RoutedEventHandler(button2_Click);
            button3.Click += new RoutedEventHandler(button3_Click);
            button4.Click += new RoutedEventHandler(button4_Click);
            button5.Click += new RoutedEventHandler(button5_Click);
            button6.Click += new RoutedEventHandler(button6_Click);
            button7.Click += new RoutedEventHandler(button7_Click);
            button8.Click += new RoutedEventHandler(button8_Click);
            button9.Click += new RoutedEventHandler(button9_Click);
            button10.Click += new RoutedEventHandler(button10_Click);
        }

        //public static readonly DependencyProperty TimeProperty1 =
        //DependencyProperty.Register("from_date", typeof(DateTime),
        //typeof(CalendarSlider), new FrameworkPropertyMetadata(DateTime.Today, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //public DateTime from_date
        //{
        //    get { return (DateTime)GetValue(TimeProperty1); }
        //    set { SetValue(TimeProperty1, value); }
        //}

        //public static readonly DependencyProperty TimeProperty2 =
        //DependencyProperty.Register("to_date", typeof(DateTime),
        //typeof(CalendarSlider), new FrameworkPropertyMetadata(DateTime.Today, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //public DateTime to_date
        //{
        //    get { return (DateTime)GetValue(TimeProperty2); }
        //    set { SetValue(TimeProperty2, value); }
        //}

        //public static readonly DependencyProperty TimeProperty3 =
        //DependencyProperty.Register("exact_date", typeof(DateTime),
        //typeof(CalendarSlider), new FrameworkPropertyMetadata(DateTime.Today, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //public DateTime exact_date
        //{
        //    get { return (DateTime)GetValue(TimeProperty3); }
        //    set { SetValue(TimeProperty3, value); }
        //}

        //public static readonly DependencyProperty TimeProperty4 =
        //DependencyProperty.Register("HE", typeof(int),
        //typeof(CalendarSlider), new FrameworkPropertyMetadata(DateTime.Now.Hour + 1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //public int HE
        //{
        //    get { return (int)GetValue(TimeProperty4); }
        //    set { SetValue(TimeProperty4, value); }
        //}

        private DateTime _from_date = DateTime.Today;
        public DateTime from_date
        {
            get { return _from_date; }
            set
            {
                _from_date = value;
                RaisePropertyChanged("from_date");
            }
        }

        private DateTime _to_date = DateTime.Today;
        public DateTime to_date
        {
            get { return _to_date; }
            set
            {
                _to_date = value;
                RaisePropertyChanged("to_date");
            }
        }

        private DateTime _exact_date = DateTime.Today;
        public DateTime exact_date
        {
            get { return _exact_date; }
            set
            {
                _exact_date = value;
                RaisePropertyChanged("exact_date");
            }
        }

        private int _he = DateTime.Now.Hour + 1;
        public int HE
        {
            get { return _he; }
            set
            {
                _he = value;
                RaisePropertyChanged("HE");
            }
        }

        private bool? _isRange = false;
        public bool? IsRange
        {
            get { return _isRange; }
            set
            {
                _isRange = value;
                RaisePropertyChanged("IsRange", _isRange.GetValueOrDefault());
            }
        }

        private bool? _isHour = true;
        public bool? IsHour
        {
            get { return _isHour; }
            set
            {
                _isHour = value;
                RaisePropertyChanged("IsHour", _isHour.GetValueOrDefault());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName,bool val = false)
        {
            if (this.PropertyChanged != null)
            {
                if (propertyName == "IsRange" || propertyName == "IsHour")
                {
                    if (!val)
                        this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            if (propertyName == "IsRange" || propertyName == "IsHour")
            {
                if ((bool)IsRange)
                    tabControl1.SelectedIndex = 1;
                if ((bool)IsHour)
                    tabControl1.SelectedIndex = 0;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            from_date = from_date.AddDays(1);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            from_date = from_date.AddDays(-1);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            to_date = to_date.AddDays(1);
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            to_date = to_date.AddDays(-1);
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            exact_date = exact_date.AddDays(1);
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            exact_date = exact_date.AddDays(-1);
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            HE++;
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            HE--;
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            exact_date = DateTime.Today;
            HE = DateTime.Now.Hour + 1;
        }

        private void button10_Click(object sender, RoutedEventArgs e)
        {
            from_date = DateTime.Today;
            to_date = DateTime.Today;
        }
    }

    public class NumberedTickBar : TickBar
    {
        protected override void OnRender(DrawingContext dc)
        {
            Size size = new Size(base.ActualWidth, base.ActualHeight);
            int tickCount = (int)((this.Maximum - this.Minimum) / this.TickFrequency) + 1;
            if ((this.Maximum - this.Minimum) % this.TickFrequency == 0)
            {
                tickCount -= 1;
            }
            Double tickFrequencySize;
            // Calculate tick's setting
            tickFrequencySize = (size.Width * this.TickFrequency / (this.Maximum - this.Minimum));
            string text = "";
            FormattedText formattedText = null;
            double num = this.Maximum - this.Minimum;
            int i = 0;
            // Draw each tick text
            for (i = 0; i <= tickCount; i++)
            {
                text = Convert.ToString(Convert.ToInt32(this.Minimum + this.TickFrequency * i), 10);
                formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 8, Brushes.Black);
                if (i % 2 == 0)
                {
                    dc.DrawText(formattedText, new Point((tickFrequencySize * i), 0)); // draw text 8 pixels below slider element
                }

            }
        }
    }
}
