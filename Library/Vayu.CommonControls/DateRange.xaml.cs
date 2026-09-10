using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.CommonControls
{
    /// <summary>
    /// Interaction logic for DateRange.xaml
    /// </summary>

    public partial class DateRange : Window
    {
        private SqlConnection VayuConnection;
        private string mUser = Environment.UserName;
        private SqlCommand mInsertDateRangeCommand;
        private SqlCommand mDateRangeComboBoxCommand;
        private SqlCommand mDateRangeComboBoxChangedCommand;
        private SqlCommand mDeleteDateRangeCommand;
        DataTable dt = new DataTable();

        public DateRange()
        {
            InitializeComponent();
            loadDBCommands();
            InitGui();
            Refreshbutton_Click(null, null);
        }

        private void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            mDeleteDateRangeCommand = new SqlCommand();
            mDeleteDateRangeCommand.CommandText = "delete daterange where trader = @trader and datename = @datename";
            mDeleteDateRangeCommand.Parameters.AddWithValue("@trader", "trader");
            mDeleteDateRangeCommand.Parameters.AddWithValue("@datename", "datename");
            mDeleteDateRangeCommand.Connection = VayuConnection;
            //
            mInsertDateRangeCommand = new SqlCommand();
            mInsertDateRangeCommand.CommandText = "insert DateRange values (@trader, @DATENAME, @marketdate)";
            mInsertDateRangeCommand.Parameters.AddWithValue("@trader", "trader");
            mInsertDateRangeCommand.Parameters.AddWithValue("@DATENAME", "DATENAME");
            mInsertDateRangeCommand.Parameters.AddWithValue("@marketdate", "marketdate");
            mInsertDateRangeCommand.Connection = VayuConnection;
            //
            mDateRangeComboBoxCommand = new SqlCommand();
            mDateRangeComboBoxCommand.CommandText = "SELECT distinct DATENAME FROM DateRange where  trader = @trader";
            mDateRangeComboBoxCommand.Parameters.AddWithValue("@trader", "trader");
            mDateRangeComboBoxCommand.Connection = VayuConnection;
            //
            mDateRangeComboBoxChangedCommand = new SqlCommand();
            mDateRangeComboBoxChangedCommand.CommandText = "select marketdate from DateRange where trader=@trader and Datename=@Datename";
            mDateRangeComboBoxChangedCommand.Parameters.AddWithValue("@trader", "@trader");
            mDateRangeComboBoxChangedCommand.Parameters.AddWithValue("@Datename", "Datename");
            mDateRangeComboBoxChangedCommand.Connection = VayuConnection;
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DatelistBox.SelectedItems.Count != 0)
            {
                foreach (var items in DatelistBox.SelectedItems)
                {
                    if (SelectedlistBox.Items.Contains(items))
                    {
                        continue;
                    }
                    SelectedlistBox.Items.Add(items.ToString());
                }
                SelectedlistBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Dates", System.ComponentModel.ListSortDirection.Ascending));
            }
            else
            {
                MessageBox.Show("Please select date from the list:", "Select Date", MessageBoxButton.OK);
            }

        }
        private void Removebutton_Click(object sender, RoutedEventArgs e)
        {
            while (SelectedlistBox.SelectedItems.Count > 0)
            {
                SelectedlistBox.Items.Remove(SelectedlistBox.SelectedItems[0]);
            }
        }
        private void RemoveAllbutton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < SelectedlistBox.Items.Count; i++)
            {
                SelectedlistBox.Items.Clear();
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DateNameComboBox.Text.Trim().Length > 0)
            {
                VayuConnection.Open();
                mDeleteDateRangeCommand.Parameters["@trader"].Value = mUser;
                mDeleteDateRangeCommand.Parameters["@datename"].Value = DateNameComboBox.Text;
                mDeleteDateRangeCommand.ExecuteNonQuery();
                mInsertDateRangeCommand.Parameters["@trader"].Value = mUser;
                mInsertDateRangeCommand.Parameters["@DATENAME"].Value = DateNameComboBox.Text;
                foreach (string dateName in SelectedlistBox.Items)
                {
                    mInsertDateRangeCommand.Parameters["@marketdate"].Value = dateName;
                    mInsertDateRangeCommand.ExecuteNonQuery();
                }
                VayuConnection.Close();
                MessageBox.Show("Data saved.");
                Refreshbutton_Click(null, null);
            }
        }
        private void InitGui()
        {
            startDatePicker.SelectedDate = DateTime.Today;
        }
        private void AddDateButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime Datevalue = (DateTime)startDatePicker.SelectedDate;
            string Datevalue1 = Datevalue.ToString("MM/dd/yyyy");
            if (!DatelistBox.Items.Contains(Datevalue1))
            {
                DatelistBox.Items.Add(Datevalue1);
            }
        }
        private void Clearbutton_Click(object sender, RoutedEventArgs e)
        {
            DatelistBox.Items.Clear();
        }
        private void Refreshbutton_Click(object sender, RoutedEventArgs e)
        {
            DateNameComboBox.Items.Clear();
            VayuConnection.Open();
            mDateRangeComboBoxCommand.Parameters["@trader"].Value = mUser;
            SqlDataReader reader = mDateRangeComboBoxCommand.ExecuteReader();
            while (reader.Read())
            {
                DateNameComboBox.Items.Add(reader.GetString(0));
            }
            VayuConnection.Close();
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Delete?", "Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                VayuConnection.Open();
                mDeleteDateRangeCommand.Parameters["@trader"].Value = mUser;
                mDeleteDateRangeCommand.Parameters["@datename"].Value = DateNameComboBox.Text;
                mDeleteDateRangeCommand.ExecuteNonQuery();
                VayuConnection.Close();
                Refreshbutton_Click(null, null);
            }
        }
        private void DateNameComboBox_DropDownClosed(object sender, EventArgs e)
        {
            SelectedlistBox.Items.Clear();
            VayuConnection.Open();
            mDateRangeComboBoxChangedCommand.Parameters["@trader"].Value = mUser;
            mDateRangeComboBoxChangedCommand.Parameters["@Datename"].Value = DateNameComboBox.Text;
            SqlDataReader reader = mDateRangeComboBoxChangedCommand.ExecuteReader();
            while (reader.Read())
            {
                SelectedlistBox.Items.Add(reader.GetDateTime(0).ToString("MM/dd/yyyy"));
            }
            reader.Close();
            VayuConnection.Close();
        }
    }
}
