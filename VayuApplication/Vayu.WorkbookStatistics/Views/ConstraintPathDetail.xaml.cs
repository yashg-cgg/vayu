
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Vayu.WorkbookStatistics.ViewModels;

namespace Vayu.WorkbookStatistics.Views
{
    /// <summary>
    /// Interaction logic for ConstraintPathDetail.xaml
    /// </summary>
    public partial class ConstraintPathDetail : Window
    {
        public ConstraintPathDetail()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Handles the CellEditEnding event of the grdPathList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridCellEditEndingEventArgs"/> instance containing the event data.</param>
        private void grdPathList_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            WorkbookStatistics.ViewModels.Path editedRow = e.Row.DataContext as WorkbookStatistics.ViewModels.Path;
            var dx = this.DataContext as ConstraintPathDetailViewModel;
            if (dx != null)
            {
                try
                {
                    if (e.Column.Header.Equals("Price"))
                    {
                        var column = (DataGridColumn)grdPathList.Columns.Where(a => a.Header.Equals("Price")).FirstOrDefault();
                        FrameworkElement element = column.GetCellContent(e.Row);
                        dx.UpdatePath(editedRow, "Price", ((TextBox)element).Text);
                    }
                    else if (e.Column.Header.Equals("MW"))
                    {
                        var column = (DataGridColumn)grdPathList.Columns.Where(a => a.Header.Equals("MW")).FirstOrDefault();
                        FrameworkElement element = column.GetCellContent(e.Row);
                        dx.UpdatePath(editedRow, "MW", ((TextBox)element).Text);
                    }
                    else if (e.Column.Header.Equals("AnalysisType"))
                    {
                        var column = (DataGridColumn)grdPathList.Columns.Where(a => a.Header.Equals("AnalysisType")).FirstOrDefault();
                        FrameworkElement element = column.GetCellContent(e.Row);
                        dx.UpdatePath(editedRow, "AnalysisType", ((TextBox)element).Text);
                    }
                }
                catch
                {
                }
            }
        }
        /// <summary>
        /// Handles the 1 event of the grdPathList_SelectionChanged control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void grdPathList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var dx = this.DataContext as ConstraintPathDetailViewModel;
            if (dx != null)
            {
                if (grdPathList.SelectedItems.Count > 0)
                {
                    List<Path> pathSelectedList = new List<Path>();
                    foreach (Path item in grdPathList.SelectedItems)
                    {
                        if (item != null)
                        {
                            if (!pathSelectedList.Contains(item))
                                pathSelectedList.Add(item);
                        }
                    }
                    dx.PathSelectedList = pathSelectedList.ToList();
                }
            }
        }
    }
}
