using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Deployment.Application;
using System.Windows.Threading;
using VayuApplication.Model;
namespace Vayu.ViewModels
{
    /// <summary>
    /// The MainWindowViewModel
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// The Data service
        /// </summary>
        private readonly IDataService _dataService;

        /// <summary>
        /// Login User Name
        /// </summary>
        private string _user = Environment.UserName;

        private string _version;

        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                RaisePropertyChanged("Version");
            }
        }
        private bool _ReconEnabled;

        public bool ReconEnabled
        {
            get { return _ReconEnabled; }
            set
            {
                _ReconEnabled = value;
                RaisePropertyChanged("ReconEnabled");
            }
        }
        private bool _bid_evEnabled;

        public bool bid_evEnabled
        {
            get { return _bid_evEnabled; }
            set { _bid_evEnabled = value; RaisePropertyChanged("bid_evEnabled"); }
        }
        private bool _aclevEnabled;

        public bool aclevEnabled
        {
            get { return _aclevEnabled; }
            set { _aclevEnabled = value; RaisePropertyChanged("aclevEnabled"); }
        }

        private bool _performanceEnabled;

        public bool performanceEnabled
        {
            get { return _performanceEnabled; }
            set { _performanceEnabled = value; RaisePropertyChanged("performanceEnabled"); }
        }

        private bool _adminEnabled;
        public bool AdminEnabled
        {
            get { return _adminEnabled; }
            set
            {
                _adminEnabled = value;
                RaisePropertyChanged("AdminEnabled");
            }
        }

        //private bool _enjNotificationEnabled;
        //public bool ENJNotificationEnabled
        //{
        //    get { return _enjNotificationEnabled; }
        //    set
        //    {
        //        _enjNotificationEnabled = value;
        //        RaisePropertyChanged("ENotificationEnabled");
        //    }
        //}

        private bool _UTCRiskCopyEnabled;
        public bool UTCRiskCopyEnabled
        {
            get { return _UTCRiskCopyEnabled; }
            set
            {
                _UTCRiskCopyEnabled = value;
                RaisePropertyChanged("UTCRiskCopyEnabled");
            }
        }

        //private bool _congestionEnabled;
        //public bool CongestionEnabled
        //{
        //    get { return _congestionEnabled; }
        //    set
        //    {
        //        _congestionEnabled = value;
        //        RaisePropertyChanged("CongestionEnabled");
        //    }
        //}

        ///// <summary>
        ///// The kill timer
        ///// </summary>
        private static DispatcherTimer _killTimer = new DispatcherTimer { Interval = new TimeSpan(0, 2, 0), IsEnabled = true };


        #region  Delegate Properties 
        public DelegateCommand PTPSubmission_Cmd { get; private set; }

        public DelegateCommand Market_View { get; private set; }

        public DelegateCommand UTC_Risk { get; private set; }
        public DelegateCommand DAM_Impact { get; private set; }
        public DelegateCommand Ercot_Notification { get; private set; }

        public DelegateCommand Risk_Control { get; private set; }

        public DelegateCommand Daily_pnl { get; private set; }

        public DelegateCommand Hourly_pnl { get; private set; }

        public DelegateCommand Historical_constraints { get; private set; }

        public DelegateCommand System_Demand_Curve { get; private set; }
        public DelegateCommand Actual_vs_7Day { get; private set; }

        public DelegateCommand Load_Curve { get; private set; }
        public DelegateCommand Node_Price_Daily { get; private set; }
        public DelegateCommand HResourceOutage { get; private set; }

        public DelegateCommand LMP_Stat_screen { get; private set; }
        public DelegateCommand congestion_volatility { get; private set; }

        public DelegateCommand constriant_expo { get; private set; }

        public DelegateCommand bid_ev { get; private set; }

        public DelegateCommand recon { get; private set; }

        public DelegateCommand acl { get; private set; }

        public DelegateCommand performance { get; private set; }

        public DelegateCommand Market_Overview { get; private set; }
        public DelegateCommand CRR_Anlysis { get; private set; }

        public DelegateCommand CRR_Ann_Anlysis { get; private set; }
        public DelegateCommand Constraint_analyzer { get; private set; }
        public DelegateCommand Shift_Factor { get; private set; }

        public DelegateCommand Node_sensitivity { get; private set; }
        public DelegateCommand LTC_Price_Graph { get; private set; }

        public DelegateCommand Node_price { get; private set; }

        public DelegateCommand constraint_outage { get; private set; }
        public DelegateCommand outage_constraint_history { get; private set; }

        public DelegateCommand block_algo { get; private set; }
        public DelegateCommand Constraint_sensetivity { get; private set; }
        public DelegateCommand Rt_Impact { get; private set; }
        public DelegateCommand Outage_Constraint_Mapping { get; private set; }

        public DelegateCommand Temp_history { get; private set; }
        public DelegateCommand Power_Map { get; private set; }

        public DelegateCommand CRR_top { get; private set; }

        public DelegateCommand crrperiods { get; private set; }

        public DelegateCommand CRRaution { get; private set; }

        public DelegateCommand Tempgraph { get; private set; }
        public DelegateCommand H_Temp_Graph { get; private set; }
        public DelegateCommand Renewable_Graph { get; private set; }

        public DelegateCommand Crrpath { get; private set; }

        public DelegateCommand Powergen { get; private set; }

        public DelegateCommand LTC { get; private set; }
        public DelegateCommand LTCAnnual_Cmd { get; private set; }

        public DelegateCommand CRRpnl { get; private set; }

        public DelegateCommand Nodeprice { get; private set; }

        public DelegateCommand loadforcast { get; private set; }
        public DelegateCommand NodePriceHourly { get; private set; }
        public DelegateCommand fuelmix { get; private set; }

        

        private void PTPSubmission()
        {
            Vayu.WorkbookStatistics.ViewModels.MainWindowViewModel workBookModel = new WorkbookStatistics.ViewModels.MainWindowViewModel(new WorkbookStatistics.Model.DataService());
            var window = new WorkbookStatistics.Views.MainWindow(workBookModel);
            window.DataContext = workBookModel;
            window.Show();
        }
        private void Loadforcast()
        {
            LoadForcast.Views.MainWindow window = new LoadForcast.Views.MainWindow();
            window.DataContext = new LoadForcast.ViewModels.MainWindowViewModel(new LoadForcast.Model.DataService());
            window.Show();
        }

        private void Node_Price_Hourly()
        {
            Vayu.Node_Price_Hourly.Views.MainWindow window = new Node_Price_Hourly.Views.MainWindow();
            window.Show();
        }

        private void MarketView()
        {
            Vayu.MarketView.Views.MainWindow window = new MarketView.Views.MainWindow();
            window.DataContext = new Vayu.MarketView.ViewModels.MainWindowViewModel(new Vayu.MarketView.Model.DataService());
            window.Show();
        }

        private void OpenAdmin()
        {
            if (_user == "rwaterston" || _user == "darshand" || _user == "gojira" || _user == "sangramp" || _user == "hjaybhay")
            {
                Vayu.Admin.Views.MainWindow window = new Admin.Views.MainWindow();
                window.DataContext = new Vayu.Admin.ViewModels.MainWindowViewModel(new Vayu.Admin.Model.DataService());
                window.Show();
            }
        }

        private void UTCControl()
        {
            UTCRiskCopy.Views.MainWindow window = new UTCRiskCopy.Views.MainWindow();
            window.DataContext = new UTCRiskCopy.ViewModels.MainWindowViewModel(new UTCRiskCopy.Model.DataService());
            window.Show();
        }

        private void DAMImpact()
        {
            DAMImpact.Views.MainWindow window = new DAMImpact.Views.MainWindow();
            window.DataContext = new DAMImpact.ViewModels.MainWindowViewModel(new DAMImpact.Model.DataService());
            window.Show();
        }

        private void Notification()
        {
            Notifications.Views.MainWindow window = new Notifications.Views.MainWindow();
            window.DataContext = new Notifications.ViewModels.MainWindowViewModel(new Notifications.Model.DataService());
            window.Show();
        }

        private void DailyPNL()
        {
            Vayu.ProfitLossDaily.Views.MainWindow window = new ProfitLossDaily.Views.MainWindow();
            window.DataContext = new ProfitLossDaily.ViewModels.MainWindowViewModel(new ProfitLossDaily.Model.DataService());
            window.Show();
        }

        private void HourlyPNL()
        {
            ProfitLossHour.Views.MainWindow window = new ProfitLossHour.Views.MainWindow();
            window.DataContext = new ProfitLossHour.ViewModels.MainWindowViewModel(new ProfitLossHour.Model.HourlyDataService());
            window.Show();

        }

        private void Historicalconstraints()
        {
            ConstraintContingencyHistory.Views.MainWindow window = new ConstraintContingencyHistory.Views.MainWindow();
            window.DataContext = new ConstraintContingencyHistory.ViewModels.MainWindowViewModel(new ConstraintContingencyHistory.Model.DataService());
            window.Show();
        }

        private void SystemDemandCurve()
        {
            SystemDemand_Curve.Views.MainWindow window = new SystemDemand_Curve.Views.MainWindow();
            window.DataContext = new SystemDemand_Curve.ViewModels.MainWindowViewModel(new SystemDemand_Curve.Model.DataService());
            window.Show();
        }

        private void ActualVs7Day()
        {
            Actualvs7DayLoad.Views.MainWindow window = new Actualvs7DayLoad.Views.MainWindow();
            window.DataContext = new Actualvs7DayLoad.ViewModels.MainWindowViewModel(new Actualvs7DayLoad.Model.DataService());
            window.Show();
        }

        private void LoadCurve()
        {
            Vayu.LoadCurve.Views.MainWindow window = new LoadCurve.Views.MainWindow();
            window.DataContext = new Vayu.LoadCurve.ViewModels.MainWindowViewModel(new LoadCurve.Model.DataService());
            window.Show();
        }

        private void NodePriceDaily()
        {
            Vayu.NodePriceDaily.Views.MainWindow window = new NodePriceDaily.Views.MainWindow();
            window.DataContext = new Vayu.NodePriceDaily.ViewModels.MainWindowViewModel(new NodePriceDaily.Model.DataService());
            window.Show();
        }

        private void HrResourceOutage()
        {
            Vayu.HourlyResourceOutgae.Views.MainWindow window = new HourlyResourceOutgae.Views.MainWindow();
            window.DataContext = new Vayu.HourlyResourceOutgae.ViewModels.MainWindowViewModel(new HourlyResourceOutgae.Model.DataService());
            window.Show();
        }

        private void LMPstatisticsAnalyzer()
        {
            Vayu.LMPStatistics.Views.MainWindow window = new LMPStatistics.Views.MainWindow();
            window.DataContext = new Vayu.LMPStatistics.ViewModels.MainWindowViewModel(new LMPStatistics.Model.DataService());
            window.Show();
        }

        private void CongestionVolatility()
        {
            Vayu.CongestionVolatilityIndex.Views.MainWindow window = new CongestionVolatilityIndex.Views.MainWindow();
            window.DataContext = new Vayu.CongestionVolatilityIndex.ViewModels.MainWindowViewModel(new CongestionVolatilityIndex.Model.DataService());
            window.Show();
        }

        private void ConstraintExpo()
        {
            Vayu.ConstraintExposure.Views.MainWindow window = new ConstraintExposure.Views.MainWindow();
            window.DataContext = new Vayu.ConstraintExposure.ViewModels.MainWindowViewModel(new ConstraintExposure.Model.DataService());
            window.Show();
        }

        private void BidsEvaluation()
        {
            if (_user == "rwaterston"  || _user == "gojira" || _user == "sangramp" || _user == "hjaybhay")
            {
                Vayu.BidsEvaluation.Views.MainWindow window = new BidsEvaluation.Views.MainWindow();
                window.DataContext = new Vayu.BidsEvaluation.ViewModels.MainWindowViewModel(new BidsEvaluation.Model.DataService());
                window.Show();
            }
        }

        private void Reconciliation()
        {
            if (_user == "rwaterston"  || _user == "gojira" || _user == "sangramp" || _user == "hjaybhay")
            {
                Vayu.ReconciliationDashboard.Views.MainWindow window = new ReconciliationDashboard.Views.MainWindow();
                window.DataContext = new Vayu.ReconciliationDashboard.ViewModels.MainWindowViewModel(new ReconciliationDashboard.Model.DataService());
                window.Show();
            }
        }

        private void ERCOTACL()
        {
            if (_user == "rwaterston"   || _user == "gojira" || _user == "sangramp" || _user == "hjaybhay")
            {
                Vayu.ErcotACLSummaryReport.Views.MainWindow window = new ErcotACLSummaryReport.Views.MainWindow();
                window.DataContext = new Vayu.ErcotACLSummaryReport.ViewModels.MainWindowViewModel(new ErcotACLSummaryReport.Model.DataService());
                window.Show();
            }

        }

        private void PerformanceReview()
        {
            if (_user == "rwaterston"   || _user == "gojira" || _user == "sangramp" || _user == "hjaybhay")
            {
                Vayu.PerformanceReview.Views.MainWindow window = new PerformanceReview.Views.MainWindow();
                window.DataContext = new Vayu.PerformanceReview.ViewModels.MainWindowViewModel(new PerformanceReview.Model.DataService());
                window.Show();
            }

        }

        private void MarketOverview()
        {
            Vayu.ERCOT_Market_Overview.Views.MainWindow window = new ERCOT_Market_Overview.Views.MainWindow();
            window.DataContext = new Vayu.ERCOT_Market_Overview.ViewModels.MainWindowViewModel(new ERCOT_Market_Overview.Model.DataService());
            window.Show();
        }

        private void CRRAnalysis()
        {
            Vayu.CRRAnalysis.Views.MainWindow window = new CRRAnalysis.Views.MainWindow();
            window.DataContext = new Vayu.CRRAnalysis.ViewModels.MainWindowViewModel(new CRRAnalysis.Model.DataService());
            window.Show();
        }
        private void CRRAnnAnalysis()
        {
            Vayu.CRRAnnAnalysis.Views.MainWindow window = new CRRAnnAnalysis.Views.MainWindow();
            window.DataContext = new Vayu.CRRAnnAnalysis.ViewModels.MainWindowViewModel(new CRRAnnAnalysis.Model.DataService());
            window.Show();
        }

        private void ConstraintAnalyzer()
        {
            Vayu.ConstraintAnalyzer.Views.MainWindow window = new ConstraintAnalyzer.Views.MainWindow();
            window.DataContext = new Vayu.ConstraintAnalyzer.ViewModels.MainWindowViewModel(new ConstraintAnalyzer.Model.DataService());
            window.Show();
        }

        private void NodeSensitivity()
        {
            Vayu.NodeSensitivityAnalysis.Views.MainWindow window = new NodeSensitivityAnalysis.Views.MainWindow();
            window.DataContext = new Vayu.NodeSensitivityAnalysis.ViewModels.MainWindowViewModel(new NodeSensitivityAnalysis.Model.DataService());
            window.Show();

        }

        private void LTCPriceGraph()
        {
            Vayu.LTC_Graphs.Views.MainWindow window = new LTC_Graphs.Views.MainWindow();
            window.DataContext = new Vayu.LTC_Graphs.ViewModels.MainWindowViewModel(new LTC_Graphs.Model.DataService());
            window.Show();

        }

        private void NodePriceMonitor()
        {
            Vayu.NodePriceMonitor.Views.MainWindow window = new NodePriceMonitor.Views.MainWindow();
            window.DataContext = new Vayu.NodePriceMonitor.ViewModels.MainWindowViewModel(new NodePriceMonitor.Model.DataService());
            window.Show();

        }
        private void ShiftFactor()
        {
            Vayu.ErcotShiftFactor.Views.MainWindow window = new ErcotShiftFactor.Views.MainWindow();
            window.DataContext = new Vayu.ErcotShiftFactor.ViewModels.MainWindowViewModel(new ErcotShiftFactor.Model.DataService());
            window.Show();

        }

        private void Block_Algo()
        {
            Vayu.UptosPathAnalysisAlgorithm.Views.MainWindow window = new UptosPathAnalysisAlgorithm.Views.MainWindow();
            window.DataContext = new Vayu.UptosPathAnalysisAlgorithm.ViewModels.MainWindowViewModel(new UptosPathAnalysisAlgorithm.Model.DataService());
            window.Show();

        }

        private void Constraint_Sensetivity()
        {
            Vayu.ConstraintSensitivityAlgorithm.Views.MainWindow window = new ConstraintSensitivityAlgorithm.Views.MainWindow();
            window.DataContext = new Vayu.ConstraintSensitivityAlgorithm.ViewModels.MainWindowViewModel(new ConstraintSensitivityAlgorithm.Model.DataService());
            window.Show();

        }

        private void RT_Constraint_impact()
        {
            Vayu.RT_Constraint_impact.Views.MainWindow window = new RT_Constraint_impact.Views.MainWindow();
            window.DataContext = new Vayu.RT_Constraint_impact.ViewModels.MainWindowViewModel(new RT_Constraint_impact.Model.DataService());
            window.Show();

        }

        private void OutageConstraintMapping()
        {
            Vayu.OutageConstraintMapping.Views.MainWindow window = new OutageConstraintMapping.Views.MainWindow();
            window.DataContext = new Vayu.OutageConstraintMapping.ViewModels.MainWindowViewModel(new OutageConstraintMapping.Model.DataService());
            window.Show();

        }

        private void ConstraintOutageMapping()
        {
            ConstraintOutageMapping.Views.MainWindow window = new ConstraintOutageMapping.Views.MainWindow();
            window.DataContext = new ConstraintOutageMapping.ViewModels.MainWindowViewModel(new ConstraintOutageMapping.Model.DataService());
            window.Show();

        }

        private void OutageConstraintHistory()
        {
            Vayu.Outage_Constraint_History.Views.MainWindow window = new Outage_Constraint_History.Views.MainWindow();
            window.DataContext = new Outage_Constraint_History.ViewModels.MainWindowViewModel(new Outage_Constraint_History.Model.DataService());
            window.Show();

        }

        private void TempHistorical()
        {
            Vayu.TemperatureHistoryDetails.Views.MainWindow window = new TemperatureHistoryDetails.Views.MainWindow();
            window.DataContext = new Vayu.TemperatureHistoryDetails.ViewModels.MainWindowViewModel(new TemperatureHistoryDetails.Model.DataService1());
            window.Show();

        }

        private void PowerMap()
        {
            Vayu.PowerMap.Views.MainWindow window = new PowerMap.Views.MainWindow();
            window.Show();

        }

        private void CRRAution()
        {
            Vayu.CRRAuction.Views.MainWindow window = new CRRAuction.Views.MainWindow();
            window.DataContext = new Vayu.CRRAuction.ViewModels.MainWindowViewModel(new CRRAuction.Model.DataService());
            window.Show();

        }

        private void CRRPeriod()
        {
            Vayu.CRRPeriods.Views.MainWindow window = new CRRPeriods.Views.MainWindow();
            window.DataContext = new Vayu.CRRPeriods.ViewModels.MainWindowViewModel(new CRRPeriods.Model.DataService());
            window.Show();

        }

        private void TempGraph()
        {
            Vayu.TemperatureGraph.Views.MainWindow window = new TemperatureGraph.Views.MainWindow();
            window.DataContext = new Vayu.TemperatureGraph.ViewModels.MainWindowViewModel(new TemperatureGraph.Model.DataService());
            window.Show();

        }

        private void HTempGraph()
        {
            Vayu.HourlyTemp_Grpah.Views.MainWindow window = new HourlyTemp_Grpah.Views.MainWindow();
            window.DataContext = new Vayu.HourlyTemp_Grpah.ViewModels.MainWindowViewModel(new HourlyTemp_Grpah.Model.DataService());
            window.Show();

        }

        private void RenewableGraph()
        {
            Vayu.RenewableOutageGraph.Views.MainWindow window = new RenewableOutageGraph.Views.MainWindow();
            window.DataContext = new Vayu.RenewableOutageGraph.ViewModels.MainWindowViewModel(new RenewableOutageGraph.Model.DataService());
            window.Show();

        }

        public void CRRTop10P()
        {
            Vayu.CRRTopTenParticipants.Views.MainWindow window = new CRRTopTenParticipants.Views.MainWindow();
            window.DataContext = new Vayu.CRRTopTenParticipants.ViewModels.MainWindowViewModel(new CRRTopTenParticipants.Model.DataService());
            window.Show();
        }

        private void CRRPathAnalyzer()
        {
            CRRPathAnalyzer.Views.MainWindow window = new CRRPathAnalyzer.Views.MainWindow();
            window.DataContext = new CRRPathAnalyzer.ViewModels.MainWindowViewModel(new CRRPathAnalyzer.Model.DataService());
            window.Show();
        }

        private void PowerGeneration()
        {
            Vayu.PowerGeneration.Views.MainWindow window = new PowerGeneration.Views.MainWindow();
            window.DataContext = new PowerGeneration.ViewModels.MainWindowViewModel(new PowerGeneration.Model.DataService());
            window.Show();
        }

        private void LTCScreen()
        {
            Vayu.LTC_Portfolio.Views.MainWindow window = new LTC_Portfolio.Views.MainWindow();
            window.DataContext = new LTC_Portfolio.ViewModels.MainWindowViewModel(new LTC_Portfolio.Model.DataService());
            window.Show();
        }
        private void LTCAnnualScreen()
        {
            Vayu.LTC_PortfolioAnnual.Views.MainWindow window = new LTC_PortfolioAnnual.Views.MainWindow();
            window.DataContext = new LTC_PortfolioAnnual.ViewModels.MainWindowViewModel(new LTC_PortfolioAnnual.Model.DataService());
            window.Show();
        }

        private void CRRpnlScreen()
        {
            Vayu.CRRPNLDetails.Views.MainWindow window = new CRRPNLDetails.Views.MainWindow();
            var dataService = new CRRPNLDetails.Model.DataService();
            window.DataContext = new CRRPNLDetails.ViewModels.MainWindowViewModel(dataService);
            //window.DataContext = new CRRPNLDetails.ViewModels.MainWindowViewModel(new CRRPNLDetails.Model.DataService());
            window.Show();

        }

        private void NodePriceGrpah()
        {
            Vayu.NodePriceGraph.Views.MainWindow window = new NodePriceGraph.Views.MainWindow();
            window.DataContext = new NodePriceGraph.ViewModels.MainWindowViewModel(new NodePriceGraph.Model.DataService());
            window.Show();

        }

        private void FuelMix()
        {
            Vayu.FuelMix.Views.MainWindow window = new FuelMix.Views.MainWindow();
            window.DataContext = new FuelMix.ViewModels.MainWindowViewModel(new FuelMix.Model.DataService());
            window.Show();

        }
        #endregion
        ///// <summary>
        ///// Handles the Tick event of the mKillTimer control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void killTimer_Tick(object sender, EventArgs e)
        {
            _killTimer.IsEnabled = false;
            KillApp();
            _killTimer.IsEnabled = true;
        }

        ///// <summary>
        ///// Kills the application.
        ///// </summary>
        private void KillApp()
        {
            if (DateTime.Now.Hour == 1)
            {
                if (DateTime.Now.Minute > 0 && DateTime.Now.Minute < 10)
                {
                    try
                    {
                        Environment.Exit(0);
                    }
                    catch (Exception)
                    {
                        Environment.Exit(0);
                    }
                }
            }
        }



        /// <summary>
        /// Home Screen MainWindowViewModel
        /// </summary>
        /// <param name="dataService"></param>
        public MainWindowViewModel()
        {
            try
            {
                Version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch (Exception ex)
            {
            }

            _killTimer.Tick += killTimer_Tick;

            PTPSubmission_Cmd = new DelegateCommand(() => PTPSubmission());

            Market_View = new DelegateCommand(() => MarketView());

            UTC_Risk = new DelegateCommand(() => UTCControl());
            DAM_Impact = new DelegateCommand(() => DAMImpact());
            Ercot_Notification = new DelegateCommand(() => Notification());

            Risk_Control = new DelegateCommand(() => OpenAdmin());

            Daily_pnl = new DelegateCommand(() => DailyPNL());

            Hourly_pnl = new DelegateCommand(() => HourlyPNL());

            Historical_constraints = new DelegateCommand(() => Historicalconstraints());

            System_Demand_Curve = new DelegateCommand(() => SystemDemandCurve());
            Actual_vs_7Day = new DelegateCommand(() => ActualVs7Day());

            Load_Curve = new DelegateCommand(() => LoadCurve());
            Node_Price_Daily = new DelegateCommand(() => NodePriceDaily());
            HResourceOutage = new DelegateCommand(() => HrResourceOutage());

            LMP_Stat_screen = new DelegateCommand(() => LMPstatisticsAnalyzer());
            congestion_volatility = new DelegateCommand(() => CongestionVolatility());

            constriant_expo = new DelegateCommand(() => ConstraintExpo());

            bid_ev = new DelegateCommand(() => BidsEvaluation());

            recon = new DelegateCommand(() => Reconciliation());

            acl = new DelegateCommand(() => ERCOTACL());

            performance = new DelegateCommand(() => PerformanceReview());

            Market_Overview = new DelegateCommand(() => MarketOverview());
            CRR_Anlysis = new DelegateCommand(() => CRRAnalysis());
            CRR_Ann_Anlysis = new DelegateCommand(() => CRRAnnAnalysis());
            Constraint_analyzer = new DelegateCommand(() => ConstraintAnalyzer());

            Node_sensitivity = new DelegateCommand(() => NodeSensitivity());
            LTC_Price_Graph = new DelegateCommand(() => LTCPriceGraph());

            Node_price = new DelegateCommand(() => NodePriceMonitor());
            Shift_Factor = new DelegateCommand(() => ShiftFactor());

            block_algo = new DelegateCommand(() => Block_Algo());
            Constraint_sensetivity = new DelegateCommand(() => Constraint_Sensetivity());
            Rt_Impact = new DelegateCommand(() => RT_Constraint_impact());
            Outage_Constraint_Mapping = new DelegateCommand(() => OutageConstraintMapping());

            constraint_outage = new DelegateCommand(() => ConstraintOutageMapping());
            outage_constraint_history = new DelegateCommand(() => OutageConstraintHistory());

            Temp_history = new DelegateCommand(() => TempHistorical());
            Power_Map = new DelegateCommand(() => PowerMap());

            crrperiods = new DelegateCommand(() => CRRPeriod());

            CRRaution = new DelegateCommand(() => CRRAution());

            Tempgraph = new DelegateCommand(() => TempGraph());
            H_Temp_Graph = new DelegateCommand(() => HTempGraph());
            Renewable_Graph = new DelegateCommand(() => RenewableGraph());

            CRR_top = new DelegateCommand(() => CRRTop10P());

            Crrpath = new DelegateCommand(() => CRRPathAnalyzer());

            Powergen = new DelegateCommand(() => PowerGeneration());

            LTC = new DelegateCommand(() => LTCScreen());

            LTCAnnual_Cmd = new DelegateCommand(() => LTCAnnualScreen());

            CRRpnl = new DelegateCommand(() => CRRpnlScreen());

            Nodeprice = new DelegateCommand(() => NodePriceGrpah());

            loadforcast = new DelegateCommand(() => Loadforcast());
            NodePriceHourly = new DelegateCommand(() => Node_Price_Hourly());
            fuelmix = new DelegateCommand(() => FuelMix());

            if (_user == "rwaterston"  || _user == "gojira" || _user == "sangramp" || _user == "hjaybhay")
            {
                UTCRiskCopyEnabled = true;
                AdminEnabled = true;
                bid_evEnabled = true;
                performanceEnabled = true;
                aclevEnabled = true;
                ReconEnabled = true;

            }
            else
            {
                UTCRiskCopyEnabled = false;
                AdminEnabled = false;
                bid_evEnabled = false;
                performanceEnabled = false;
                aclevEnabled = false;
                ReconEnabled = false;
            }
            
        }
    }
}
