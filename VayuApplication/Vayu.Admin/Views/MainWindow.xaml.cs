
using System;
using System.Windows;
using Vayu.Admin.Model;
using Vayu.Admin.ViewModels;

namespace Vayu.Admin.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //this.DataContext = new MainWindowViewModel();
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
        private void dgAdminData_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                var dataContext = this.DataContext as MainWindowViewModel;

                if (this.DataContext != null)
                {
                    var items = dgAdminData.SelectedItem as AdminData;
                    dataContext.AssignData(items);
                }
            }
            catch (Exception)
            {
            }
        }
    }

}
