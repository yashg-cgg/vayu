
using System.Windows;

namespace Vayu.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
        }
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (Window item in Application.Current.Windows)
            {
                if (item != null && item != this)
                {
                    item.Close();
                }
            }
            System.Environment.Exit(0);
        }
    }
}
