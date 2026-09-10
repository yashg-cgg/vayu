using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Vayu.DBLibrary;
using Vayu.LMPStatistics.Model;

namespace Vayu.LMPStatistics.ViewModels
{
    public class ConstraintViewModel : BindableBase
    {
        private IDataService dataService;

        public DelegateCommand Refresh { private set; get; }

        private bool mDAChecked;
        public bool DAChecked
        {
            get { return mDAChecked; }
            set
            {
                mDAChecked = value;
                RaisePropertyChanged("DAChecked");
            }
        }
        private bool mRTChecked;
        public bool RTChecked
        {
            get { return mRTChecked; }
            set
            {
                mRTChecked = value;
                RaisePropertyChanged("RTChecked");

            }
        }

        private List<Constraint> sConstraintList;
        public List<Constraint> ConstraintList
        {
            get { return sConstraintList; }
            set
            {
                sConstraintList = value;
                RaisePropertyChanged("ConstraintList");

            }
        }

        private List<Constraint> sPtvConstraintList;
        public List<Constraint> PtvConstraintList
        {
            get { return sPtvConstraintList; }
            set
            {
                sPtvConstraintList = value;
                RaisePropertyChanged("ConstraintList");

            }
        }

        private List<Constraint> sNegConstraintList;
        public List<Constraint> NegConstraintList
        {
            get { return sNegConstraintList; }
            set
            {
                sNegConstraintList = value;
                RaisePropertyChanged("ConstraintList");

            }
        }

        private List<SourceSinkData> mSourceSinkList;
        /// <summary>
        /// Gets or sets the source sink list.
        /// </summary>
        /// <value>
        /// The source sink list.
        /// </value>
        public List<SourceSinkData> SourceSinkList
        {
            get
            {
                return mSourceSinkList;
            }
            set
            {
                mSourceSinkList = value;
                RaisePropertyChanged("SourceSinkList");
            }
        }

        private SourceSinkData mSrcSnkDataSelected;
        /// <summary>
        /// Gets or sets the source or sink data selected.
        /// </summary>
        /// <value>
        /// The source or sink data selected.
        /// </value>
        public SourceSinkData SourceSinkDataSelected
        {
            get
            {
                return mSrcSnkDataSelected;
            }
            set
            {
                mSrcSnkDataSelected = value;

                RaisePropertyChanged("SourceSinkDataSelected");

            }
        }

        private DateTime sStartTime;

        public DateTime StartTime
        {
            get { return sStartTime; }
            set { sStartTime = value; }
        }

        private DateTime sEndTime;

        public DateTime EndTime
        {
            get { return sEndTime; }
            set { sEndTime = value; }
        }

        private Dictionary<string, SourceSinkData> mFillSourceSinkHash = new Dictionary<string, SourceSinkData>();



        public ConstraintViewModel(SourceSinkData sourceSink, DateTime startDate, DateTime endDate)
        {
            RTChecked = true;
            StartTime = startDate;
            EndTime = endDate;
            Refresh = new DelegateCommand(Getconstraints);
            SourceSinkData sourceSinkData = new SourceSinkData();
            sourceSinkData.Source = sourceSink.Source;
            sourceSinkData.Sink = sourceSink.Sink;
            string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                                            sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
            if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
            {
                mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
                SourceSinkDataSelected = sourceSinkData;
            }
            Getconstraints();

        }

        public void Getconstraints()
        {

            List<Constraint> sdt = new List<Constraint>();
            ConstraintList = new List<Constraint>();
            PtvConstraintList = new List<Constraint>();
            NegConstraintList = new List<Constraint>();
            dataService = dataService ?? new DataService();
            List<Constraint> sensitivityList = new List<Constraint>();
            dataService.GetConstraintData((h, e) =>
            {

                DataService ds = new DataService();
                foreach (Constraint c in h)
                {
                    sensitivityList.AddRange(ds.GetErcotSensitivitiesData(c.ConstraintText, c.ContingencyText, SourceSinkDataSelected.Source.ToString(), SourceSinkDataSelected.Sink.ToString(), c.ConstraintDate, c.Sensitivity, StartTime, EndTime, RTChecked));

                }

                ConstraintList = sensitivityList;
                RaisePropertyChanged("ConstraintList");
                if (ConstraintList.Count > 0)
                {
                    ConstraintList = ConstraintList.Where(c => c.Sensitivity != 0).ToList();
                    PtvConstraintList = ConstraintList.Where(c => c.Sensitivity >= 0).ToList();
                    NegConstraintList = ConstraintList.Where(c => c.Sensitivity < 0).ToList();
                    RaisePropertyChanged("PtvConstraintList");
                    RaisePropertyChanged("NegConstraintList");
                }

            }, SourceSinkDataSelected.Source.ToString(), SourceSinkDataSelected.Sink.ToString(), RTChecked, StartTime, EndTime);


        }
    }
}
