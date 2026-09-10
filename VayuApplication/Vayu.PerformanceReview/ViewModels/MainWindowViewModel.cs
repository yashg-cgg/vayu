using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Vayu.PerformanceReview.Model;
namespace Vayu.PerformanceReview.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDataService _dataService;
        private DateTime today = DateTime.Today;
        public DelegateCommand RunRefreshommand { private set; get; }
        public DelegateCommand RunExportCSVCommand { private set; get; }
        List<Performance> finalDetails = new List<Performance>();
        private bool mERCOTChecked;
        public bool ERCOTChecked
        {
            get
            {
                return mERCOTChecked;
            }
            set
            {
                mERCOTChecked = value;
                RaisePropertyChanged("ERCOTChecked");
                //Refresh();
            }
        }

        private double? mClientPNL;
        public double? ClientPNL
        {
            get
            {
                return mClientPNL;
            }
            set
            {
                mClientPNL = value;
                RaisePropertyChanged("ClientPNL");
            }
        }
        private double? mClientFee;
        public double? ClientFee
        {
            get
            {
                return mClientFee;
            }
            set
            {
                mClientFee = value;
                RaisePropertyChanged("ClientFee");
            }
        }
        private double? mClientNetPNL;
        public double? ClientNetPNL
        {
            get
            {
                return mClientNetPNL;
            }
            set
            {
                mClientNetPNL = value;
                RaisePropertyChanged("ClientNetPNL");
            }
        }
        private double? mClientMWs;
        public double? ClientMWs
        {
            get
            {
                return mClientMWs;
            }
            set
            {
                mClientMWs = value;
                RaisePropertyChanged("ClientMWs");
            }
        }

        private double? mQuantMWs;
        public double? QuantMWs
        {
            get
            {
                return mQuantMWs;
            }
            set
            {
                mQuantMWs = value;
                RaisePropertyChanged("QuantMWs");
            }
        }

        private double? mQuantNetPNL;
        public double? QuantNetPNL
        {
            get
            {
                return mQuantNetPNL;
            }
            set
            {
                mQuantNetPNL = value;
                RaisePropertyChanged("QuantNetPNL");
            }
        }

        private double? mCurrentISO;
        public double? CurrentISO
        {
            get
            {
                return mCurrentISO;
            }
            set
            {
                mCurrentISO = value;
                RaisePropertyChanged("CurrentISO");
            }
        }

        private double? mQuantPNL;
        public double? QuantPNL
        {
            get
            {
                return mQuantPNL;
            }
            set
            {
                mQuantPNL = value;
                RaisePropertyChanged("QuantPNL");
            }
        }

        private double? mQuantFee;
        public double? QuantFee
        {
            get
            {
                return mQuantFee;
            }
            set
            {
                mQuantFee = value;
                RaisePropertyChanged("QuantFee");
            }
        }
        private List<Performance> mPNLDailyList;
        public List<Performance> PNLDailyList
        {
            get
            {
                return mPNLDailyList;
            }
            set
            {
                mPNLDailyList = value;
                RaisePropertyChanged("PNLDailyList");
            }
        }
        private List<Performance> mPNLDailyListAll;
        public List<Performance> PNLDailyListAll
        {
            get
            {
                return mPNLDailyListAll;
            }
            set
            {
                mPNLDailyListAll = value;
                RaisePropertyChanged("PNLDailyListAll");
            }
        }


        private List<string> mProductList;
        public List<string> ProductList
        {
            get
            {
                return mProductList;
            }
            set
            {
                mProductList = value;
                GetAllData();
                RaisePropertyChanged("ProductList");
            }
        }
        private List<string> mMarketList;
        public List<string> MarketList
        {
            get
            {
                return mMarketList;
            }
            set
            {
                mMarketList = value;
                RaisePropertyChanged("MarketList");
            }
        }

        private List<int> mYearList;
        public List<int> YearList
        {
            get
            {
                return mYearList;
            }
            set
            {
                mYearList = value;
                RaisePropertyChanged("YearList");
            }
        }
        private List<string> mTermList;
        public List<string> TermList
        {
            get
            {
                return mTermList;
            }
            set
            {
                mTermList = value;
                RaisePropertyChanged("TermList");
            }
        }

        private string mSelectedProduct = "PTP";
        public string SelectedProduct
        {
            get
            {
                return mSelectedProduct;
            }
            set
            {
                mSelectedProduct = value;
                GetAllData();
                RaisePropertyChanged("SelectedProduct");
            }
        }
        private string mSelectedMarket = "ERCOT";
        public string SelectedMarket
        {
            get
            {
                return mSelectedMarket;
            }
            set
            {
                mSelectedMarket = value;
                GetAllData();
                RaisePropertyChanged("SelectedMarket");
            }
        }
        private int mSelectedYear = Convert.ToInt32(DateTime.Now.Year);
        public int SelectedYear
        {
            get
            {
                return mSelectedYear;
            }
            set
            {
                mSelectedYear = value;
                GetAllData();
                RaisePropertyChanged("SelectedYear");
            }
        }
        private string mSelectedTerm;
        public string SelectedTerm
        {
            get
            {
                return mSelectedTerm;
            }
            set
            {
                mSelectedTerm = value;
                {
                    GetAllData();
                }
                RaisePropertyChanged("SelectedTerm");
            }
        }
        private double? miTotalPNL;
        public double? iTotalPNL
        {
            get
            {
                return miTotalPNL;
            }
            set
            {
                miTotalPNL = value;
                RaisePropertyChanged("iTotalPNL");
            }
        }

        private double? miClientFee;
        public double? iClientFee
        {
            get
            {
                return miClientFee;
            }
            set
            {
                miClientFee = value;
                RaisePropertyChanged("iClientFee");
            }
        }
        private double? miClientNetPNL;
        public double? iClientNetPNL
        {
            get
            {
                return miClientNetPNL;
            }
            set
            {
                miClientNetPNL = value;
                RaisePropertyChanged("iClientNetPNL");
            }
        }
        private double? mCompanyPNL;
        public double? CompanyPNL
        {
            get
            {
                return mCompanyPNL;
            }
            set
            {
                mCompanyPNL = value;
                RaisePropertyChanged("CompanyPNL");
            }
        }

        private double? mCompanyFee;
        public double? CompanyFee
        {
            get
            {
                return mCompanyFee;
            }
            set
            {
                mCompanyFee = value;
                RaisePropertyChanged("CompanyFee");
            }
        }
        private double? mCompanyNet;
        public double? CompanyNet
        {
            get
            {
                return mCompanyNet;
            }
            set
            {
                mCompanyNet = value;
                RaisePropertyChanged("CompanyNet");
            }
        }

        private double? mCompanyMWs;
        public double? CompanyMWs
        {
            get
            {
                return mCompanyMWs;
            }
            set
            {
                mCompanyMWs = value;
                RaisePropertyChanged("CompanyMWs");
            }
        }

        private double? mYTDISO;
        public double? YTDISO
        {
            get
            {
                return mYTDISO;
            }
            set
            {
                mYTDISO = value;
                RaisePropertyChanged("YTDISO");
            }
        }
        private bool mQuantChecked;
        public bool QuantChecked
        {
            get
            {
                return mQuantChecked;
            }
            set
            {
                mQuantChecked = value;
                if (value == true)
                {
                    GetAllData();
                    RefreshMtdDateClick();
                }
                RaisePropertyChanged("QuantChecked");
            }
        }
        private bool mClientChecked;
        public bool ClientChecked
        {
            get
            {
                return mClientChecked;
            }
            set
            {
                mClientChecked = value;
                if (value == true)
                {
                    GetAllData();
                    RefreshMtdDateClick();
                }
                RaisePropertyChanged("ClientChecked");
            }
        }
        private bool mCompanyChecked = true;
        public bool CompanyChecked
        {
            get
            {
                return mCompanyChecked;
            }
            set
            {
                mCompanyChecked = value;
                if (value == true)
                {
                    GetAllData();
                    RefreshMtdDateClick();
                }
                RaisePropertyChanged("CompanyChecked");
            }
        }
        public void SetMarket()
        {
            List<string> marketList = new List<string>();
            //marketList.Add("PJM");
            marketList.Add("ERCOT");
            MarketList = marketList;
        }
        public MainWindowViewModel(IDataService dataService = null)
        {
            _dataService = dataService;
            SelectedYear = DateTime.Today.Year;
            var dt = DateTime.Now;
            SetMarket();
            SetYear();
            SetTerm();
            ProductList = new List<string> { "PTP" };
            //ProductList = new List<string> { "PTP" };
            RunRefreshommand = new DelegateCommand(RefreshMtdDateClick);
            RunExportCSVCommand = new DelegateCommand(ExportCSVData);
            //Refresh();
            GetAllData();
        }

        private void SetTerm()
        {
            List<string> termlist = new List<string>();
            termlist.Add("1st-Term");
            termlist.Add("2nd-Term");
            TermList = termlist;
            if (DateTime.Now.Month <= 1 && DateTime.Now.Month >= 6)
            {
                SelectedTerm = "2nd-Term";
            }
            else
            {
                SelectedTerm = "1st-Term";
            }

        }

        private void SetYear()
        {
            List<int> yearList = new List<int>();
            for (int i = 2016; i <= DateTime.Now.Year; i++)
            {
                yearList.Add(i);
            }
            YearList = yearList;
        }
        private void incdDateClick()
        {
            try
            {
                if (SelectedMarket == null)
                {
                    MessageBox.Show("Please select market.");
                }
                else
                {
                    DisplayMTD(false);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void YesterdayDateClick()
        {
            try
            {
                if (SelectedMarket == null)
                {
                    MessageBox.Show("Please select market.");
                }
                else
                {
                    DisplayMTD(false);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void MtdDateClick()
        {
            try
            {
                if (SelectedMarket == null)
                {
                    MessageBox.Show("Please select market.");
                }
                else
                {
                    var dt = DateTime.Now;
                    DisplayMTD(true);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void RefreshMtdDateClick()
        {
            try
            {
                GetAllData();
                if (SelectedMarket == null)
                {
                    MessageBox.Show("Please select market.");
                }
                else
                {
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        private void GetAllData()
        {
            //PNLList = null;
            PNLDailyList = null;
            QuantPNL = null;
            CurrentISO = null;
            QuantFee = null;
            QuantNetPNL = null;
            QuantMWs = null;
            ClientPNL = null;
            ClientFee = null;
            ClientNetPNL = null;
            ClientMWs = null;
            CompanyPNL = null;
            CompanyFee = null;
            CompanyNet = null;
            YTDISO = null;
            iTotalPNL = null;
            iClientFee = null;
            iClientNetPNL = null;
            CompanyMWs = null;
            //if (SelectedProduct == "CRR" && SelectedMarket == "ERCOT")
            //{
            //    MessageBox.Show("Please select proper market for FTR", "InvalidProductForErcot", MessageBoxButton.OK);
            //}
            //else
            {
                int marketkey = 9;
                DateTime sdate = new DateTime();
                DateTime edate = new DateTime();
                //if (SelectedProduct == "PTP")
                {
                    //if (SelectedMarket == "PJM")
                    //{
                    //    marketkey = 1;
                    //    sdate = new DateTime(2018, 05, 01);
                    //}
                    //else 
                    if (SelectedMarket == "ERCOT")
                    {
                        if (SelectedTerm == "1st-Term")
                        {
                            sdate = new DateTime(SelectedYear, 01, 01);
                            edate = new DateTime(SelectedYear, 06, 30);
                        }
                        else
                        {
                            sdate = new DateTime(SelectedYear, 07, 01);
                            edate = new DateTime(SelectedYear, 12, 31);
                        }
                        marketkey = 9;
                    }
                    DataService ds = new DataService();
                    PNLDailyList = ds.GetReconcilationListDaily(sdate, edate, marketkey).OrderBy(x => x.DateFormat).ToList<Performance>();
                    DisplayMTD(true);
                }
                Mouse.OverrideCursor = null;
            }
        }

        private void DisplayMTD(bool ShowData)
        {
            try
            {
                List<Performance> clientDetails = new List<Performance>();

                finalDetails = PNLDailyList;

                List<Performance> companyDetails = new List<Performance>();
                companyDetails = finalDetails.ToList<Performance>();
                if (CompanyChecked)
                {
                    PNLDailyList = companyDetails;
                }
                // companyDetails = PNLDailyList.ToList<Performance>();
                if (companyDetails.Count > 0)
                {
                    CompanyPNL = companyDetails.Sum(x => x.CompanyGross);
                    CompanyFee = companyDetails.Sum(x => x.CompanyFee);
                    CompanyNet = companyDetails.Sum(x => x.CompanyNet);
                    CompanyMWs = Math.Round(Convert.ToDouble(companyDetails.Sum(x => x.CompanyMW)), 2);
                }

                clientDetails = finalDetails.Where(x => x.AccountName != "A17").ToList<Performance>();
                if (ClientChecked)
                {
                    PNLDailyList = clientDetails;
                }
                // companyDetails = PNLDailyList.ToList<Performance>();
                if (clientDetails.Count > 0)
                {
                    ClientPNL = clientDetails.Sum(x => x.CompanyGross);
                    ClientFee = clientDetails.Sum(x => x.CompanyFee);
                    ClientNetPNL = clientDetails.Sum(x => x.CompanyNet);
                    ClientMWs = Math.Round(Convert.ToDouble(clientDetails.Sum(x => x.CompanyMW)), 2);
                }

                List<Performance> quantDetails = new List<Performance>();
                quantDetails = finalDetails.Where(x => x.AccountName == "A17").ToList<Performance>();
                if (QuantChecked)
                {
                    PNLDailyList = quantDetails;
                }
                // companyDetails = PNLDailyList.ToList<Performance>();
                if (quantDetails.Count > 0)
                {
                    QuantPNL = quantDetails.Sum(x => x.CompanyGross);
                    QuantFee = quantDetails.Sum(x => x.CompanyFee);
                    QuantNetPNL = quantDetails.Sum(x => x.CompanyNet);
                    QuantMWs = Math.Round(Convert.ToDouble(quantDetails.Sum(x => x.CompanyMW)), 2);
                }


            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void ExportCSVData()
        {
            if (PNLDailyList != null)
            {
                SaveFileDialog savefiledialog = new SaveFileDialog();
                savefiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                savefiledialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                savefiledialog.FilterIndex = 1;
                savefiledialog.RestoreDirectory = true;
                savefiledialog.FileName = "Performance" + SelectedYear.ToString("00");
                if ((bool)savefiledialog.ShowDialog())
                {

                    StringBuilder builder = new StringBuilder();
                    //daily
                    foreach (PropertyInfo item in PNLDailyList[0].GetType().GetProperties())
                    {
                        builder.Append(item.Name + ",");
                    }
                    builder.ToString().Remove(builder.Length - 1, 1);
                    builder.AppendLine();
                    foreach (Performance item in PNLDailyList)
                    {
                        foreach (PropertyInfo propName in item.GetType().GetProperties())
                        {
                            builder.Append((item.GetType().GetProperty(propName.Name).GetValue(item) == null ? "" : item.GetType().GetProperty(propName.Name).GetValue(item).ToString().Replace(',', '-')) + ",");
                        }
                        builder.AppendLine();
                    }
                    try
                    {
                        TextWriter writer = new StreamWriter(savefiledialog.FileName);
                        writer.Write(builder.ToString());
                        writer.Flush();
                        writer.Close();
                        MessageBox.Show("Successfully created the file");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Unable To Create File");
                    }
                }

            }
            else
            {
                MessageBox.Show("Please Insert the Data");
            }
        }
    }
    public class ValueToForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            double doubleValue = 0.0;
            if (value != null)
            {
                double.TryParse(value.ToString(), out doubleValue);
                if (doubleValue < 0)
                {
                    brush = new SolidColorBrush(Colors.Red);
                }
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CellBackgroundColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var objItem = value as Performance;
                if (objItem != null)
                {
                    if (parameter != null)
                    {
                        switch (parameter.ToString().ToLower())
                        {
                            case "crrgross":
                                if (objItem.CRRGross < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "crrfee":
                                if (objItem.CRRFee < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "crrnet":
                                if (objItem.CRRNet < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "crrmw":
                                if (objItem.CRRMw < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "ptpgross":
                                if (objItem.PTPGross < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "ptpfee":
                                if (objItem.PTPFee < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "ptpnet":
                                if (objItem.PTPNet < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "ptpmw":
                                if (objItem.PTPMW < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "ptpdollarscleared":
                                if (objItem.PTPDollarsCleared < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "companygross":
                                if (objItem.CompanyGross < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "companyfee":
                                if (objItem.CompanyFee < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "companynet":
                                if (objItem.CompanyNet < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);
                            case "companymw":
                                if (objItem.CompanyMW < 0)
                                {
                                    return new SolidColorBrush(Colors.Red);
                                }
                                return new SolidColorBrush(Colors.Black);

                            default:
                                return new SolidColorBrush();
                        }
                    }
                    else return new SolidColorBrush(Colors.Black);
                }
                else return new SolidColorBrush(Colors.Black);
            }
            else return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
    public class CellBackgroundColorConverter1 : IValueConverter
    {

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                //var objItem = value as ReconcilationMTLY;
                //if (objItem != null)
                //{
                //    if (parameter != null)
                //    {
                //        switch (parameter.ToString().ToLower())
                //        {
                //            case "fee":
                //                if (objItem.Fee < 0)
                //                {
                //                    return new SolidColorBrush(Colors.Red);
                //                }
                //                return new SolidColorBrush(Colors.Black);
                //            case "gross":
                //                if (objItem.Gross < 0)
                //                {
                //                    return new SolidColorBrush(Colors.Red);
                //                }
                //                return new SolidColorBrush(Colors.Black);
                //            case "monthlypnlfromstatement":
                //                if (objItem.MonthlyPNLFromStatement < 0)
                //                {
                //                    return new SolidColorBrush(Colors.Red);
                //                }
                //                return new SolidColorBrush(Colors.Black);
                //            case "pnl":
                //                if (objItem.PNL < 0)
                //                {
                //                    return new SolidColorBrush(Colors.Red);
                //                }
                //                return new SolidColorBrush(Colors.Black);
                //            case "monthlyrunningtotal":
                //                if (objItem.MonthlyRunningTotal < 0)
                //                {
                //                    return new SolidColorBrush(Colors.Red);
                //                }
                //                return new SolidColorBrush(Colors.Black);
                //            case "isomiscellaneouscharges":
                //                if (objItem.ISOMiscellaneousCharges < 0)
                //                {
                //                    return new SolidColorBrush(Colors.Red);
                //                }
                //                return new SolidColorBrush(Colors.Black);
                //            case "monthgrossiso":
                //                if (objItem.ISOMiscellaneousCharges < 0)
                //                {
                //                    return new SolidColorBrush(Colors.Red);
                //                }
                //                return new SolidColorBrush(Colors.Black);
                //            case "monthfeeiso":
                //                if (objItem.ISOMiscellaneousCharges < 0)
                //                {
                //                    return new SolidColorBrush(Colors.Red);
                //                }
                //                return new SolidColorBrush(Colors.Black);
                //            case "feepermw":
                //                if (objItem.FeePerMW < 0)
                //                {
                //                    return new SolidColorBrush(Colors.Red);
                //                }
                //                return new SolidColorBrush(Colors.Black);
                //            case "rtm":
                //                if (objItem.RTM < 0)
                //                {
                //                    return new SolidColorBrush(Colors.Red);
                //                }
                //                return new SolidColorBrush(Colors.Black);
                //            case "dam":
                //                if (objItem.DAM < 0)
                //                {
                //                    return new SolidColorBrush(Colors.Red);
                //                }
                //                return new SolidColorBrush(Colors.Black);

                //            case "mws":
                //                if (objItem.MWs < 0)
                //                {
                //                    return new SolidColorBrush(Colors.Red);
                //                }
                //                return new SolidColorBrush(Colors.Black);
                //            default:
                //                return new SolidColorBrush();
                //        }
                //    }
                //    else return new SolidColorBrush(Colors.Black);
                //}
                //else

                return new SolidColorBrush(Colors.Black);
            }
            else return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class CellBackgroundColorConverter2 : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var objItem = value as Performance;
                if (objItem != null)
                {
                    if (parameter != null)
                    {
                        switch (parameter.ToString().ToLower())
                        {
                            case "dailyfee":
                                if (objItem.CRRMw == 0)
                                {
                                    return new SolidColorBrush(Colors.LightBlue);
                                }
                                return new SolidColorBrush(Colors.White);

                            default:
                                return new SolidColorBrush();
                        }
                    }
                    else return new SolidColorBrush(Colors.Black);
                }
                else return new SolidColorBrush(Colors.Black);
            }
            else return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
