
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using Vayu.CommonAccessLibrary;
using Vayu.PowerMap.Model;

namespace Vayu.PowerMap.Views
{
    /// <summary>
    /// Interaction logic for IIRReprtWindow.xaml
    /// </summary>
    public partial class IIRReprtWindow : Window, INotifyPropertyChanged
    {
        #region Declaration

        /// <summary>
        /// The m report data
        /// </summary>
        private Dictionary<string, List<IIRReport>> mReportData = new Dictionary<string, List<IIRReport>>();
        /// <summary>
        /// The m temporary model
        /// </summary>
        private PlotModel mTempModel;
        /// <summary>
        /// The lock object
        /// </summary>
        private static readonly object lockObj = new object();
        /// <summary>
        /// The m oxy color list
        /// </summary>
        private Dictionary<string, OxyColor> mOxyColorList;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Sigma database connection.
        /// </summary>
        /// <value>
        /// The Sigma database connection.
        /// </value>
        private SqlConnection VayuConnection { get; set; }
        /// <summary>
        /// Gets or sets the m select pjmiir outage report command.
        /// </summary>
        /// <value>
        /// The m select pjmiir outage report command.
        /// </value>
        private SqlCommand mSelectPJMIIROutageReportCommand { get; set; }
        /// <summary>
        /// Gets or sets the m select ercot iir outage report command.
        /// </summary>
        /// <value>
        /// The m select ercot iir outage report command.
        /// </value>
        private SqlCommand mSelectErcotIIROutageReportCommand { get; set; }
        /// <summary>
        /// Gets or sets the m select misoiir outage report command.
        /// </summary>
        /// <value>
        /// The m select misoiir outage report command.
        /// </value>
        private SqlCommand mSelectMISOIIROutageReportCommand { get; set; }
        /// <summary>
        /// Gets or sets the m select nyisoiir outage report command.
        /// </summary>
        /// <value>
        /// The m select nyisoiir outage report command.
        /// </value>
        private SqlCommand mSelectNYISOIIROutageReportCommand { get; set; }
        /// <summary>
        /// Gets or sets the m select sppiir outage report command.
        /// </summary>
        /// <value>
        /// The m select sppiir outage report command.
        /// </value>
        private SqlCommand mSelectSPPIIROutageReportCommand { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="IIRReprtWindow"/> class.
        /// </summary>
        public IIRReprtWindow()
        {
            InitializeComponent();
            //RefereshPlot();
            InItDB();
        }
        /// <summary>
        /// The m iir report list
        /// </summary>
        private List<IIRReport> mIIRReportList;
        /// <summary>
        /// Gets or sets the iir report list.
        /// </summary>
        /// <value>
        /// The iir report list.
        /// </value>
        public List<IIRReport> IIRReportList
        {
            get
            {
                return mIIRReportList;
            }
            set
            {
                mIIRReportList = value;
                RaisePropertyChanged("IIRReportList");
            }
        }
        /// <summary>
        /// The m report list
        /// </summary>
        private List<IIRReport> mReportList;
        /// <summary>
        /// Gets or sets the report list.
        /// </summary>
        /// <value>
        /// The report list.
        /// </value>
        public List<IIRReport> ReportList
        {
            get
            {
                return mReportList;
            }
            set
            {
                mReportList = value;
                RaisePropertyChanged("ReportList");
            }
        }
        /// <summary>
        /// The m plot data model
        /// </summary>
        private OxyPlot.PlotModel mPlotDataModel;
        /// <summary>
        /// Gets or sets the plot data model.
        /// </summary>
        /// <value>
        /// The plot data model.
        /// </value>
        public OxyPlot.PlotModel PlotDataModel
        {
            get
            {
                return mPlotDataModel;
            }
            set
            {
                mPlotDataModel = value;
                RaisePropertyChanged("PlotDataModel");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Refereshes the plot.
        /// </summary>
        private void RefereshPlot()
        {
            if (ReportList != null && ReportList.Count > 0)
            {
                lock (lockObj)
                {
                    try
                    {
                        PlotDataModel = new PlotModel();
                        mTempModel = new PlotModel();
                        if (ReportList == null || ReportList.Count == 0)
                            return;
                        mTempModel.Axes.Add(new LinearAxis
                        {
                            Key = "Y Axis",
                            MajorGridlineStyle = LineStyle.Solid,
                            MinorGridlineStyle = LineStyle.Dot,
                            MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                            MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                            IsPanEnabled = false,
                            IsZoomEnabled = true,
                            MaximumPadding = 0.5,
                            MinimumPadding = 0.1,
                            TextColor = OxyColors.White,
                            TitleColor = OxyColors.WhiteSmoke,
                            EndPosition = 1,
                            Position = AxisPosition.Left,
                            Title = "MW ------>",
                            AxisTitleDistance = 0

                        });
                        mTempModel.Axes.Add(new LinearAxis
                        {
                            Key = "X Axis",
                            Title = "Days ----->",
                            Position = AxisPosition.Bottom,
                            TextColor = OxyColors.White,
                            TitleColor = OxyColors.WhiteSmoke,
                            AxisTitleDistance = 0,
                            MajorGridlineStyle = LineStyle.Solid,
                            AxislineThickness = 3,
                            MinorGridlineStyle = LineStyle.Dot,
                            MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                            MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255)
                        });
                        try
                        {
                            if (mOxyColorList == null)
                            {
                                FillOxyColors();
                            }
                            foreach (var item in mReportData.Keys)
                            {
                                mTempModel.Series.Add(CreateAreaSeries(mReportData[item].OrderBy(i => i.Days).ToList(), item));
                            }
                            var l = new Legend
                            {
                                LegendOrientation = LegendOrientation.Horizontal,
                                LegendPlacement = LegendPlacement.Inside,
                                LegendPosition = LegendPosition.BottomLeft,
                                LegendTextColor = OxyColors.White,
                            };

                            PlotDataModel.Legends.Add(l);

                            mTempModel.TitlePadding = 3;
                            mTempModel.IsLegendVisible = true;
                            PlotDataModel = mTempModel;

                        }
                        catch (Exception)
                        {

                            //throw;
                        }
                    }
                    catch (Exception EX)
                    {

                        // throw;
                    }
                }
            }
        }
        /// <summary>
        /// Ins it database.
        /// </summary>
        private void InItDB()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();


            //PJM Load
            mSelectPJMIIROutageReportCommand = new SqlCommand();
            mSelectPJMIIROutageReportCommand.CommandText = "select CapOffline,UnitName,PlantName,PowerUsage from pjm.IIROutage where  StartDate<=@StartDate and EndDate>=@EndDate" +
                                                             " and ISORTORegion='PJM' and HeatRate> 0 GROUP BY CapOffline,UnitName,PlantName,PowerUsage";
            mSelectPJMIIROutageReportCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectPJMIIROutageReportCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectPJMIIROutageReportCommand.Connection = VayuConnection;
            //MISO Load
            mSelectMISOIIROutageReportCommand = new SqlCommand();
            mSelectMISOIIROutageReportCommand.CommandText = "select CapOffline,UnitName,PlantName,PowerUsage from MISO.IIROutage where  StartDate<=@StartDate and EndDate>=@EndDate" +
                                                             " and ISORTORegion='MISO' and HeatRate> 0 GROUP BY CapOffline,UnitName,PlantName,PowerUsage";
            mSelectMISOIIROutageReportCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectMISOIIROutageReportCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectMISOIIROutageReportCommand.Connection = VayuConnection;

            //ERCOT Load
            mSelectErcotIIROutageReportCommand = new SqlCommand();
            mSelectErcotIIROutageReportCommand.CommandText = "select CapOffline,UnitName,PlantName,PowerUsage from ERCOT.IIROutage where  StartDate<=@StartDate and EndDate>=@EndDate" +
                                                             " and ISORTORegion='ERCOT' and HeatRate> 0 GROUP BY CapOffline,UnitName,PlantName,PowerUsage";
            mSelectErcotIIROutageReportCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectErcotIIROutageReportCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectErcotIIROutageReportCommand.Connection = VayuConnection;

            //NYISO Load
            mSelectNYISOIIROutageReportCommand = new SqlCommand();
            mSelectNYISOIIROutageReportCommand.CommandText = "select CapOffline,UnitName,PlantName,PowerUsage from NYISO.IIROutage where  StartDate<=@StartDate and EndDate>=@EndDate" +
                                                             " and ISORTORegion='NYISO' and HeatRate> 0 GROUP BY CapOffline,UnitName,PlantName,PowerUsage";
            mSelectNYISOIIROutageReportCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectNYISOIIROutageReportCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectNYISOIIROutageReportCommand.Connection = VayuConnection;

            //SPP Load
            mSelectSPPIIROutageReportCommand = new SqlCommand();
            mSelectSPPIIROutageReportCommand.CommandText = "select CapOffline,UnitName,PlantName,PowerUsage from SPP.IIROutage where  StartDate<=@StartDate and EndDate>=@EndDate" +
                                                             " and ISORTORegion='SPP' and HeatRate> 0 GROUP BY CapOffline,UnitName,PlantName,PowerUsage";
            mSelectSPPIIROutageReportCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            mSelectSPPIIROutageReportCommand.Parameters.AddWithValue("@EndDate", "EndDate");
            mSelectSPPIIROutageReportCommand.Connection = VayuConnection;
        }
        /// <summary>
        /// Fills the oxy colors.
        /// </summary>
        private void FillOxyColors()
        {
            mOxyColorList = new Dictionary<string, OxyColor>();
            mOxyColorList.Add("Base Load", OxyColors.MidnightBlue);
            mOxyColorList.Add("Peak Load", OxyColors.DarkGray);
            mOxyColorList.Add("Intermediate Load", OxyColors.CornflowerBlue);
        }
        /// <summary>
        /// Creates the area series.
        /// </summary>
        /// <param name="listData">The list data.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private AreaSeries CreateAreaSeries(List<IIRReport> listData, string key)
        {
            //CategoryAxis categoryAxis = new CategoryAxis();
            //categoryAxis = new CategoryAxis(AxisPosition.Bottom)
            //{
            //    MajorGridlineStyle = LineStyle.Solid,
            //    //MajorGridlineColor = OxyColor.FromAColor(20, c),
            //    Angle = 0, /* 90 */
            //    StringFormat = "0",
            //    MajorStep = 1,
            //    IsPanEnabled = true,
            //    IsZoomEnabled = true,
            //    MaximumPadding = 0,
            //    MinimumPadding = 0,
            //    StartPosition = 0.005,
            //    EndPosition = 0.995,
            //    TickStyle = TickStyle.None,
            //    FontSize = 10,
            //    IsTickCentered = true,
            //    Key = "XAxisBCategory",
            //    GapWidth = 0.05,
            //    AxisTitleDistance = 2,
            //    AxisTickToLabelDistance = 0,
            //    TextColor = OxyColors.Transparent
            //};
            //PlotDataModel.Axes.Add(categoryAxis);
            var areaSeries1 = new AreaSeries()
            {
                //Fill = OxyColors.LightBlue,
                DataFieldX2 = "X",
                DataFieldY2 = "Minimum",
                //Color = OxyColors.Black,
                StrokeThickness = 1,
                MarkerFill = OxyColors.Transparent,
                DataFieldX = "X",
                DataFieldY = "Maximum",
                LineStyle = LineStyle.Solid,
                CanTrackerInterpolatePoints = false,
                // TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0.00}",
                XAxisKey = "X",
                YAxisKey = "Y",
                Fill = mOxyColorList.ContainsKey(key) ? mOxyColorList[key] : OxyColors.MediumPurple,
                Title = key.ToUpper(),
                TextColor = OxyColors.Black
            };
            List<GraphItem> lstGraph = new List<GraphItem>();
            if (key == "Base Load")
            {
                listData.RemoveAll(a => a.MWBase == 0);
                try
                {
                    foreach (var gitem in listData)
                    {
                        areaSeries1.Points.Add(new DataPoint(gitem.Days, gitem.MWBase));
                        areaSeries1.Points2.Add(new DataPoint(gitem.Days, 0));
                    }

                }
                catch (Exception)
                {

                    throw;
                }
            }
            else if (key == "Peak Load")
            {
                listData.RemoveAll(a => a.MWPeak == 0);
                try
                {
                    foreach (var gitem in listData)
                    {
                        areaSeries1.Points.Add(new DataPoint(gitem.Days, gitem.MWPeak));
                        areaSeries1.Points2.Add(new DataPoint(gitem.Days, 0));
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            else if (key == "Intermediate Load")
            {
                listData.RemoveAll(a => a.MWIntermed == 0);
                try
                {

                    foreach (var gitem in listData)
                    {
                        areaSeries1.Points.Add(new DataPoint(gitem.Days, gitem.MWIntermed));
                        areaSeries1.Points2.Add(new DataPoint(gitem.Days, 0));
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return areaSeries1;
        }

        #endregion

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="Markets">The markets.</param>
        /// <param name="mMarketInContext">The m market in context.</param>
        /// <param name="startDate">The start date.</param>
        public void GetData(Dictionary<string, int> Markets, int mMarketInContext, DateTime startDate)
        {
            DateTime StartDate = startDate;
            IIRReportList = new List<IIRReport>();
            DateTime endDate = startDate;
            if (!Markets.Values.Contains(mMarketInContext))
                return;
            if (VayuConnection == null)
                return;
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            for (int i = 0; i < 365; i++)
            {
                if (i != 0)
                {
                    StartDate = StartDate.AddDays(1);
                    //endDate = StartDate.AddDays(1);
                }
                SqlDataReader reader = null;
                if (mMarketInContext == Markets["ERCOT"])
                {
                    mSelectErcotIIROutageReportCommand.Parameters["@StartDate"].Value = StartDate.ToString("yyyy-MM-dd");
                    mSelectErcotIIROutageReportCommand.Parameters["@EndDate"].Value = StartDate.ToString("yyyy-MM-dd");
                    reader = mSelectErcotIIROutageReportCommand.ExecuteReader();
                }
                while (reader.Read())
                {
                    IIRReport report = new IIRReport();
                    report.ReportDate = StartDate;
                    string PowerUsage = reader.IsDBNull(3) ? "" : Convert.ToString(reader.GetValue(3));
                    if (PowerUsage == "Base Load")
                    {
                        report.MWBase = reader.IsDBNull(0) ? 0 : Convert.ToDouble(reader.GetValue(0));
                    }
                    if (PowerUsage == "Peak Load")
                    {
                        report.MWPeak = reader.IsDBNull(0) ? 0 : Convert.ToDouble(reader.GetValue(0));
                    }
                    if (PowerUsage == "Intermediate Load")
                    {
                        report.MWIntermed = reader.IsDBNull(0) ? 0 : Convert.ToDouble(reader.GetValue(0));
                    }
                    report.PowerUsage = PowerUsage;
                    IIRReportList.Add(report);
                }
                reader.Close();
                //SigmaDbConn.Close();
            }
            int days = 0;
            if (IIRReportList.Count > 0)
            {
                ReportList = new List<IIRReport>();
                List<IIRReport> plotList = new List<IIRReport>();
                foreach (var item in IIRReportList.GroupBy(x => x.ReportDate))
                {
                    IIRReport report = new IIRReport();
                    DateTime dt = item.Key;
                    report.ReportDate = dt;
                    DayOfWeek dw = dt.DayOfWeek;
                    report.Day = dw.ToString().Substring(0, 3);
                    days = days + 1;
                    report.Days = days;
                    report.MWBase = Math.Round(item.Sum(x => x.MWBase));
                    report.MWPeak = Math.Round(item.Sum(x => x.MWPeak));
                    report.MWIntermed = Math.Round(item.Sum(x => x.MWIntermed));
                    ReportList.Add(report);
                    if (days <= 90)
                    {
                        plotList.Add(report);
                    }
                }
                List<IIRReport> lstMWBase = plotList.Select(x => new IIRReport { MWBase = x.MWBase, Days = x.Days, PowerUsage = x.PowerUsage }).ToList<IIRReport>();
                List<IIRReport> lstMWPeak = plotList.Select(x => new IIRReport { MWPeak = x.MWPeak, Days = x.Days, PowerUsage = x.PowerUsage }).ToList<IIRReport>();
                List<IIRReport> lstMWIntermed = plotList.Select(x => new IIRReport { MWIntermed = x.MWIntermed, Days = x.Days, PowerUsage = x.PowerUsage }).ToList<IIRReport>();
                mReportData.Add("Base Load", lstMWBase);
                mReportData.Add("Intermediate Load", lstMWIntermed);
                mReportData.Add("Peak Load", lstMWPeak);
                RefereshPlot();
            }
        }

        #region INotification
        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
    /// <summary>
    /// 
    /// </summary>
    public class GraphItem
    {
        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public int X { get; set; }
        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        public double? Y { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ReportItem
    {
        /// <summary>
        /// Gets or sets the days.
        /// </summary>
        /// <value>
        /// The days.
        /// </value>
        public int Days { get; set; }
        /// <summary>
        /// Gets or sets the mw base.
        /// </summary>
        /// <value>
        /// The mw base.
        /// </value>
        public double? MWBase { get; set; }
        /// <summary>
        /// Gets or sets the mw peak.
        /// </summary>
        /// <value>
        /// The mw peak.
        /// </value>
        public double? MWPeak { get; set; }
        /// <summary>
        /// Gets or sets the mw intermed.
        /// </summary>
        /// <value>
        /// The mw intermed.
        /// </value>
        public double? MWIntermed { get; set; }
        /// <summary>
        /// Gets or sets the report date.
        /// </summary>
        /// <value>
        /// The report date.
        /// </value>
        public DateTime ReportDate { get; set; }
    }
}
