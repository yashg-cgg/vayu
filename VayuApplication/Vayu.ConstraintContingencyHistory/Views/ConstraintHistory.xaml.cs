
using System;
using System.Windows;

namespace Vayu.ConstraintContingencyHistory.ViewModels
{
    /// <summary>
    /// Interaction logic for ConstraintHistory.xaml
    /// </summary>
    public partial class ConstraintHistory : Window
    {
        public ConstraintHistory()
        {
            InitializeComponent();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            ConstraintHistoryViewModel dataContext = DataContext as ConstraintHistoryViewModel;

            if (dataContext.ParentModel != null)
            {
                dataContext.ParentModel.SelectedConstraintItem = string.Empty;
                dataContext.ParentModel.SelectedContengencyItem = string.Empty;
            }
        }
    }
}
