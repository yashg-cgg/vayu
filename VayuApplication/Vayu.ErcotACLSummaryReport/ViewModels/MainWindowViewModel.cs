using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using Vayu.ErcotACLSummaryReport.Model;
using Vayu.ErcotACLSummaryReport.ViewModel;

namespace Vayu.ErcotACLSummaryReport.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDataService _dataService;
        public DelegateCommand RunRefreshommand { private set; get; }
        public DelegateCommand CLExportCommand { private set; get; }
        public DelegateCommand CRRExportCommand { private set; get; }
        public DelegateCommand DAMExportCommand { private set; get; }
        public DelegateCommand cltranExportCommand { private set; get; }
        #region Properties

        private DateTime stFromSelectedDate;
        public DateTime FromSelectedDate
        {
            get
            {
                return stFromSelectedDate;
            }
            set
            {
                if (value != null && DateRangeCheckBoxChecked && value > ThroDate)
                {
                    System.Windows.MessageBox.Show("thro' date should be greater than from date");
                    stFromSelectedDate = DateTime.Today;
                }
                else
                    stFromSelectedDate = value;

                RaisePropertyChanged("FromSelectedDate");
            }
        }

        private DateTime stThroDate = DateTime.Today;
        public DateTime ThroDate
        {
            get { return stThroDate; }
            set
            {
                if (value != null && value < FromSelectedDate)
                {
                    System.Windows.MessageBox.Show("thro' date should be greater than from date");
                }
                else
                    stThroDate = value;

                RaisePropertyChanged("ThroDate");
            }
        }

        private ObservableCollection<ACLData> reconDataList;
        public ObservableCollection<ACLData> DataList
        {
            get
            {
                return reconDataList;
            }
            set
            {
                reconDataList = value;
                RaisePropertyChanged("DataList");
            }
        }
        private ObservableCollection<CRRACLData> reconCRRDataList;
        public ObservableCollection<CRRACLData> CRRDataList
        {
            get
            {
                return reconCRRDataList;
            }
            set
            {
                reconCRRDataList = value;
                RaisePropertyChanged("CRRDataList");
            }
        }

        private ObservableCollection<DAMACLData> damCRRDataList;
        public ObservableCollection<DAMACLData> damDataList
        {
            get
            {
                return damCRRDataList;
            }
            set
            {
                damCRRDataList = value;
                RaisePropertyChanged("damDataList");
            }
        }
        private ObservableCollection<ClTranACLData> cltranCRRDataList;
        public ObservableCollection<ClTranACLData> cltranDataList
        {
            get
            {
                return cltranCRRDataList;
            }
            set
            {
                cltranCRRDataList = value;
                RaisePropertyChanged("cltranDataList");
            }
        }
        private bool stDateRangeCheckBoxChecked = true;
        public bool DateRangeCheckBoxChecked
        {
            get { return stDateRangeCheckBoxChecked; }
            set
            {
                stDateRangeCheckBoxChecked = value;
                if (FromSelectedDate > ThroDate)
                {
                    //System.Windows.MessageBox.Show("Resetting the from date");
                    FromSelectedDate = ThroDate.AddDays(-1);
                }
                RaisePropertyChanged("DateRangeCheckBoxChecked");
            }
        }

        #endregion
        #region ClProperties
        private bool vCheckedAll = true;
        public bool CheckedAll
        {
            get
            {
                return vCheckedAll;
            }
            set
            {
                vCheckedAll = value;
                RaisePropertyChanged("CheckedAll");
                DisplayColumns();
            }
        }
        private bool vCheckedLetterOfCredit = true;
        public bool CheckedLetterOfCredit
        {
            get
            {
                return vCheckedLetterOfCredit;
            }
            set
            {
                vCheckedLetterOfCredit = value;
                RaisePropertyChanged("CheckedLetterOfCredit");
            }
        }
        private bool vCheckedMonth = true;
        public bool CheckedMonth
        {
            get
            {
                return vCheckedMonth;
            }
            set
            {
                vCheckedMonth = value;
                RaisePropertyChanged("CheckedMonth");
            }
        }
        private bool vCheckedSuretyBond = true;
        public bool CheckedSuretyBond
        {
            get
            {
                return vCheckedSuretyBond;
            }
            set
            {
                vCheckedSuretyBond = value;
                RaisePropertyChanged("CheckedSuretyBond");
            }
        }

        private bool vCheckedGuarantee = true;
        public bool CheckedGuarantee
        {
            get
            {
                return vCheckedGuarantee;
            }
            set
            {
                vCheckedGuarantee = value;
                RaisePropertyChanged("CheckedGuarantee");
            }
        }

        private bool vCheckedUnsecuredCreditLimit = true;
        public bool CheckedUnsecuredCreditLimit
        {
            get
            {
                return vCheckedUnsecuredCreditLimit;
            }
            set
            {
                vCheckedUnsecuredCreditLimit = value;
                RaisePropertyChanged("CheckedUnsecuredCreditLimit");
            }
        }
        private bool vCheckedApprovedCRRBilateralTrades = true;
        public bool CheckedApprovedCRRBilateralTrades
        {
            get
            {
                return vCheckedApprovedCRRBilateralTrades;
            }
            set
            {
                vCheckedApprovedCRRBilateralTrades = value;
                RaisePropertyChanged("CheckedApprovedCRRBilateralTrades");
            }
        }
        private bool vCheckedCRRLockedACL = true;
        public bool CheckedCRRLockedACL
        {
            get
            {
                return vCheckedCRRLockedACL;
            }
            set
            {
                vCheckedCRRLockedACL = value;
                RaisePropertyChanged("CheckedCRRLockedACL");
            }
        }
        #endregion
        #region crrProperties
        private bool vCheckedAllcrr = true;
        public bool CheckedAllcrr
        {
            get
            {
                return vCheckedAllcrr;
            }
            set
            {
                vCheckedAllcrr = value;
                RaisePropertyChanged("CheckedAllcrr");
                DisplayColumnsCRR();
            }
        }
        private bool vCheckedApprovedBilateralTrades = true;
        public bool CheckedApprovedBilateralTrades
        {
            get
            {
                return vCheckedApprovedBilateralTrades;
            }
            set
            {
                vCheckedApprovedBilateralTrades = value;
                RaisePropertyChanged("CheckedApprovedBilateralTrades");
            }
        }

        private bool vCheckedACLLockedForCRR = true;
        public bool CheckedACLLockedForCRR
        {
            get
            {
                return vCheckedACLLockedForCRR;
            }
            set
            {
                vCheckedACLLockedForCRR = value;
                RaisePropertyChanged("CheckedACLLockedForCRR");
            }
        }
        private bool vTPESInExcessOfSecuredCollateral = true;
        public bool TPESInExcessOfSecuredCollateral
        {
            get
            {
                return vTPESInExcessOfSecuredCollateral;
            }
            set
            {
                vTPESInExcessOfSecuredCollateral = value;
                RaisePropertyChanged("TPESInExcessOfSecuredCollateral");
            }
        }

        private bool vCheckedOutstandingSecuredCollateralRequest = true;
        public bool CheckedOutstandingSecuredCollateralRequest
        {
            get
            {
                return vCheckedOutstandingSecuredCollateralRequest;
            }
            set
            {
                vCheckedOutstandingSecuredCollateralRequest = value;
                RaisePropertyChanged("CheckedOutstandingSecuredCollateralRequest");
            }
        }

        private bool vCheckedAdditionalSecuredCollateralRequired = true;
        public bool CheckedAdditionalSecuredCollateralRequired
        {
            get
            {
                return vCheckedAdditionalSecuredCollateralRequired;
            }
            set
            {
                vCheckedAdditionalSecuredCollateralRequired = value;
                RaisePropertyChanged("CheckedAdditionalSecuredCollateralRequired");
            }
        }

        #endregion
        #region damProperties
        private bool vCheckedAlldam = true;
        public bool CheckedAlldam
        {
            get
            {
                return vCheckedAlldam;
            }
            set
            {
                vCheckedAlldam = value;
                RaisePropertyChanged("CheckedAlldam");
                DisplayColumnsDAM();
            }
        }
        private bool vCheckedGt = true;
        public bool CheckedGt
        {
            get
            {
                return vCheckedGt;
            }
            set
            {
                vCheckedGt = value;
                RaisePropertyChanged("CheckedGt");
            }
        }

        private bool vCheckedUnsecuredCreditt = true;
        public bool CheckedUnsecuredCreditt
        {
            get
            {
                return vCheckedUnsecuredCreditt;
            }
            set
            {
                vCheckedUnsecuredCreditt = value;
                RaisePropertyChanged("CheckedUnsecuredCreditt");
            }
        }
        private bool vTPEAInExcessOfRemainderCollateral = true;
        public bool TPEAInExcessOfRemainderCollateral
        {
            get
            {
                return vTPEAInExcessOfRemainderCollateral;
            }
            set
            {
                vTPEAInExcessOfRemainderCollateral = value;
                RaisePropertyChanged("TPEAInExcessOfRemainderCollateral");
            }
        }

        private bool vCheckedAdditionalAnyCollateralRd = true;
        public bool CheckedAdditionalAnyCollateralRd
        {
            get
            {
                return vCheckedAdditionalAnyCollateralRd;
            }
            set
            {
                vCheckedAdditionalAnyCollateralRd = value;
                RaisePropertyChanged("CheckedAdditionalAnyCollateralRd");
            }
        }
        private bool vOutstandingAnyCollateralRequest = true;
        public bool OutstandingAnyCollateralRequest
        {
            get
            {
                return vOutstandingAnyCollateralRequest;
            }
            set
            {
                vOutstandingAnyCollateralRequest = value;
                RaisePropertyChanged("OutstandingAnyCollateralRequest");
            }
        }

        #endregion
        public MainWindowViewModel(IDataService dataService = null)
        {
            _dataService = dataService;
            RunRefreshommand = new DelegateCommand(Refresh);
            CLExportCommand = new DelegateCommand(CLExport);
            CRRExportCommand = new DelegateCommand(CRRExport);
            DAMExportCommand = new DelegateCommand(DAMExport);
            cltranExportCommand = new DelegateCommand(CltranExport);
            FromSelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 01);
        }
        private void DisplayColumns()
        {
            if (CheckedAll == true)
            {
                CheckedLetterOfCredit = true;
                CheckedMonth = true;
                CheckedSuretyBond = true;
                CheckedGuarantee = true;
                CheckedUnsecuredCreditLimit = true;
                CheckedApprovedCRRBilateralTrades = true;
                CheckedCRRLockedACL = true;
            }
            if (CheckedAll == false)
            {
                CheckedLetterOfCredit = false;
                CheckedMonth = false;
                CheckedSuretyBond = false;
                CheckedGuarantee = false;
                CheckedUnsecuredCreditLimit = false;
                CheckedApprovedCRRBilateralTrades = false;
                CheckedCRRLockedACL = false;
            }
        }
        private void DisplayColumnsCRR()
        {
            if (CheckedAllcrr == true)
            {
                CheckedApprovedBilateralTrades = true;
                CheckedACLLockedForCRR = true;
                TPESInExcessOfSecuredCollateral = true;
                CheckedOutstandingSecuredCollateralRequest = true;
                CheckedAdditionalSecuredCollateralRequired = true;
            }
            if (CheckedAllcrr == false)
            {
                CheckedApprovedBilateralTrades = false;
                CheckedACLLockedForCRR = false;
                TPESInExcessOfSecuredCollateral = false;
                CheckedOutstandingSecuredCollateralRequest = false;
                CheckedAdditionalSecuredCollateralRequired = false;
            }
        }
        private void DisplayColumnsDAM()
        {
            if (CheckedAlldam == true)
            {
                CheckedGt = true;
                CheckedUnsecuredCreditt = true;
                TPEAInExcessOfRemainderCollateral = true;
                OutstandingAnyCollateralRequest = true;
                CheckedAdditionalAnyCollateralRd = true;
            }
            if (CheckedAlldam == false)
            {
                CheckedGt = false;
                CheckedUnsecuredCreditt = false;
                TPEAInExcessOfRemainderCollateral = false;
                OutstandingAnyCollateralRequest = false;
                CheckedAdditionalAnyCollateralRd = false;
            }
        }
        private void CLExport()
        {
            try
            {
                if (DataList == null)
                {
                    MessageBox.Show("Please click Refresh button");
                }
                else
                {
                    SaveFileDialog savefiledialog = new SaveFileDialog();
                    savefiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    savefiledialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                    savefiledialog.FilterIndex = 1;
                    savefiledialog.RestoreDirectory = true;
                    savefiledialog.FileName = "ErcotACLSummary" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");
                    if ((bool)savefiledialog.ShowDialog())
                    {
                        StringBuilder builder = new StringBuilder();

                        builder.AppendLine("Ercot ACL Summary Data");
                        foreach (PropertyInfo item in DataList[0].GetType().GetProperties())
                        {
                            builder.Append(item.Name + ",");
                        }
                        builder.ToString().Remove(builder.Length - 1, 1);
                        builder.AppendLine();

                        foreach (ACLData item in DataList)
                        {
                            foreach (PropertyInfo propName in item.GetType().GetProperties())
                            {
                                builder.Append((item.GetType().GetProperty(propName.Name).GetValue(item) == null ? "" : item.GetType().GetProperty(propName.Name).GetValue(item).ToString().Replace(',', '-')) + ",");
                            }
                            builder.AppendLine();
                        }

                        builder.AppendLine();

                        try
                        {
                            TextWriter writer = new StreamWriter(savefiledialog.FileName);
                            writer.Write(builder.ToString());
                            writer.Flush();
                            writer.Close();
                            MessageBox.Show("Successfully created the file. ", savefiledialog.FileName);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Unable To Create File");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void CRRExport()
        {
            try
            {
                if (CRRDataList == null)
                {
                    MessageBox.Show("Please click Refresh button");
                }
                else
                {
                    SaveFileDialog savefiledialog = new SaveFileDialog();
                    savefiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    savefiledialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                    savefiledialog.FilterIndex = 1;
                    savefiledialog.RestoreDirectory = true;
                    savefiledialog.FileName = "TPESAndCRRACLSummary" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");
                    if ((bool)savefiledialog.ShowDialog())
                    {
                        StringBuilder builder = new StringBuilder();

                        builder.AppendLine("TPES and CRR ACL Summary data");
                        foreach (PropertyInfo item in CRRDataList[0].GetType().GetProperties())
                        {
                            builder.Append(item.Name + ",");
                        }
                        builder.ToString().Remove(builder.Length - 1, 1);
                        builder.AppendLine();

                        foreach (CRRACLData item in CRRDataList)
                        {
                            foreach (PropertyInfo propName in item.GetType().GetProperties())
                            {
                                builder.Append((item.GetType().GetProperty(propName.Name).GetValue(item) == null ? "" : item.GetType().GetProperty(propName.Name).GetValue(item).ToString().Replace(',', '-')) + ",");
                            }
                            builder.AppendLine();
                        }
                        builder.AppendLine();

                        try
                        {
                            TextWriter writer = new StreamWriter(savefiledialog.FileName);
                            writer.Write(builder.ToString());
                            writer.Flush();
                            writer.Close();
                            MessageBox.Show("Successfully created the file. ", savefiledialog.FileName);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Unable To Create File");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void DAMExport()
        {
            try
            {
                if (damDataList == null)
                {
                    MessageBox.Show("Please click Refresh button");
                }
                else
                {
                    SaveFileDialog savefiledialog = new SaveFileDialog();
                    savefiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    savefiledialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                    savefiledialog.FilterIndex = 1;
                    savefiledialog.RestoreDirectory = true;
                    savefiledialog.FileName = "TPESAndCRRACLSummary" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");
                    if ((bool)savefiledialog.ShowDialog())
                    {
                        StringBuilder builder = new StringBuilder();

                        builder.AppendLine("TPEAandDAMACLSummaryData");
                        foreach (PropertyInfo item in damDataList[0].GetType().GetProperties())
                        {
                            builder.Append(item.Name + ",");
                        }
                        builder.ToString().Remove(builder.Length - 1, 1);
                        builder.AppendLine();

                        foreach (DAMACLData item in damDataList)
                        {
                            foreach (PropertyInfo propName in item.GetType().GetProperties())
                            {
                                builder.Append((item.GetType().GetProperty(propName.Name).GetValue(item) == null ? "" : item.GetType().GetProperty(propName.Name).GetValue(item).ToString().Replace(',', '-')) + ",");
                            }
                            builder.AppendLine();
                        }
                        builder.AppendLine();

                        try
                        {
                            TextWriter writer = new StreamWriter(savefiledialog.FileName);
                            writer.Write(builder.ToString());
                            writer.Flush();
                            writer.Close();
                            MessageBox.Show("Successfully created the file. ", savefiledialog.FileName);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Unable To Create File");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void CltranExport()
        {
            try
            {
                if (cltranDataList == null)
                {
                    MessageBox.Show("Please click Refresh button");
                }
                else
                {
                    SaveFileDialog savefiledialog = new SaveFileDialog();
                    savefiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    savefiledialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                    savefiledialog.FilterIndex = 1;
                    savefiledialog.RestoreDirectory = true;
                    savefiledialog.FileName = "CollateralTransactionsSummary" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");
                    if ((bool)savefiledialog.ShowDialog())
                    {
                        StringBuilder builder = new StringBuilder();

                        builder.AppendLine("Collateral Transactions Summary");
                        foreach (PropertyInfo item in cltranDataList[0].GetType().GetProperties())
                        {
                            builder.Append(item.Name + ",");
                        }
                        builder.ToString().Remove(builder.Length - 1, 1);
                        builder.AppendLine();

                        foreach (ClTranACLData item in cltranDataList)
                        {
                            foreach (PropertyInfo propName in item.GetType().GetProperties())
                            {
                                builder.Append((item.GetType().GetProperty(propName.Name).GetValue(item) == null ? "" : item.GetType().GetProperty(propName.Name).GetValue(item).ToString().Replace(',', '-')) + ",");
                            }
                            builder.AppendLine();
                        }
                        builder.AppendLine();

                        try
                        {
                            TextWriter writer = new StreamWriter(savefiledialog.FileName);
                            writer.Write(builder.ToString());
                            writer.Flush();
                            writer.Close();
                            MessageBox.Show("Successfully created the file. ", savefiledialog.FileName);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Unable To Create File");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void Refresh()
        {
            try
            {
                DataService ds = new DataService();
                if (DateRangeCheckBoxChecked == false)
                {
                    DataList = ds.GetAllData(FromSelectedDate);
                    CRRDataList = ds.GetCRRAllData(FromSelectedDate);
                    damDataList = ds.GetDAMAllData(FromSelectedDate);
                    cltranDataList = ds.GetClTRanAllData(FromSelectedDate);
                }
                else
                {
                    DataList = ds.GetMultipleDaysData(FromSelectedDate, ThroDate);
                    CRRDataList = ds.GetCRRMultipleDaysData(FromSelectedDate, ThroDate);
                    damDataList = ds.GetDAMMultipleDaysData(FromSelectedDate, ThroDate);
                    cltranDataList = ds.GetClTranMultipleDaysData(FromSelectedDate, ThroDate);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
