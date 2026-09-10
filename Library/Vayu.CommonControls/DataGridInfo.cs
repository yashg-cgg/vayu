using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Vayu.CommonControls
{
    /// <summary>
    /// 
    /// </summary>
    public static class DataGridInfo
    {
        /// <summary>
        /// Gets the cell.
        /// </summary>
        /// <param name="dataGridCellInfo">The data grid cell information.</param>
        /// <returns></returns>
        public static DataGridCell GetCell(DataGridCellInfo dataGridCellInfo)
        {
            if (!dataGridCellInfo.IsValid)
            {
                return null;
            }

            var cellContent = dataGridCellInfo.Column.GetCellContent(dataGridCellInfo.Item);
            if (cellContent != null)
            {
                return (DataGridCell)cellContent.Parent;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the index of the row.
        /// </summary>
        /// <param name="dataGridCell">The data grid cell.</param>
        /// <returns></returns>
        public static int GetRowIndex(DataGridCell dataGridCell)
        {
            if (dataGridCell == null)
            {
                return -1;
            }
            PropertyInfo rowDataItemProperty = dataGridCell.GetType().GetProperty("RowDataItem", BindingFlags.Instance | BindingFlags.NonPublic);
            DataGrid dataGrid = GetDataGridFromChild(dataGridCell);
            return dataGrid.Items.IndexOf(rowDataItemProperty.GetValue(dataGridCell, null));
        }

        /// <summary>
        /// Gets the data grid from child.
        /// </summary>
        /// <param name="dataGridPart">The data grid part.</param>
        /// <returns></returns>
        /// <exception cref="System.NullReferenceException">Control is null.</exception>
        public static DataGrid GetDataGridFromChild(DependencyObject dataGridPart)
        {
            if (VisualTreeHelper.GetParent(dataGridPart) == null)
            {
                throw new NullReferenceException("Control is null.");
            }
            if (VisualTreeHelper.GetParent(dataGridPart) is DataGrid)
            {
                return (DataGrid)VisualTreeHelper.GetParent(dataGridPart);
            }
            else
            {
                return GetDataGridFromChild(VisualTreeHelper.GetParent(dataGridPart));
            }
        }
    }
}
