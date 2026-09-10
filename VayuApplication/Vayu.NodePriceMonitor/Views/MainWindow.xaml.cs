
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Vayu.NodePriceMonitor.Model;
using Vayu.NodePriceMonitor.ViewModels;

namespace Vayu.NodePriceMonitor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Declaration

        /// <summary>
        /// The is update
        /// </summary>
        private bool isUpdate = false;
        /// <summary>
        /// The m selectedcell
        /// </summary>
        private int[] mSelectedcell = new int[] { 0, 0 };
        /// <summary>
        /// The m keyp pess identifier
        /// </summary>
        private int mKeypPessId = 0;
        /// <summary>
        /// The m manual
        /// </summary>
        private const int mManual = 13;
        /// <summary>
        /// The m begin edit
        /// </summary>
        private bool mBeginEdit = false;
        /// <summary>
        /// The m enter
        /// </summary>
        private bool mEnter = false;
        /// <summary>
        /// The m last column
        /// </summary>
        private int mLastColumn = -1;
        /// <summary>
        /// The m column
        /// </summary>
        private int mColumn;
        /// <summary>
        /// The m row
        /// </summary>
        private DataGridRow mRow = null;
        /// <summary>
        /// The m is first tab
        /// </summary>
        private bool mIsFirstTab = true;
        /// <summary>
        /// The m is enter
        /// </summary>
        private bool mIsEnter = false;

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            lmpDataGrid.PreviewKeyDown += lmpDataGrid_PreviewKeyDown;
        }

        #region Events

        /// <summary>
        /// Handles the PreviewKeyDown event of the lmpDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        void lmpDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                lmpDataGrid.CancelEdit();
                MainWindowViewModel dx = DataContext as MainWindowViewModel;
                if (lmpDataGrid.SelectedCells != null && lmpDataGrid.SelectedCells.Count > 0)
                {

                    HourlyLMP selectedNodePrice = lmpDataGrid.SelectedCells[0].Item as HourlyLMP;
                    if (selectedNodePrice != null && dx.NodePricePriceList != null)
                    {
                        dx.NodePricePriceList.ForEach(a =>
                        {
                            if (a.Hour == selectedNodePrice.Hour)
                            {
                                PropertyInfo prop = a.GetType().GetProperty(lmpDataGrid.SelectedCells[0].Column.SortMemberPath);
                                if (prop != null)
                                {
                                    prop.SetValue(a, null);
                                }
                                a.RT = null;
                            }
                        });
                        dx.mCacheNodePricePriceList = dx.NodePricePriceList.ToList();
                        dx.RefreshNodePrice();
                        lmpDataGrid.Items.Refresh();
                        lmpDataGrid.Focus();
                    }
                }
            }
        }
        /// <summary>
        /// Handles the KeyDown event of the lmpDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void lmpDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                lmpDataGrid.CommitEdit();
                MainWindowViewModel lmpmonitorvm = DataContext as MainWindowViewModel;
                lmpmonitorvm.RefreshNodePrice();
                lmpDataGrid.Items.Refresh();
                lmpDataGrid.Focus();
                DataGridCellInfo cellinfo = new DataGridCellInfo(lmpDataGrid.Items[mSelectedcell[0]], lmpDataGrid.Columns[mSelectedcell[1]]);
                lmpDataGrid.ScrollIntoView(cellinfo);
                lmpDataGrid.CurrentCell = cellinfo;
            }
            else if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                lmpDataGrid.CancelEdit();
                MainWindowViewModel dx = DataContext as MainWindowViewModel;
                if (lmpDataGrid.SelectedCells != null && lmpDataGrid.SelectedCells.Count > 0)
                {
                    if (lmpDataGrid.SelectedCells[0].Column.Header.Equals("RT"))
                    {
                        HourlyLMP selectedNodePrice = lmpDataGrid.SelectedCells[0].Item as HourlyLMP;
                        if (selectedNodePrice != null && dx.NodePricePriceList != null)
                        {
                            dx.NodePricePriceList.ForEach(a =>
                            {
                                if (a.Hour == selectedNodePrice.Hour)
                                {
                                    a.RT = null;
                                }
                            });
                            lmpDataGrid.Items.Refresh();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Handles the CellEditEnding event of the lmpDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridCellEditEndingEventArgs"/> instance containing the event data.</param>
        private void lmpDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (mIsEnter)
                {
                    mBeginEdit = false;
                    mLastColumn = -1;
                    return;
                }
                bool isImaginary = false;
                if (e != null)
                {
                    mColumn = e.Column.DisplayIndex;
                    mRow = e.Row;
                    isImaginary = true;
                }
                if (mColumn <= mLastColumn)
                {
                    mColumn = ++mLastColumn;
                }
                mSelectedcell[1] = mColumn;
                int header = SetColumn(mColumn);
                FrameworkElement element = lmpDataGrid.Columns[mColumn].GetCellContent(mRow);
                HourlyLMP hourlmp = mRow.Item as HourlyLMP;
                int hour = hourlmp.Hour;
                string value = null;
                if (element.GetType().Name.Equals("TextBlock"))
                {
                    value = ((TextBlock)element).Text;
                }
                else
                {
                    value = ((TextBox)element).Text;
                }
                if (value != null && value != "")
                {
                    MainWindowViewModel lmpmonitorvm = DataContext as MainWindowViewModel;
                    isUpdate = lmpmonitorvm.AddImaginaryValue(hour, header, value, isImaginary);
                }
                if (mColumn == 12)
                {
                    mSelectedcell[0]++;
                    mColumn = 0;
                    mSelectedcell[1] = 0;
                    mLastColumn = -1;
                }
                if (mColumn < 13)
                {
                    lmpDataGrid.ScrollIntoView(lmpDataGrid.Items[mSelectedcell[0]]);
                    DataGridRow row = lmpDataGrid.ItemContainerGenerator.ContainerFromIndex(mSelectedcell[0]) as DataGridRow;
                    if (row != null)
                    {
                        mRow = row;
                        Vayu.CommonControls.DataGridHelper helper = new Vayu.CommonControls.DataGridHelper();
                        DataGridCell cell = helper.GetCell(lmpDataGrid, row, mSelectedcell[1] + 1);
                        if (cell != null)
                        {
                            cell.Focus();
                            lmpDataGrid.BeginEdit();
                            mBeginEdit = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// Handles the KeyUp event of the lmpDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void lmpDataGrid_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                mIsEnter = false;
                if (e.Key == Key.Tab && mBeginEdit)
                {
                    if (mIsFirstTab)
                    {
                        mIsFirstTab = false;
                        return;
                    }
                    mIsFirstTab = true;
                    mBeginEdit = false;
                    lmpDataGrid.CommitEdit();
                    mLastColumn = mColumn;
                    lmpDataGrid_CellEditEnding(null, null);
                }
                if (e.Key == Key.Return && mBeginEdit)
                {
                    mIsEnter = true;
                    lmpDataGrid.CommitEdit();
                    lmpDataGrid_CellEditEnding(null, null);
                }
                if (e.Key == Key.Delete)
                {
                    if (lmpDataGrid.SelectedCells.Count > 0)
                    {
                        string itemdelete = string.Empty;
                        var item = lmpDataGrid.SelectedCells[0];
                        Int32 columnHeader = SetColumn(item.Column.DisplayIndex);
                        if (columnHeader != -1 || item.Column.DisplayIndex != mManual)
                        {
                            HourlyLMP row = (HourlyLMP)item.Item;
                            if (row.HourShow.Equals("Avg"))
                            {
                                return;
                            }
                            itemdelete = row.Hour + ":" + (columnHeader != -1 ? columnHeader.ToString() : "RT");
                            if (item.Column.GetCellContent(item.Item) != null)
                            {
                                if (item.Column.GetCellContent(item.Item).GetType().Name.Equals("TextBlock"))
                                {
                                    TextBlock minutetextbox = item.Column.GetCellContent(item.Item) as TextBlock;
                                    minutetextbox.Text = string.Empty;
                                }
                            }
                            if (lmpDataGrid.Items.Count < 24)
                            {
                                mSelectedcell[0] = row.HourTypeIndex;
                            }
                            else
                            {
                                mSelectedcell[0] = row.Hour - 1;
                            }
                            if (item.Column.DisplayIndex != mManual)
                            {
                                mSelectedcell[1] = item.Column.DisplayIndex;
                                //columnHeader != -1 ? columnHeader : 1;
                            }
                            else
                            {
                                mSelectedcell[1] = mManual;
                            }
                        }
                        MainWindowViewModel lmpmonitorvm = DataContext as MainWindowViewModel;
                        bool result = lmpmonitorvm.DeleteImaginaryValue(itemdelete);
                        DataGridCellInfo cellinfo = new DataGridCellInfo(lmpDataGrid.Items[mSelectedcell[0]], lmpDataGrid.Columns[mSelectedcell[1]]);
                        lmpDataGrid.CurrentCell = cellinfo;
                        lmpDataGrid.ScrollIntoView(cellinfo);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Handles the MouseUp event of the lmpDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void lmpDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mLastColumn = -1;
            if (lmpDataGrid.SelectedCells.Count > 0)
            {
                HourlyLMP rowindex = (HourlyLMP)lmpDataGrid.SelectedCells[0].Item;
                if (lmpDataGrid.Items.Count < 24)
                {
                    mSelectedcell[0] = rowindex.HourTypeIndex;
                }
                else
                {
                    mSelectedcell[0] = rowindex.Hour - 1;
                }
                mSelectedcell[1] = lmpDataGrid.SelectedCells[0].Column.DisplayIndex;
            }
        }

        /// <summary>
        /// Handles the SelectedCellsChanged event of the lmpDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectedCellsChangedEventArgs"/> instance containing the event data.</param>
        private void lmpDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            //foreach (var item in lmpDataGrid.SelectedCells)
            //{
            //    HourlyLMP rowValue = (HourlyLMP)item.Item;
            //    string column = item.Column.Header.ToString();
            //    if (true)
            //    {

            //    }
            //}
        }

        #endregion
        /// <summary>
        /// Sets the positions.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <param name="top">The top.</param>
        /// <param name="left">The left.</param>
        public void SetPositions(int height, int width, int top, int left)
        {
            this.Height = height;
            this.Width = width;
            this.Top = top;
            this.Left = left;
        }

        /// <summary>
        /// Sets the column.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private int SetColumn(int index)
        {
            int cellHeaderTest = 100;
            switch (index)
            {
                case 1:
                    cellHeaderTest = 0;
                    break;
                case 2:
                    cellHeaderTest = 5;
                    break;
                case 3:
                    cellHeaderTest = 10;
                    break;
                case 4:
                    cellHeaderTest = 15;
                    break;
                case 5:
                    cellHeaderTest = 20;
                    break;
                case 6:
                    cellHeaderTest = 25;
                    break;
                case 7:
                    cellHeaderTest = 30;
                    break;
                case 8:
                    cellHeaderTest = 35;
                    break;
                case 9:
                    cellHeaderTest = 40;
                    break;
                case 10:
                    cellHeaderTest = 45;
                    break;
                case 11:
                    cellHeaderTest = 50;
                    break;
                case 12:
                    cellHeaderTest = 55;
                    break;
            }
            return cellHeaderTest;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class DataGridCellBackColorConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding" /> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty" />.<see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding" />.<see cref="F:System.Windows.Data.Binding.DoNothing" /> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Windows.Media.SolidColorBrush mybrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray);
            try
            {
                DataGridCell cell = null;
                int cellindex = -1;
                Int32 cellHeaderTest = -1;
                int marketkey = 0;
                if (values[3] != DependencyProperty.UnsetValue && values[3] != null)
                {
                    marketkey = (int)values[3];
                }
                if (marketkey <= 0)
                {
                    return mybrush;
                }
                if (!values[0].Equals(""))
                {
                    cell = (DataGridCell)values[0];
                    cellindex = cell.Column.DisplayIndex;
                }
                switch (cellindex)
                {
                    case 1:
                        cellHeaderTest = 0;
                        break;
                    case 2:
                        cellHeaderTest = 5;
                        break;
                    case 3:
                        cellHeaderTest = 10;
                        break;
                    case 4:
                        cellHeaderTest = 15;
                        break;
                    case 5:
                        cellHeaderTest = 20;
                        break;
                    case 6:
                        cellHeaderTest = 25;
                        break;
                    case 7:
                        cellHeaderTest = 30;
                        break;
                    case 8:
                        cellHeaderTest = 35;
                        break;
                    case 9:
                        cellHeaderTest = 40;
                        break;
                    case 10:
                        cellHeaderTest = 45;
                        break;
                    case 11:
                        cellHeaderTest = 50;
                        break;
                    case 12:
                        cellHeaderTest = 55;
                        break;
                }
                HourlyLMP lmp = null;
                if (values[1] != DependencyProperty.UnsetValue)
                {
                    lmp = (HourlyLMP)values[1];
                }
                string textValue = string.Empty;
                if (values[2] != DependencyProperty.UnsetValue)
                {
                    textValue = (string)values[2];
                }
                if (cellHeaderTest > -1)
                {
                    if (lmp.HourShow.Equals("Avg"))
                    {
                        mybrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray);
                        return mybrush;
                    }
                    if (textValue.Equals(""))
                    {
                        mybrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Purple);
                    }
                    else if (lmp.ExanteDispatch != null && lmp.ExanteDispatch.Contains(cellHeaderTest))
                    {
                        mybrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.ForestGreen);
                    }
                    else if ((lmp.ImaginaryValue != null && lmp.ImaginaryValue.Contains(cellHeaderTest)))
                    {
                        mybrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.SaddleBrown);
                    }
                    else
                    {
                        mybrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    }
                }
                if (marketkey == 9 && lmp != null)
                {
                    if (cellHeaderTest == 10 || cellHeaderTest == 25 || cellHeaderTest == 40 || cellHeaderTest == 55)
                    {
                        if (cellHeaderTest + 5 >= DateTime.Now.Minute && lmp.Hour > DateTime.Now.Hour && values[2] != "")
                        {
                            mybrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                        }
                    }
                }
                return mybrush;
            }
            catch (Exception ex)
            {
                return mybrush;
            }
        }
        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
