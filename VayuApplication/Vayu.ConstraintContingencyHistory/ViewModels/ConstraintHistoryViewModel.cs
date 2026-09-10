using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Vayu.ConstraintContingencyHistory.Model;

namespace Vayu.ConstraintContingencyHistory.ViewModels
{
    public class ConstraintHistoryViewModel : BindableBase
    {
        public DelegateCommand ExporttoExcelCommand { private set; get; }
        private List<Constraint> mHistoryConstraintList;
        public List<Constraint> HistoryConstraintList
        {
            get { return mHistoryConstraintList; }
            set
            {
                mHistoryConstraintList = value;
                RaisePropertyChanged("HistoryConstraintList");
            }
        }
        private string mContengencyItem;
        public string ContengencyItem
        {
            get { return mContengencyItem; }
            set
            {
                mContengencyItem = value;
                RaisePropertyChanged("ContengencyItem");
            }
        }
        private string mConstraintItem;
        public string ConstraintItem
        {
            get { return mConstraintItem; }
            set
            {
                mConstraintItem = value;
                RaisePropertyChanged("ConstraintItem");
            }
        }
        public ConstraintHistoryViewModel(MainWindowViewModel parentModel, List<Constraint> histconstList, string constraint, string contingency)
        {
            ExporttoExcelCommand = new DelegateCommand(() => ExportExcel());
            ConstraintItem = constraint;
            ContengencyItem = contingency;
            HistoryConstraintList = histconstList.ToList();
            mParentModel = parentModel;
        }
        public void ExportExcel()
        {
            {
                if (HistoryConstraintList == null)
                {
                    Mouse.OverrideCursor = null;
                    return;
                }
                if (HistoryConstraintList == null || HistoryConstraintList.Count == 0)
                {
                    System.Windows.MessageBox.Show("No data to export to");
                    return;
                }
                SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
                dialog.FileName = "ConstraintHistory_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
                if ((bool)dialog.ShowDialog())
                {
                    if (dialog.FileName != "")
                    {
                        if (HistoryConstraintList != null && HistoryConstraintList.Count > 0)
                        {
                            StringBuilder builder = new StringBuilder();
                            foreach (var item in HistoryConstraintList[0].GetType().GetProperties())
                            {
                                if (item.Name == "ConstraintDate" || item.Name == "ConstraintText" || item.Name == "MonitoredFacility" || item.Name == "ContingencyText"
                                    || item.Name == "Avg" || item.Name == "HE1" || item.Name == "HE2" || item.Name == "HE3" || item.Name == "HE4" || item.Name == "HE5"
                                    || item.Name == "HE6" || item.Name == "HE7" || item.Name == "HE8" || item.Name == "HE9" || item.Name == "HE10" || item.Name == "HE11"
                                    || item.Name == "HE12" || item.Name == "HE13" || item.Name == "HE14" || item.Name == "HE15" || item.Name == "HE16" || item.Name == "HE17"
                                    || item.Name == "HE18" || item.Name == "HE19" || item.Name == "HE20" || item.Name == "HE21" || item.Name == "HE22" || item.Name == "HE23" || item.Name == "HE24")
                                {
                                    builder.Append(item.Name + ",");
                                }
                            }
                            builder.AppendLine();
                            foreach (var item in HistoryConstraintList)
                            {
                                foreach (var propItem in item.GetType().GetProperties())
                                {
                                    if (propItem.Name == "ConstraintDate" || propItem.Name == "ConstraintText" || propItem.Name == "MonitoredFacility" || propItem.Name == "ContingencyText"
                                    || propItem.Name == "Price" || propItem.Name == "HE1" || propItem.Name == "HE2" || propItem.Name == "HE3" || propItem.Name == "HE4" || propItem.Name == "HE5"
                                    || propItem.Name == "HE6" || propItem.Name == "HE7" || propItem.Name == "HE8" || propItem.Name == "HE9" || propItem.Name == "HE10" || propItem.Name == "HE11"
                                    || propItem.Name == "HE12" || propItem.Name == "HE13" || propItem.Name == "HE14" || propItem.Name == "HE15" || propItem.Name == "HE16" || propItem.Name == "HE17"
                                    || propItem.Name == "HE18" || propItem.Name == "HE19" || propItem.Name == "HE20" || propItem.Name == "HE21" || propItem.Name == "HE22" || propItem.Name == "HE23" || propItem.Name == "HE24")
                                    {
                                        if (propItem.Name == "Price" || propItem.Name == "HE1" || propItem.Name == "HE2" || propItem.Name == "HE3" || propItem.Name == "HE4" || propItem.Name == "HE5"
                                        || propItem.Name == "HE6" || propItem.Name == "HE7" || propItem.Name == "HE8" || propItem.Name == "HE9" || propItem.Name == "HE10" || propItem.Name == "HE11"
                                        || propItem.Name == "HE12" || propItem.Name == "HE13" || propItem.Name == "HE14" || propItem.Name == "HE15" || propItem.Name == "HE16" || propItem.Name == "HE17"
                                        || propItem.Name == "HE18" || propItem.Name == "HE19" || propItem.Name == "HE20" || propItem.Name == "HE21" || propItem.Name == "HE22" || propItem.Name == "HE23" || propItem.Name == "HE24")
                                        {
                                            object st = propItem.GetValue(item);
                                            double s = Convert.ToDouble(st);
                                            double val = Math.Round(s, 2);
                                            string g = "";
                                            if (val == 0.0)
                                            {
                                                g = val.ToString();
                                                g = "";
                                                builder.Append(g + ",");
                                            }
                                            else
                                            {
                                                builder.Append(val + ",");
                                            }

                                        }
                                        else
                                        {
                                            builder.Append(propItem.GetValue(item) + ",");
                                        }
                                    }
                                }
                                builder.AppendLine();
                            }
                            if (builder.Length > 0)
                            {
                                using (TextWriter str = new StreamWriter(dialog.FileName, false))
                                {
                                    str.Write(builder.ToString());
                                    str.Flush();
                                    str.Close();
                                    str.Dispose();
                                }
                                if (File.Exists(dialog.FileName))
                                {
                                    System.Windows.MessageBox.Show("Successfully saved the file " + dialog.FileName);
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("Couldnot save the file");
                                }
                            }
                        }
                    }
                }
            }
        }


        private MainWindowViewModel mParentModel;
        public MainWindowViewModel ParentModel
        {
            get { return mParentModel; }
            set { mParentModel = value; }
        }
    }
}
