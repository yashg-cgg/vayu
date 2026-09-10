using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Vayu.ConstraintContingencyHistory.ViewModels
{

    public class UptosNodePriceViewModel : BindableBase
    {
        private List<Model.NodePriceHelper> _NodePriceList;

        public List<Model.NodePriceHelper> NodePriceList
        {
            get { return _NodePriceList; }
            set
            {
                _NodePriceList = value;
                RaisePropertyChanged("NodePriceList");
            }
        }
        public DelegateCommand Export5MinLMPsToExcel { get; set; }

        public UptosNodePriceViewModel()
        {
            Export5MinLMPsToExcel = new DelegateCommand(() => ExortFiveMinsPricesToExcel());
        }

        private void ExortFiveMinsPricesToExcel()
        {
            if (NodePriceList != null && NodePriceList.Count > 0)
            {
                SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
                dialog.FileName = "FiveMinsPrices" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
                if ((bool)dialog.ShowDialog())
                {
                    if (dialog.FileName != "")
                    {
                        {
                            StringBuilder builder = new StringBuilder();
                            foreach (var item in NodePriceList[0].GetType().GetProperties())
                            {
                                if (item.Name == "NodeName" || item.Name == "MktDateTime" || item.Name == "LMP" || item.Name == "Congestion"
                                    || item.Name == "Loss" || item.Name == "Zone")
                                {
                                    builder.Append(item.Name + ",");
                                }

                            }
                            builder.AppendLine();
                            int lineCount = 1;
                            foreach (var item in NodePriceList)
                            {
                                lineCount++;
                                foreach (var propItem in item.GetType().GetProperties())
                                {
                                    if (propItem.Name == "NodeName" || propItem.Name == "MktDateTime" || propItem.Name == "LMP" || propItem.Name == "Congestion"
                                    || propItem.Name == "Loss" || propItem.Name == "Zone")
                                    {
                                        {

                                            string s = string.Empty;
                                            object st = propItem.GetValue(item) == null ? "NA " : propItem.GetValue(item);
                                            try
                                            {
                                                s = st.ToString() == null ? " " : st.ToString();
                                            }
                                            catch (Exception ex)
                                            {

                                                System.Windows.MessageBox.Show(lineCount + '\t' + ex.Message);
                                            }
                                            string val = s;
                                            string g = "";
                                            {
                                                builder.Append(val + ",");
                                            }

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
    }

}
