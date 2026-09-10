using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using Vayu.Invoice.Models;

namespace Vayu.Invoice.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Properties
        private readonly IDataService mDataService;
        /// <summary>
        /// The m constraint contingency list
        /// </summary>
        private List<InvoiceSettlement> mInvoiceList = new List<InvoiceSettlement>();
        /// <summary>
        /// Gets or sets the constraint contingency list.
        /// </summary>
        /// <value>
        /// The constraint contingency list.
        /// </value>
        public List<InvoiceSettlement> InvoiceList
        {
            get
            {
                return mInvoiceList;
            }
            set
            {
                mInvoiceList = value;
                RaisePropertyChanged("InvoiceList");
            }
        }
        /// <summary>
        /// The m source sink node list
        /// </summary>
        private DateTime mStartDate;
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate
        {
            get
            {
                return mStartDate;
            }
            set
            {
                mStartDate = value.Date;
                RaisePropertyChanged("StartDate");
                //FetchDataAndUpdateChartCommand();
            }
        }
        /// <summary>
        /// The m end date
        /// </summary>
        private DateTime mEndDate;
        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime EndDate
        {
            get
            {
                return mEndDate;
            }
            set
            {
                mEndDate = value;
                RaisePropertyChanged("EndDate");
            }
        }
        /// <summary>
        /// The m source sink
        /// </summary>
        private double? mSourceCong;
        /// <summary>
        /// Gets or sets the source cong.
        /// </summary>
        /// <value>
        /// The source cong.
        /// </value>
        public double? SourceCong
        {
            get
            {
                return mSourceCong;
            }
            set
            {
                if (value.Equals(double.NaN) || value == null)
                {
                    mSourceCong = null;
                }
                else
                {
                    mSourceCong = value;
                }
                RaisePropertyChanged("SourceCong");
            }
        }
        /// <summary>
        /// The m sink cong
        /// </summary>
        private double? mSinkCong;
        /// <summary>
        /// Gets or sets the sink cong.
        /// </summary>
        /// <value>
        /// The sink cong.
        /// </value>
        public double? SinkCong
        {
            get
            {
                return mSinkCong;
            }
            set
            {
                if (value.Equals(double.NaN) || value == null)
                {
                    mSinkCong = null;
                }
                else
                {
                    mSinkCong = value;
                }
                RaisePropertyChanged("SinkCong");
            }
        }
        /// <summary>
        /// The m source price
        /// </summary>
        private double? mSourcePrice;
        /// <summary>
        /// Gets or sets the source price.
        /// </summary>
        /// <value>
        /// The source price.
        /// </value>
        public double? SourcePrice
        {
            get
            {
                return mSourcePrice;
            }
            set
            {
                if (value.Equals(double.NaN) || value == null)
                {
                    mSourcePrice = null;
                }
                else
                {
                    mSourcePrice = value;
                }
                RaisePropertyChanged("SourcePrice");
            }
        }
        /// <summary>
        /// The m sink price
        /// </summary>
        private double? mSinkPrice;
        /// <summary>
        /// Gets or sets the sink price.
        /// </summary>
        /// <value>
        /// The sink price.
        /// </value>
        public double? SinkPrice
        {
            get
            {
                return mSinkPrice;
            }
            set
            {
                if (value.Equals(double.NaN) || value == null)
                {
                    mSinkPrice = null;
                }
                else
                {
                    mSinkPrice = value;
                }
                RaisePropertyChanged("SinkPrice");
            }
        }
        /// <summary>
        /// The m hour
        /// </summary>
        private int mHour;
        /// <summary>
        /// Gets or sets the hour.
        /// </summary>
        /// <value>
        /// The hour.
        /// </value>
        public int Hour
        {
            get
            {
                return mHour;
            }
            set
            {
                mHour = value;
                RaisePropertyChanged("Hour");
            }
        }
        /// <summary>
        /// Gets or sets the market.
        /// </summary>
        /// <value>
        /// The market.
        /// </value>
        public int Market { get; set; }
        /// <summary>
        /// The mconstraint
        /// </summary>
        private string mconstraint;
        /// <summary>
        /// Gets or sets the constraint.
        /// </summary>
        /// <value>
        /// The constraint.
        /// </value>
        public string Constraint
        {
            get
            {
                return mconstraint;
            }
            set
            {
                mconstraint = value;
                RaisePropertyChanged("Constraint");
            }
        }
        /// <summary>
        /// The mSource
        /// </summary>
        private string mSource;
        /// <summary>
        /// Gets or sets the Source.
        /// </summary>
        /// <value>
        /// The Source.
        /// </value>
        public string Source
        {
            get
            {
                return mSource;
            }
            set
            {
                mSource = value;
                RaisePropertyChanged("Source");
            }
        }
        /// <summary>
        /// The mSink 
        /// </summary>
        /// <summary>
        /// The mSink 
        /// </summary>
        private string mSink;
        /// <summary>
        /// Gets or sets the Sink .
        /// </summary>
        /// <value>
        /// The Sink .
        /// </value>
        public string Sink
        {
            get
            {
                return mSink;
            }
            set
            {
                mSink = value;
                RaisePropertyChanged("Sink");
            }
        }

        private string mSource15minsLMP;
        public string Source15minsLMP
        {
            get
            {
                return mSource15minsLMP;
            }
            set
            {
                mSource15minsLMP = value;
                RaisePropertyChanged("Source15minsLMP");
            }
        }

        private string mSink15minsLMP;
        public string Sink15minsLMP
        {
            get
            {
                return mSink15minsLMP;
            }
            set
            {
                mSink15minsLMP = value;
                RaisePropertyChanged("Sink15minsLMP");
            }
        }

        private string mSink_Source;
        public string Sink_Source
        {
            get
            {
                return mSink_Source;
            }
            set
            {
                mSink_Source = value;
                RaisePropertyChanged("Sink_Source");
            }
        }
        /// <summary>
        /// The mcontingency
        /// </summary>
        private string mcontingency;
        /// <summary>
        /// Gets or sets the contingency.
        /// </summary>
        /// <value>
        /// The contingency.
        /// </value>
        public string Contingency
        {
            get
            {
                return mcontingency;
            }
            set
            {
                mcontingency = value;
                RaisePropertyChanged("Contingency");
            }
        }
        /// <summary>
        /// Gets or sets the shadow price.
        /// </summary>
        /// <value>
        /// The shadow price.
        /// </value>
        public double ShadowPrice { get; set; }
        /// <summary>
        /// The m maximum dart
        /// </summary>
        private double? mMaxDart;
        /// <summary>
        /// Gets or sets the maximum dart.
        /// </summary>
        /// <value>
        /// The maximum dart.
        /// </value>
        public double? MaxDart
        {
            get
            {
                return mMaxDart;
            }
            set
            {
                mMaxDart = value;
                RaisePropertyChanged("MaxDart");
            }
        }
        /// <summary>
        /// The m minimum dart
        /// </summary>
        private double? mMinDart;
        private LoadModel loadModel;

        /// <summary>
        /// Gets or sets the minimum dart.
        /// </summary>
        /// <value>
        /// The minimum dart.
        /// </value>
        public double? MinDart
        {
            get
            {
                return mMinDart;
            }
            set
            {
                mMinDart = value;
                RaisePropertyChanged("MinDart");
            }
        }

        /// <summary>
        /// Gets or sets the run retrieve fetch data and update chart command.
        /// </summary>
        /// <value>
        /// The run retrieve fetch data and update chart command.
        /// </value>
        public DelegateCommand RunRetrieveFetchDataAndUpdateChartCommand { private set; get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="dataService">The data service.</param>
        public MainWindowViewModel(IDataService dataService)
        {
            mDataService = dataService;
            mDataService.loadDBCommands();
            // mDataService.GetConstraintContingencyData();
            RunRetrieveFetchDataAndUpdateChartCommand = new DelegateCommand(FetchDataAndUpdateChartCommand);
        }

        public MainWindowViewModel(LoadModel loadModel)
        {
            this.loadModel = loadModel;
            this.loadModel.loadDBCommands();

            // mDataService.GetConstraintContingencyData();
            RunRetrieveFetchDataAndUpdateChartCommand = new DelegateCommand(FetchDataAndUpdateChartCommand);
        }

        #region Public Methods

        /// <summary>
        /// Sets the data.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="sourceSink">The source sink.</param>
        /// <param name="maxDart">The maximum dart.</param>
        /// <param name="minDart">The minimum dart.</param>
        public void SetData(string desc)
        {
            ////StartDate = startDate;
            //EndDate = endDate;
            //SourceSink = sourceSink;
            //SourceSinkNodeList.Add(sourceSink);
            GetInvoiceData(desc);

            //if (mLoadList.Count > 0)
            {
                RefreshChart();
            }
        }

        /// <summary>
        /// Fetches the data and updates the chart.
        /// </summary>
        public void FetchDataAndUpdateChartCommand()
        {

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the constraint contingency data.
        /// </summary>
        private void GetInvoiceData(string desc)
        {
            if (StartDate == null)
            {
                return;
            }
            this.loadModel.GetInvoiceData((Invoicelist, error) =>
            {
                if (error != null)
                {
                    return;
                }
                InvoiceList = Invoicelist;
            }, desc);
        }

        /// <summary>
        /// Gets the load data.
        /// </summary>
        private void GetLoadData()
        {
            if (StartDate == null)
            {
                return;
            }
            if (EndDate < StartDate)
            {
                return;
            }

        }

        /// <summary>
        /// Refreshes the chart.
        /// </summary>
        private void RefreshChart()
        {
            // PlotModel = CreatePlotModel();
        }

        /// <summary>
        /// Creates the plot model.
        /// </summary>
        /// <returns></returns>
        /// Gets the source sink prices.
        /// </summary>
        #endregion
    }
    public class Item
    {
        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public int? X { get; set; }
        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        public double? Y { get; set; }
    }
}
