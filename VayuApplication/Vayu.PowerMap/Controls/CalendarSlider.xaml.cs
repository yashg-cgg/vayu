using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Vayu.PowerMap.Controls
{
    /// <summary>
    /// Interaction logic for CalendarSlider.xaml
    /// </summary>
    public partial class CalendarSlider : UserControl
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
        }
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
                RaisePropertyChanged("IsRange");
            }
        }

        private bool? _isHour = true;
        public bool? IsHour
        {
            get { return _isHour; }
            set
            {
                _isHour = value;
                RaisePropertyChanged("IsHour");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

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
            //FromDate_datePicker.SelectedDate = from_date.AddDays(1);
            from_date = from_date.AddDays(1);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //FromDate_datePicker.SelectedDate = from_date.AddDays(-1);
            from_date = from_date.AddDays(-1);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            //ToDate_datePicker.SelectedDate = to_date.AddDays(1);
            to_date = to_date.AddDays(1);
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            //ToDate_datePicker.SelectedDate = to_date.AddDays(-1);
            to_date = to_date.AddDays(-1);
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            //Exact_datePicker.SelectedDate = exact_date.AddDays(1);
            exact_date = exact_date.AddDays(1);
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            //Exact_datePicker.SelectedDate = exact_date.AddDays(-1);
            exact_date = exact_date.AddDays(-1);
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            HE++;
            //Timeslider.Value = Timeslider.Value + 1;
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            HE--;
            //Timeslider.Value = Timeslider.Value - 1;
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            exact_date = DateTime.Today;
            HE = DateTime.Now.Hour + 1;
            //Exact_datePicker.SelectedDate = DateTime.Today;
            //Timeslider.Value = DateTime.Now.Hour + 1;
        }

        private void button10_Click(object sender, RoutedEventArgs e)
        {
            from_date = DateTime.Today;
            to_date = DateTime.Today;
            //FromDate_datePicker.SelectedDate = DateTime.Today;
            //ToDate_datePicker.SelectedDate = DateTime.Today;
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
