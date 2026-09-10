using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.CommonAccessLibrary;

namespace Vayu.ConstraintContingencyHistory.ViewModels
{
    public class ErcotSensitivitiesViewModel : BindableBase
    {
        /// <summary>
        /// Initializes a new instance of the ErcotSensitivitiesViewModel class.
        /// </summary>
        /// 
        List<SensitivityHelper> UpdatedSensitivityList = null;
        string mUser = Environment.UserName;
        //  string mUser = "abc";
        public DelegateCommand CommitUpdatedShiftFactors { get; set; }
        public DelegateCommand UpdateShiftFactors { get; set; }
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
        private bool _isUpdateEnabled;

        public bool isUpdateEnabled
        {
            get { return _isUpdateEnabled; }
            set
            {
                _isUpdateEnabled = value;
                RaisePropertyChanged("isUpdateEnabled");
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
        public ErcotSensitivitiesViewModel(List<SensitivityHelper> sensList, int constraintNum, string constraint, string continhency, bool isDA)
        {
            if (mUser == "neelams" || mUser == "darshand" || mUser == "gojira" || mUser == "sangramp")
            {
                isUpdateEnabled = true;
            }
            else
                isUpdateEnabled = false;
            isCommitEnabled = false;
            if (isDA)
                type = "DA";
            else
                type = "RT";
            UpdatedSensitivityList = new List<SensitivityHelper>();
            CommitUpdatedShiftFactors = new DelegateCommand(() => CommitSHiftFacotos());
            UpdateShiftFactors = new DelegateCommand(() => Update());
            ConstraintID = "ID: " + constraintNum.ToString();
            ConstraintName = "Constraint Name: " + constraint;
            ContingencyName = "Contingency Name:    " + continhency;
            Markettype = "Market Type: " + type;
            SensitivityList = sensList.ToList();
        }
        private void Update()
        {
            if (SelectedNode == null)
            {
                MessageBox.Show("Please Select a Node To Update");
                return;
            }
            SensitivityHelper updatedSensitivity = SelectedNode;
            updatedSensitivity.Source = "AppUpdated";
            UpdatedSensitivityList.Add(updatedSensitivity);
            if (UpdatedSensitivityList.Count > 0)
            {
                isCommitEnabled = true;
            }

        }

        private void CommitSHiftFacotos()
        {
            if (UpdatedSensitivityList == null || UpdatedSensitivityList.Count == 0)
            {
                Console.WriteLine("Please Select Atleast one node for Updating");
                return;
            }
            UpdateCommittedShiftFactors(UpdatedSensitivityList);
        }

        private void UpdateCommittedShiftFactors(List<SensitivityHelper> updatedSensitivityList)
        {
            int updatedCount = 0;
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.Connection = con;

                    foreach (SensitivityHelper item in updatedSensitivityList)
                    {
                        cmd.CommandText = "update RTMasterVector_new set Sensitivity =" + item.Sensitivity.ToString() + " , Source = 'APP' where ConstraintRTNum = " + item.ConstraintId.ToString() + " and NodeKey = " + item.NodeKey.ToString();
                        try
                        {
                            int i = cmd.ExecuteNonQuery();
                            if (i == 1)
                            {
                                updatedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error in Updating FOr Node" + item.NodeName.ToString());
                        }

                    }
                    con.Close();
                    if (updatedCount == updatedSensitivityList.Count)
                    {
                        MessageBox.Show("All Nodes Updated Successfully");
                    }
                }

            }
            isCommitEnabled = false;
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
}
