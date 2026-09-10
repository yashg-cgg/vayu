using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.ErcotShiftFactor.Model;

namespace Vayu.ErcotShiftFactor.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        List<SensitivityHelper> UpdatedSensitivityList = null;
        string mUser = Environment.UserName;
        //  string mUser = "abc";
        private IDataService dataService;

        #region Properties

        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        /// <value>
        /// The refresh command.
        /// </value>
        public DelegateCommand RefreshCommand { get; private set; }
        /// <summary>
        /// Gets the clear command.
        /// </summary>
        /// <value>
        /// The clear command.
        /// </value>
        public DelegateCommand ClearCommand { get; private set; }

        private List<SensitivityHelper> _SensitivityList;
        bool isEdited = false;
        string type = string.Empty;
        public List<SensitivityHelper> SensitivityList
        {
            get { return _SensitivityList; }
            set
            {
                _SensitivityList = value;
                RaisePropertyChanged("SensitivityList");
            }
        }
        private string _ConstraintID;

        public string ConstraintID
        {
            get { return _ConstraintID; }
            set
            {
                _ConstraintID = value;
                RaisePropertyChanged("ConstraintID");
            }
        }
        private string _ConstraintName;

        public string ConstraintName
        {
            get { return _ConstraintName; }
            set
            {
                _ConstraintName = value;
                RaisePropertyChanged("ConstraintName");
            }
        }

        private string _ContingencyName;

        public string ContingencyName
        {
            get { return _ContingencyName; }
            set
            {
                _ContingencyName = value;
                RaisePropertyChanged("ContingencyName");
            }
        }


        private string mMarkettype;
        public string Markettype
        {
            get { return mMarkettype; }
            set
            {
                mMarkettype = value;
                RaisePropertyChanged("Markettype");
            }
        }
        private SensitivityHelper _SelectedNode;

        public SensitivityHelper SelectedNode
        {
            get { return _SelectedNode; }
            set
            {
                _SelectedNode = value;
                RaisePropertyChanged("SelectedNode");
            }
        }

        private bool _isCommitEnabled;

        public bool isCommitEnabled
        {
            get { return _isCommitEnabled; }
            set
            {
                _isCommitEnabled = value;
                RaisePropertyChanged("isCommitEnabled");
            }
        }

        //changes by sangramp
        /// <summary>
        /// The selected constraint item
        /// </summary>
        private string _SelectedConstraintItem;
        /// <summary>
        /// Gets or sets the selected constraint item.
        /// </summary>
        /// <value>
        /// The selected constraint item.
        /// </value>
        public string SelectedConstraintItem
        {
            get { return _SelectedConstraintItem; }
            set
            {
                _SelectedConstraintItem = value;
                if (SelectedConstraintItem != null)
                {
                    if (SelectedConstraintItem == null)
                    {
                        if (RTChecked)
                            GetAllContingencies(SelectedConstraintItem, true);
                        else if (DAChecked)
                            GetAllContingencies(SelectedConstraintItem, false);
                    }
                }
                RaisePropertyChanged("SelectedConstraintItem");
            }
        }
        /// <summary>
        /// The selected contengency item
        /// </summary>
        private string _SelectedContengencyItem;
        /// <summary>
        /// Gets or sets the selected contengency item.
        /// </summary>
        /// <value>
        /// The selected contengency item.
        /// </value>
        public string SelectedContengencyItem
        {
            get { return _SelectedContengencyItem; }
            set
            {
                _SelectedContengencyItem = value;
                RaisePropertyChanged("SelectedContengencyItem");
                if (SelectedContengencyItem != null)
                {
                    //GetConstraintHistory();
                }
            }
        }
        /// <summary>
        /// The is constraint checked
        /// </summary>
        private bool _isConstraintChecked;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constraint checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is constraint checked; otherwise, <c>false</c>.
        /// </value>
        public bool isConstraintChecked
        {
            get { return _isConstraintChecked; }
            set
            {
                _isConstraintChecked = value;
                RaisePropertyChanged("isConstraintChecked");
            }
        }

        /// <summary>
        /// The contingency search list
        /// </summary>
        private List<string> _ContingencySearchList;
        /// <summary>
        /// Gets or sets the contingency search list.
        /// </summary>
        /// <value>
        /// The contingency search list.
        /// </value>
        public List<string> ContingencySearchList
        {
            get { return _ContingencySearchList; }
            set
            {
                _ContingencySearchList = value;
                RaisePropertyChanged("ContingencySearchList");
            }
        }
        /// <summary>
        /// The m da checked
        /// </summary>
        private bool mDAChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [da checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [da checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DAChecked
        {
            get { return mDAChecked; }
            set
            {
                mDAChecked = value;
                RaisePropertyChanged("DAChecked");
                ConstraintSearchList = null;
                ContingencySearchList = null;
                fillConstraintSearchList(false);
            }
        }

        /// <summary>
        /// The m rt checked
        /// </summary>
        private bool mRTChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [rt checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [rt checked]; otherwise, <c>false</c>.
        /// </value>
        public bool RTChecked
        {
            get { return mRTChecked; }
            set
            {
                mRTChecked = value;
                RaisePropertyChanged("RTChecked");
                SelectedConstraintItem = null;
                SelectedContengencyItem = null;
                ConstraintList = null;
                ConstraintSearchList = null;
                ContingencySearchList = null;
                fillConstraintSearchList(true);
            }
        }
        /// <summary>
        /// Gets all contingencies.
        /// </summary>
        /// <param name="constraintname">The constraintname.</param>
        /// <param name="isRt">if set to <c>true</c> [is rt].</param>
        private void GetAllContingencies(string constraintname, bool isRt)
        {
            DataService ds = new DataService();
            // ContingencySearchList = ds.GetAllContingencies(constraintname, isRt, 9);
            ContingencySearchList = ds.GetAllContingency(constraintname);
        }
        /// <summary>
        /// The m constraint search list
        /// </summary>
        private List<string> mConstraintSearchList;
        /// <summary>
        /// Gets or sets the constraint search list.
        /// </summary>
        /// <value>
        /// The constraint search list.
        /// </value>
        public List<string> ConstraintSearchList
        {
            get { return mConstraintSearchList; }
            set
            {
                mConstraintSearchList = value;
                RaisePropertyChanged("ConstraintSearchList");

            }
        }

        /// <summary>
        /// The search typed text
        /// </summary>
        private string _SearchTypedText;
        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        /// <value>
        /// The search text.
        /// </value>
        public string SearchTypedText
        {
            get { return _SearchTypedText; }
            set
            {
                _SearchTypedText = value;
                RaisePropertyChanged("SearchTypedText");
            }
        }
        private void fillConstraintSearchList(bool isRt)
        {
            DataService ds = new DataService();
            SearchTypedText = "";

            ConstraintSearchList = ds.FillConstraintsList();

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
            get { return mConstraintList; }
            set
            {
                mConstraintList = value;
                RaisePropertyChanged("ConstraintList");
            }
        }
        #endregion
        public MainWindowViewModel(IDataService _dataService = null)
        {
            dataService = _dataService ?? new DataService();
            RTChecked = true;
            RefreshCommand = new DelegateCommand(() => RefreshList());
            ClearCommand = new DelegateCommand(() => ClearCombos());

        }
        public void ClearCombos()
        {
            SelectedConstraintItem = null;
            SelectedContengencyItem = null;
            SearchTypedText = string.Empty;
            ConstraintSearchList.Clear();

            if (ContingencySearchList != null)
            {
                ContingencySearchList.Clear();
            }

            //RefreshList();

            //this.IsRefreshEnabled = true;
            //Mouse.OverrideCursor = Cursors.Arrow;
        }
        public void RefreshList()
        {
            if (SelectedConstraintItem == null)
            {
                MessageBox.Show("Please select a constraint");
                return;
            }
            bool isDA = false;
            string constraintName = SelectedConstraintItem;
            string contingencyName = SelectedContengencyItem;
            DataService ds = new DataService();
            List<SensitivityHelper> tempsensitivityList = ds.GetErcotSensitivities(constraintName, contingencyName, isDA);
            if (tempsensitivityList != null && tempsensitivityList.Count > 0)
            {
                SensitivityHelper firstHelper = tempsensitivityList.First();
                SensitivityList = tempsensitivityList;

            }
        }
    }

    public class FontColorConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double number;
            double.TryParse(value.ToString(), out number);
            SolidColorBrush myBrush = new SolidColorBrush();
            if (number < 0)
            {
                myBrush = new SolidColorBrush(Colors.Red);
            }
            return number;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class CurrencyColorConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                double val = 0;
                if (double.TryParse(value.ToString(), out val))
                {
                    if (val < 0)
                    {
                        return System.Windows.Media.Brushes.Red;
                    }
                    else
                    {
                        return System.Windows.Media.Brushes.Black;
                    }
                }
                else
                {
                    return System.Windows.Media.Brushes.Black;
                }
            }
            else
            {
                return System.Windows.Media.Brushes.Black;
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }

    public class SensitivityHelper
    {
        public int ConstraintId { get; set; }
        public string Constraint { get; set; }
        public string Contingency { get; set; }
        public string NodeName { get; set; }

        public int NodeKey { get; set; }
        public string Source { get; set; }
        public double Sensitivity { get; set; }
        public string Zone { get; set; }
    }
}
