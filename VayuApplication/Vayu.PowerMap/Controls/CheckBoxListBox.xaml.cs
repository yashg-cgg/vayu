using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Vayu.PowerMap.Controls
{
    /// <summary>
    /// Interaction logic for CheckBoxListBox.xaml
    /// </summary>
    public partial class CheckBoxListBox : UserControl
    {
        public CheckBoxListBox()
        {
            InitializeComponent();
        }
        private void All_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            foreach (LItem item in ListBox.Items)
            {
                item.IsChecked = true;
            }
        }

        private void None_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            foreach (LItem item in ListBox.Items)
            {
                item.IsChecked = false;
            }
        }
    }
    public class LItem : INotifyPropertyChanged
    {
        public bool? isChecked = true;
        public string _text;

        public bool? IsChecked
        {
            get { return this.isChecked; }
            set
            {
                this.isChecked = value;
                RaisePropertyChanged("IsChecked");
            }
        }

        public string Text
        {
            get { return this._text; }
            set { this._text = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
