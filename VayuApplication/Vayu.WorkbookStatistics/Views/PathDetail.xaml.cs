
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Vayu.WorkbookStatistics.Model;
using Vayu.WorkbookStatistics.ViewModels;

namespace Vayu.WorkbookStatistics.Views
{
    /// <summary>
    /// Interaction logic for PathDetail.xaml
    /// </summary>
    public partial class PathDetail : Window
    {
        public PathDetail()
        {
            InitializeComponent();
        }
        private void ButtonClickScale(object sender, RoutedEventArgs e)
        {
            var cellinfos = grdPathConstraintDataList.SelectedCells;
            MainWindowViewModel dx = ((Vayu.WorkbookStatistics.ViewModels.PathDetailViewModel)(DataContext)).mParentModel;
            foreach (DataGridCellInfo cells in cellinfos)
            {

                Exposure item = cells.Item as Exposure;
                string columnName = cells.Column.SortMemberPath;
                string[] hrs = columnName.Split(new string[] { "HE" }, StringSplitOptions.None);
                int hr = int.Parse(hrs[1]);
                if (item == null || columnName == null)
                {

                }
                object result = item.GetType().GetProperty(columnName).GetValue(item, null);
                string source = item.Constraint;
                string sink = item.Contingency;
                if (result != null)
                {

                    //string source = data.Constraint;
                    //string sink = data.Contingency;
                    DataService mDataService = new DataService();
                    DateTime Date = dx.PortfolioDate.AddHours(hr);
                    if (dx.PathList != null)
                    {
                        string status = dx.PathList.FirstOrDefault().Status;
                        if (status == "IMPORTED")
                        {
                            if (((Vayu.WorkbookStatistics.ViewModels.PathDetailViewModel)(DataContext)).TextScale1 != null)
                            {
                                double num = Convert.ToDouble(((Vayu.WorkbookStatistics.ViewModels.PathDetailViewModel)(DataContext)).TextScale1);
                                if (num > 0)
                                {
                                    if (dx.TraderPortfolioComboSelectedItem == null)
                                    {
                                        //mDataService.UpdateScaleNumberSourceSinkHRS(num, dx.PortfolioComboSelectedItem, dx.ImportDate, dx.ImportDate.AddDays(1), dx.stMarket, source, sink);
                                        mDataService.UpdateScaleNumberSourceSinkHRS(num, dx.PortfolioComboSelectedItem, Date, source, sink);
                                        //dx.Retrieve();
                                        //MessageBox.Show("Updated MWs");
                                    }
                                    else
                                    {
                                        mDataService.UpdateScaleNumberSourceSinkHRS(num, dx.TraderPortfolioComboSelectedItem, Date, source, sink);
                                        //dx.Retrieve();
                                        //MessageBox.Show("Updated MWs");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Scaling Number should be more than ZERO.");
                                }
                            }
                            else
                                MessageBox.Show("Please enter Scaling Number.");
                        }
                        else
                        {
                            MessageBox.Show("Status of the Portfolio should be IMPORTED.");
                        }

                    }
                    else
                    {
                        MessageBox.Show("Please Import the file.");
                    }
                }
                //else
                //{
                //    MessageBox.Show("Please Select Path.");
                //}
            }

            //            Exposure data = (Exposure)grdPathConstraintDataList.SelectedItem;
            dx.Retrieve();
            MessageBox.Show("Updated MWs");



        }
    }
}
