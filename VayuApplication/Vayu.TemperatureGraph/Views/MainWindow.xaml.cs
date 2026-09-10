
using System.Windows;

namespace Vayu.TemperatureGraph.Views
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
        /// <summary>
        /// Sets the position.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <param name="top">The top.</param>
        /// <param name="left">The left.</param>
        public void SetPosition(int height, int width, int top, int left)
        {
            Height = height;
            Width = width;
            Top = top;
            Left = left;
        }
    }
}
