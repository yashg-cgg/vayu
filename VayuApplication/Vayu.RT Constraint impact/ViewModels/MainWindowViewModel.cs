using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Vayu.RT_Constraint_impact.Model;

namespace Vayu.RT_Constraint_impact.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Declaration

        /// <summary>
        /// The m data service
        /// </summary>
        private readonly IDataService mDataService;
        /// <summary>
        /// The m start date
        /// </summary>
        private DateTime mStartDate;
        /// <summary>
        /// The m hourly checked
        /// </summary>
        private bool mHourlyChecked;
        /// <summary>
        /// The m constraints list
        /// </summary>
        private List<Constraints> mConstraintsList = new List<Constraints>();
        /// <summary>
        /// Gets or sets the run retrieve fetch data command.
        /// </summary>
        /// <value>
        /// The run retrieve fetch data command.
        /// </value>
        public DelegateCommand RunRetrieveFetchDataCommand { private set; get; }

        #endregion

        public MainWindowViewModel(IDataService dataService)
        {
            MarketList = new List<string>() { "ERCOT" };
            MarketSelected = "ERCOT";
            mDataService = dataService;
            mStartDate = DateTime.Now.Date;
            mHourlyChecked = false;
            mDataService.loadDBCommands();
            RetrieveFetchDataCommand();
            RunRetrieveFetchDataCommand = new DelegateCommand(RetrieveFetchDataCommand);
        }

        /// <summary>
        /// Retrieves the fetch data command.
        /// </summary>
        public void RetrieveFetchDataCommand()
        {
            if (mStartDate == null)
            {
                return;
            }
            mDataService.GetConstraints((conConstraintList, error) =>
            {
                if (error != null)
                {
                    return;
                }
                ConstraintList = conConstraintList.OrderBy(t => t.MarketDate).ToList();
            }, MarketSelected, mStartDate, mHourlyChecked);
        }

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
                if (StartDate != DateTime.MaxValue)
                    RetrieveFetchDataCommand();
            }
        }
        private List<string> _MarketList;

        public List<string> MarketList
        {
            get { return _MarketList; }
            set
            {
                _MarketList = value;
                RaisePropertyChanged("MarketList");
            }
        }

        private string _MarketSelected;

        public string MarketSelected
        {
            get { return _MarketSelected; }
            set
            {
                _MarketSelected = value;
                RaisePropertyChanged("MarketSelected");
                if (MarketSelected != null && StartDate != DateTime.MinValue)
                    RetrieveFetchDataCommand();
            }
        }



        /// <summary>
        /// Gets or sets the constraint list.
        /// </summary>
        /// <value>
        /// The constraint list.
        /// </value>
        public List<Constraints> ConstraintList
        {
            get
            {
                return mConstraintsList;
            }
            set
            {
                mConstraintsList = value;
                RaisePropertyChanged("ConstraintList");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [hourly checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hourly checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HourlyChecked
        {
            get
            {
                return mHourlyChecked;
            }
            set
            {
                mHourlyChecked = value;
                RaisePropertyChanged("HourlyChecked");
                RetrieveFetchDataCommand();
            }

        }
    }
}
