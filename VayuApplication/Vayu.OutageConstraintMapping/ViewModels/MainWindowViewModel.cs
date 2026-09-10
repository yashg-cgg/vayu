using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Vayu.OutageConstraintMapping.Model;

namespace Vayu.OutageConstraintMapping.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// The m data service
        /// </summary>
        public readonly IDataService mDataService;
        /// <summary>
        /// The selected outage list
        /// </summary>
        List<OutageData> selectedOutageList;

        #region Properties

        /// <summary>
        /// Gets or sets the retrieve.
        /// </summary>
        /// <value>
        /// The retrieve.
        /// </value>
        public DelegateCommand Retrieve { private set; get; }

        /// <summary>
        /// The m start date
        /// </summary>
        private DateTime mStartDate = DateTime.Today;
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
                mStartDate = value;
                RaisePropertyChanged("StartDate");
            }
        }
        /// <summary>
        /// The m end date
        /// </summary>
        private DateTime mEndDate = DateTime.Today.AddDays(1);
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
        /// The m range checked
        /// </summary>
        private bool mRangeChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [range checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [range checked]; otherwise, <c>false</c>.
        /// </value>
        public bool RangeChecked
        {
            get
            {
                return mRangeChecked;
            }
            set
            {
                mRangeChecked = value;
                if (mRangeChecked)
                {
                    IsStartDateEnabled = true;
                    IsEndDateEnabled = true;
                    ConstraintList = null;
                    OutageList = mDataService.GetOutageData(mStartDate, EndDate, true, StartChecked);
                    SearchOutageList = OutageList.Select(a => a.DriverName).ToList();
                    CountoutageList = OutageList.Count;
                }
                else
                {
                    IsStartDateEnabled = false;
                    IsEndDateEnabled = false;
                }
                RaisePropertyChanged("RangeChecked");
            }
        }

        /// <summary>
        /// The m all checked
        /// </summary>
        private bool mAllChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [all checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [all checked]; otherwise, <c>false</c>.
        /// </value>
        public bool AllChecked
        {
            get
            {
                return mAllChecked;
            }
            set
            {
                mAllChecked = value;
                if (mAllChecked)
                {
                    IsStartDateEnabled = false;
                    IsEndDateEnabled = false;
                    OutageList = null;
                    ConstraintList = null;
                    OutageList = mDataService.GetOutageData(mStartDate, EndDate, false, StartChecked);
                    SearchOutageList = OutageList.Select(a => a.DriverName).ToList();
                    CountoutageList = OutageList.Count;
                }
                else
                {
                    IsStartDateEnabled = true;
                    IsEndDateEnabled = true;
                }
                RaisePropertyChanged("AllChecked");
            }
        }

        /// <summary>
        /// The m outage list
        /// </summary>
        private List<OutageData> mOutageList;
        /// <summary>
        /// Gets or sets the outage list.
        /// </summary>
        /// <value>
        /// The outage list.
        /// </value>
        public List<OutageData> OutageList
        {
            get
            {
                return mOutageList;
            }
            set
            {
                mOutageList = value;
                RaisePropertyChanged("OutageList");
            }
        }

        /// <summary>
        /// The m search outage list
        /// </summary>
        private List<string> mSearchOutageList;
        /// <summary>
        /// Gets or sets the search outage list.
        /// </summary>
        /// <value>
        /// The search outage list.
        /// </value>
        public List<string> SearchOutageList
        {
            get
            {
                return mSearchOutageList;
            }
            set
            {
                mSearchOutageList = value;
                RaisePropertyChanged("SearchOutageList");
            }
        }

        /// <summary>
        /// The m search constraint list
        /// </summary>
        private List<string> mSearchConstraintList;
        /// <summary>
        /// Gets or sets the search constraint list.
        /// </summary>
        /// <value>
        /// The search constraint list.
        /// </value>
        public List<string> SearchConstraintList
        {
            get
            {
                return mSearchConstraintList;
            }
            set
            {
                mSearchConstraintList = value;
                RaisePropertyChanged("SearchConstraintList");
            }
        }

        /// <summary>
        /// The m countoutage list
        /// </summary>
        private int mCountoutageList;
        /// <summary>
        /// Gets or sets the countoutage list.
        /// </summary>
        /// <value>
        /// The countoutage list.
        /// </value>
        public int CountoutageList
        {
            get
            {
                return mCountoutageList;
            }
            set
            {
                mCountoutageList = value;
                RaisePropertyChanged("CountoutageList");
            }
        }

        /// <summary>
        /// The m constraint family list
        /// </summary>
        private List<Family> mConstraintFamilyList;
        /// <summary>
        /// Gets or sets the constraint family list.
        /// </summary>
        /// <value>
        /// The constraint family list.
        /// </value>
        public List<Family> ConstraintFamilyList
        {
            get
            {
                return mConstraintFamilyList;
            }
            set
            {
                mConstraintFamilyList = value;
                RaisePropertyChanged("ConstraintFamilyList");
            }
        }

        /// <summary>
        /// The m familt dictionary data
        /// </summary>
        private Dictionary<string, List<string>> mFamiltDictData;
        /// <summary>
        /// Gets or sets the familt dictionary data.
        /// </summary>
        /// <value>
        /// The familt dictionary data.
        /// </value>
        public Dictionary<string, List<string>> FamiltDictData
        {
            get
            {
                return mFamiltDictData;
            }
            set
            {
                mFamiltDictData = value;
                RaisePropertyChanged("FamiltDictData");
            }
        }

        /// <summary>
        /// The m constraint list
        /// </summary>
        private List<Constraint> mConstraintList;
        /// <summary>
        /// Gets or sets the constraint list.
        /// </summary>
        /// <value>
        /// The constraint list.
        /// </value>
        public List<Constraint> ConstraintList
        {
            get
            {
                return mConstraintList;
            }
            set
            {
                mConstraintList = value;
                RaisePropertyChanged("ConstraintList");
            }
        }

        /// <summary>
        /// The m contingency list
        /// </summary>
        private List<Contingency> mContingencyList;
        /// <summary>
        /// Gets or sets the contingency list.
        /// </summary>
        /// <value>
        /// The contingency list.
        /// </value>
        public List<Contingency> ContingencyList
        {
            get
            {
                return mContingencyList;
            }
            set
            {
                mContingencyList = value;
                RaisePropertyChanged("ContingencyList");
            }
        }

        /// <summary>
        /// The m constraint count
        /// </summary>
        private int mConstraintCount = 0;
        /// <summary>
        /// Gets or sets the constraint count.
        /// </summary>
        /// <value>
        /// The constraint count.
        /// </value>
        public int ConstraintCount
        {
            get
            {
                return mConstraintCount;
            }
            set
            {
                mConstraintCount = value;
                RaisePropertyChanged("ConstraintCount");
            }
        }

        /// <summary>
        /// The m search family list
        /// </summary>
        private List<string> mSearchFamilyList;
        /// <summary>
        /// Gets or sets the search family list.
        /// </summary>
        /// <value>
        /// The search family list.
        /// </value>
        public List<string> SearchFamilyList
        {
            get
            {
                return mSearchFamilyList;
            }
            set
            {
                mSearchFamilyList = value;
                RaisePropertyChanged("SearchFamilyList");
            }
        }

        /// <summary>
        /// The m family count
        /// </summary>
        private int mFamilyCount;
        /// <summary>
        /// Gets or sets the family count.
        /// </summary>
        /// <value>
        /// The family count.
        /// </value>
        public int FamilyCount
        {
            get
            {
                return mFamilyCount;
            }
            set
            {
                mFamilyCount = value;
                RaisePropertyChanged("FamilyCount");
            }
        }

        /// <summary>
        /// The m constraint text
        /// </summary>
        private string mConstraintText;
        /// <summary>
        /// Gets or sets the constraint text.
        /// </summary>
        /// <value>
        /// The constraint text.
        /// </value>
        public string ConstraintText
        {
            get
            {
                return mConstraintText;
            }
            set
            {
                mConstraintText = value;
                RaisePropertyChanged("ConstraintText");
            }
        }

        /// <summary>
        /// The m selected outage
        /// </summary>
        private string mSelectedOutage;
        /// <summary>
        /// Gets or sets the selected outage.
        /// </summary>
        /// <value>
        /// The selected outage.
        /// </value>
        public string SelectedOutage
        {
            get
            {
                return mSelectedOutage;
            }
            set
            {
                mSelectedOutage = value;
                //List<Constraint> inConstraintList = mDataService.GetConstraintData(SelectedOutage);
                //SetConstraintList(inConstraintList);
                RaisePropertyChanged("SelectedOutage");
            }
        }

        /// <summary>
        /// The m start checked
        /// </summary>
        private bool mStartChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [start checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [start checked]; otherwise, <c>false</c>.
        /// </value>
        public bool StartChecked
        {
            get
            {
                return mStartChecked;
            }
            set
            {
                mStartChecked = value;
                RaisePropertyChanged("StartChecked");
            }
        }

        /// <summary>
        /// The m is start date enabled
        /// </summary>
        private bool mIsStartDateEnabled = true;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is start date enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is start date enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsStartDateEnabled
        {
            get
            {
                return mIsStartDateEnabled;
            }
            set
            {
                mIsStartDateEnabled = value;
                RaisePropertyChanged("IsStartDateEnabled");
            }
        }

        /// <summary>
        /// The m is end date enabled
        /// </summary>
        private bool mIsEndDateEnabled = true;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is end date enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is end date enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEndDateEnabled
        {
            get
            {
                return mIsEndDateEnabled;
            }
            set
            {
                mIsEndDateEnabled = value;
                RaisePropertyChanged("IsEndDateEnabled");
            }
        }

        #endregion


        public MainWindowViewModel(IDataService dataService)
        {
            mDataService = dataService;
            Retrieve = new DelegateCommand(() => RetrieveData());
            selectedOutageList = new List<OutageData>();
        }



        private void RetrieveData()
        {
            if (RangeChecked)
            {
                if (StartDate != null && EndDate != null)
                {
                    OutageList = mDataService.GetOutageData(mStartDate, EndDate, RangeChecked, StartChecked);
                    SearchOutageList = OutageList.Select(a => a.DriverName).ToList();
                    CountoutageList = OutageList.Count;
                }
            }
            else
            {
                OutageList = mDataService.GetOutageData(mStartDate, EndDate, false, StartChecked);
                SearchOutageList = OutageList.Select(a => a.DriverName).ToList();
                CountoutageList = OutageList.Count;
            }

        }

        #region Public Methods

        /// <summary>
        /// Shows the constraint sesitivity window.
        /// </summary>
        /// <param name="ConstraintText">The constraint text.</param>
        public void ShowConstraintSesitivityWindow(string ConstraintText)
        {
            Vayu.ConstraintSensitivityAlgorithm.ViewModels.MainWindowViewModel model = new ConstraintSensitivityAlgorithm.ViewModels.MainWindowViewModel(new ConstraintSensitivityAlgorithm.Model.DataService());
            var window = new Vayu.ConstraintSensitivityAlgorithm.Views.MainWindow();
            window.DataContext = model;
            model.OpenOutageConstraint(ConstraintText);
            window.Show();
        }
        /// <summary>
        /// Filters the constraintdata.
        /// </summary>
        /// <param name="familyList">The family list.</param>
        public void FilterConstraintdata(List<string> familyList)
        {
            ConstraintList = new List<Constraint>();
            foreach (string Familykey in familyList)
            {
                List<string> templist = FamiltDictData[Familykey];
                List<Constraint> tempConstraint = new List<Constraint>();
                foreach (string value in templist)
                {
                    Constraint cons = new Constraint();
                    cons.ConstraintName = value;
                    tempConstraint.Add(cons);
                }
                ConstraintList.AddRange(tempConstraint);
            }
            ConstraintList = ConstraintList.GroupBy(a => a.ConstraintName).Select(p => p.First()).ToList();
            SearchConstraintList = ConstraintList.Select(a => a.ConstraintName).ToList();
            ConstraintCount = ConstraintList.Count;
            ContingencyList = null;
        }
        /// <summary>
        /// Shows the constraint family.
        /// </summary>
        /// <param name="dictCons">The dictionary cons.</param>
        public void showConstraintFamily(Dictionary<string, List<string>> dictCons)
        {
            FamiltDictData = dictCons;
            ConstraintFamilyList = new List<Family>();

            List<Tuple<String, String, String>> newFamilyList = mDataService.ConstraintFamilyList();

            foreach (var varNewFamilyList in newFamilyList)
            {
                Family familyObj = new Family();
                familyObj.ConstraintFamily = varNewFamilyList.Item1;
                familyObj.ConstraintDriverName = varNewFamilyList.Item3;
                familyObj.ConstraintSource = varNewFamilyList.Item2;
                ConstraintFamilyList.Add(familyObj);
            }

            // Previous Code
            //foreach (string key in dictCons.Keys)
            //{
            //    Family objCons = new Family();
            //    objCons.ConstraintFamily = key;
            //    ConstraintFamilyList.Add(objCons);
            //}
            SearchFamilyList = ConstraintFamilyList.Select(x => x.ConstraintFamily).ToList();
            FamilyCount = SearchFamilyList.Count;
            ContingencyList = null;
        }
        #endregion
    }
}
