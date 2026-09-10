
using System.Windows;

namespace Vayu.LoadCurve.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void SetPosition(int height, int width, int top, int left)
        {
            Height = height;
            Width = width;
            Top = top;
            Left = left;
        }
    }
}
