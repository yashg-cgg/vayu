using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.ErcotConstraintsDownload
{
    
    public class ApplicationLog
    {
        private SqlConnection mConnection90;
        private SqlCommand mDeleteApplicationLogCommand;
        private SqlCommand mInsertApplicationLogCommand;
        private SqlCommand mUpdateErrorApplicationLogCommand;
        private SqlCommand mUpdateInfoApplicationLogCommand;
        private string mApplicationName;
        private string mPassed;

        public ApplicationLog(string applicationName, string passed)
        {
            mApplicationName = applicationName;
            mPassed = passed;
            InitDB();
            SetApplicationLog();
        }
        private void InitDB()
        {
            mConnection90 = new VayuDBConnection().GetInstance().GetSqlConnection();
            //Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection()
            mDeleteApplicationLogCommand = new SqlCommand();
            mDeleteApplicationLogCommand.CommandText = "delete application_log where application = @application and passed = @passed";
            mDeleteApplicationLogCommand.Parameters.AddWithValue("@application", "application");
            mDeleteApplicationLogCommand.Parameters.AddWithValue("@passed", "passed");
            mDeleteApplicationLogCommand.Connection = mConnection90;
            //
            mInsertApplicationLogCommand = new SqlCommand();
            mInsertApplicationLogCommand.CommandText = "insert application_log values (@application, @start_time, null, null, null, null, null, null, @passed, @computer)";
            mInsertApplicationLogCommand.Parameters.AddWithValue("@application", "application");
            mInsertApplicationLogCommand.Parameters.AddWithValue("@start_time", "start_time");
            mInsertApplicationLogCommand.Parameters.AddWithValue("@passed", "passed");
            mInsertApplicationLogCommand.Parameters.AddWithValue("@computer", "computer");
            mInsertApplicationLogCommand.Connection = mConnection90;
            //
            mUpdateErrorApplicationLogCommand = new SqlCommand();
            mUpdateErrorApplicationLogCommand.CommandText = "update application_log set error = @error, error_time = @error_time, error_method = @error_method where application = @application and passed = @passed";
            mUpdateErrorApplicationLogCommand.Parameters.AddWithValue("@application", "application");
            mUpdateErrorApplicationLogCommand.Parameters.AddWithValue("@error_time", "error_time");
            mUpdateErrorApplicationLogCommand.Parameters.AddWithValue("@error_method", "error_method");
            mUpdateErrorApplicationLogCommand.Parameters.AddWithValue("@error", "error");
            mUpdateErrorApplicationLogCommand.Parameters.AddWithValue("@passed", "passed");
            mUpdateErrorApplicationLogCommand.Connection = mConnection90;
            //
            mUpdateInfoApplicationLogCommand = new SqlCommand();
            mUpdateInfoApplicationLogCommand.CommandText = "update application_log set information = @information, info_time = @info_time, info_method = @info_method where application = @application and passed = @passed";
            mUpdateInfoApplicationLogCommand.Parameters.AddWithValue("@application", "application");
            mUpdateInfoApplicationLogCommand.Parameters.AddWithValue("@info_time", "info_time");
            mUpdateInfoApplicationLogCommand.Parameters.AddWithValue("@information", "information");
            mUpdateInfoApplicationLogCommand.Parameters.AddWithValue("@info_method", "info_method");
            mUpdateInfoApplicationLogCommand.Parameters.AddWithValue("@passed", "passed");
            mUpdateInfoApplicationLogCommand.Connection = mConnection90;
        }
        private void SetApplicationLog()
        {
            mConnection90.Open();
            string passed = mPassed == null ? "" : mPassed;
            mDeleteApplicationLogCommand.Parameters["@application"].Value = mApplicationName;
            mDeleteApplicationLogCommand.Parameters["@passed"].Value = mPassed;
            mDeleteApplicationLogCommand.ExecuteNonQuery();
            mInsertApplicationLogCommand.Parameters["@application"].Value = mApplicationName;
            mInsertApplicationLogCommand.Parameters["@start_time"].Value = DateTime.Now;
            mInsertApplicationLogCommand.Parameters["@passed"].Value = mPassed;
            mInsertApplicationLogCommand.Parameters["@computer"].Value = Environment.MachineName;
            mInsertApplicationLogCommand.ExecuteNonQuery();
            mConnection90.Close();
        }
        public void UpdateError(string method, string error)
        {
            mConnection90.Open();
            error = error == null ? "" : error;
            mUpdateErrorApplicationLogCommand.Parameters["@application"].Value = mApplicationName;
            mUpdateErrorApplicationLogCommand.Parameters["@error_time"].Value = DateTime.Now;
            mUpdateErrorApplicationLogCommand.Parameters["@error_method"].Value = method;
            mUpdateErrorApplicationLogCommand.Parameters["@error"].Value = error;
            mUpdateErrorApplicationLogCommand.Parameters["@passed"].Value = mPassed;
            mUpdateErrorApplicationLogCommand.ExecuteNonQuery();
            mConnection90.Close();
        }
        public void UpdateInfo(string method, string info)
        {
            mConnection90.Open();
            info = info == null ? "" : info;
            mUpdateInfoApplicationLogCommand.Parameters["@application"].Value = mApplicationName;
            mUpdateInfoApplicationLogCommand.Parameters["@info_time"].Value = DateTime.Now;
            mUpdateInfoApplicationLogCommand.Parameters["@info_method"].Value = method;
            mUpdateInfoApplicationLogCommand.Parameters["@information"].Value = info;
            mUpdateInfoApplicationLogCommand.Parameters["@passed"].Value = mPassed;
            mUpdateInfoApplicationLogCommand.ExecuteNonQuery();
            mConnection90.Close();
        }
    }
}
