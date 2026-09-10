using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VayuApplication.Model
{
    public class DataService : IDataService
    { 
        SqlConnection VayuConnection;
        SqlCommand mInsertUserWindowPreferenceCmd; 
        private SqlCommand mSelectUserWindowPreference;
        private SqlCommand mSelectEndUserCommand;
        private SqlCommand mSelectPrefernceCommand;
        private SqlCommand mDeletePreferenceCommand;
        private SqlCommand mDeletePreferenceMasterCommand;
        private SqlCommand mSelectPreferenceNames;
        private SqlCommand mSaveUserWindowPreferenceMasterCommand;
        private SqlCommand mInsertScreenCommand;
        private void LoadDB()
        {
            mInsertUserWindowPreferenceCmd = VayuConnection.CreateCommand();
            
            mInsertUserWindowPreferenceCmd.CommandText = "INSERT INTO UserWindowPreferenceDetails(PreferenceId, FormName, XCoordinate, YCoordinate, Height, Width, SavedTime, Comments)"
                                                        + "VALUES (@PreferenceId, @FormName, @X, @Y, @Height, @Width, GETDATE(), @Comments)";
            mInsertUserWindowPreferenceCmd.Parameters.AddWithValue("@PreferenceId", "PreferenceId");
            mInsertUserWindowPreferenceCmd.Parameters.AddWithValue("@FormName", "FormName");
            mInsertUserWindowPreferenceCmd.Parameters.AddWithValue("@X", "XCoordinate");
            mInsertUserWindowPreferenceCmd.Parameters.AddWithValue("@Y", "YCoordinate");
            mInsertUserWindowPreferenceCmd.Parameters.AddWithValue("@Height", "Height");
            mInsertUserWindowPreferenceCmd.Parameters.AddWithValue("@Width", "Width");
            mInsertUserWindowPreferenceCmd.Parameters.AddWithValue("@Comments", "Comments");

            mSelectUserWindowPreference = VayuConnection.CreateCommand();
            mSelectUserWindowPreference.CommandText = "SELECT a.FormName, a.Height, a.Width, a.XCoordinate, a.YCoordinate, a.Comments FROM UserWindowPreferenceDetails a " +
                                                        "WHERE PreferenceId IN (SELECT DISTINCT PreferenceId FROM UserWindowPreferenceMaster " +
                                                        "WHERE PreferenceName LIKE @PrefName AND UserName LIKE @User) ";
           
            mSelectUserWindowPreference.Parameters.AddWithValue("@PrefName", "PreferenceName");
            mSelectUserWindowPreference.Parameters.AddWithValue("@User", "UserName");

            mSelectEndUserCommand = new SqlCommand();
            mSelectEndUserCommand.CommandText = "SELECT LocalLogin FROM EndUser WHERE ActiveYN='Y' order by UserAlias";
            mSelectEndUserCommand.Connection = VayuConnection;

            mSelectPreferenceNames = VayuConnection.CreateCommand();
            mSelectPreferenceNames.CommandText = "SELECT DISTINCT preferencename FROM UserWindowPreferenceMaster WHERE UserName LIKE @user";
            mSelectPreferenceNames.Parameters.AddWithValue("@user", "");

            mSelectPrefernceCommand = VayuConnection.CreateCommand();
            mSelectPrefernceCommand.CommandText = "select Count(PreferenceId) from [NewTrading].[dbo].[UserWindowPreferenceMaster] where UserName=@user";
            mSelectPrefernceCommand.Parameters.AddWithValue("@user", "");

            mDeletePreferenceCommand = VayuConnection.CreateCommand();
            mDeletePreferenceCommand.CommandText = "DELETE FROM UserWindowPreferenceDetails WHERE PreferenceId IN (SELECT DISTINCT PreferenceId FROM UserWindowPreferenceMaster " +
                                                    "WHERE PreferenceName LIKE @prefName AND UserName LIKE @user)";
            mDeletePreferenceCommand.Parameters.AddWithValue("@prefName", "");
            mDeletePreferenceCommand.Parameters.AddWithValue("@user", "");

            mDeletePreferenceMasterCommand = VayuConnection.CreateCommand();
            mDeletePreferenceMasterCommand.CommandText = "DELETE FROM UserWindowPreferenceMaster WHERE preferencename LIKE @prefName AND username LIKE @user";
            mDeletePreferenceMasterCommand.Parameters.AddWithValue("@prefName", "");
            mDeletePreferenceMasterCommand.Parameters.AddWithValue("@user", "");

            mSaveUserWindowPreferenceMasterCommand = VayuConnection.CreateCommand();
            mSaveUserWindowPreferenceMasterCommand.CommandText = "SaveUserWindowPreferenceMaster";
            mSaveUserWindowPreferenceMasterCommand.CommandType = CommandType.StoredProcedure;
            mSaveUserWindowPreferenceMasterCommand.Parameters.AddWithValue("@UserName", "");
            mSaveUserWindowPreferenceMasterCommand.Parameters.AddWithValue("@PreferenceName", "");

            mInsertScreenCommand = VayuConnection.CreateCommand();
            mInsertScreenCommand.CommandText = "Insert into [NewTrading].[dbo].[UserWindowPreferenceMaster] ([UserName],[PreferenceName],[SavedTime]) Values(@UserName,@PreferenceName,@SavedTime)";
            mInsertScreenCommand.Parameters.AddWithValue("@UserName", "UserName");
            mInsertScreenCommand.Parameters.AddWithValue("@PreferenceName", "PreferenceName");
            mInsertScreenCommand.Parameters.AddWithValue("@SavedTime", "SavedTime");
        }

        public void GetUserLoginDetails(Action<List<string>, Exception> callback)
        {
            LoadDB();
            List<string> userLoginDetails = new List<string>();
            try
            {
                if (mSelectEndUserCommand.Connection.State.Equals(System.Data.ConnectionState.Closed))
                    mSelectEndUserCommand.Connection.Open();
                var reader = mSelectEndUserCommand.ExecuteReader();
                while (reader.Read())
                {
                    userLoginDetails.Add(reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString());
                }
                if (!reader.IsClosed)
                    reader.Close();
                callback(userLoginDetails, null);
            }
            catch
            {
                callback(null, new ArgumentException("something went wrong"));
            }
            finally
            {
                if (VayuConnection.State == ConnectionState.Closed)
                { 
                    VayuConnection.Close();
                }
                   
            }

        }

        public int GetPrefernceId(string user)
        {
            int count = 0;
            if (VayuConnection == null)
            {
                LoadDB();
            }
            try
            {
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Close();
                }
                mSelectPrefernceCommand.Parameters[0].Value = user;
                var reader = mSelectPrefernceCommand.ExecuteReader();
                while (reader.Read())
                {
                    count = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                }
                if (!reader.IsClosed)
                    reader.Close();
                VayuConnection.Close();
            }
            catch
            {
            }
            return count;
        }

        public bool UpdatePreference(List<DataItem> userWindoList, string preferenceName, string user)
        {
            if (VayuConnection == null)
            {
                LoadDB();
            }
            try
            {
                if (VayuConnection.State.Equals(ConnectionState.Closed))
                {
                    VayuConnection.Open();
                }
                mDeletePreferenceCommand.Parameters[0].Value = preferenceName;
                mDeletePreferenceCommand.Parameters[1].Value = user;
                // mDeletePreferenceCommand.ExecuteNonQuery();

                mDeletePreferenceMasterCommand.Parameters[0].Value = preferenceName;
                mDeletePreferenceMasterCommand.Parameters[1].Value = user;
                // mDeletePreferenceMasterCommand.ExecuteNonQuery();
            }
            catch
            {
            }
            finally
            {
                if (VayuConnection.State.Equals(ConnectionState.Open))
                {
                    VayuConnection.Close();
                }
            }
            bool isSuccess = SaveUserWindowPreference(userWindoList, preferenceName, user);

            return isSuccess;
        }

        public bool SaveUserWindowPreference(List<DataItem> userWindowList, string preferenceName, string username)
        {
            try
            {
                bool isSuccess = false;

                LoadDB();

                if (VayuConnection.State == ConnectionState.Open)
                {
                    VayuConnection.Close();
                }
                VayuConnection.Open();

                int prefid = -1;
                mSaveUserWindowPreferenceMasterCommand.Parameters[0].Value = username;
                mSaveUserWindowPreferenceMasterCommand.Parameters[1].Value = preferenceName;
                prefid = Convert.ToInt32(mSaveUserWindowPreferenceMasterCommand.ExecuteScalar());

                if (prefid > 0)
                {
                    foreach (DataItem item in userWindowList)
                    {
                        //mInsertUserWindowPreferenceCmd.Parameters[0].Value = item.FormName.ToUpperInvariant();
                        //mInsertUserWindowPreferenceCmd.Parameters[1].Value = prefid;
                        //mInsertUserWindowPreferenceCmd.Parameters[2].Value = double.IsNaN(item.XCoordinate) ? 0.0 : item.XCoordinate;
                        //mInsertUserWindowPreferenceCmd.Parameters[3].Value = double.IsNaN(item.YCoordinate) ? 0.0 : item.YCoordinate;
                        //mInsertUserWindowPreferenceCmd.Parameters[4].Value = double.IsNaN(item.Height) ? 0.0 : item.Height;
                        //mInsertUserWindowPreferenceCmd.Parameters[5].Value = double.IsNaN(item.Width) ? 0.0 : item.Width;
                        //mInsertUserWindowPreferenceCmd.Parameters[6].Value = item.Comments == null ? "" : item.Comments;

                        mInsertUserWindowPreferenceCmd.Parameters["@PreferenceId"].Value = prefid;
                        mInsertUserWindowPreferenceCmd.Parameters["@FormName"].Value = item.FormName.ToUpperInvariant();
                        mInsertUserWindowPreferenceCmd.Parameters["@X"].Value = double.IsNaN(item.XCoordinate) ? 0.0 : item.XCoordinate;
                        mInsertUserWindowPreferenceCmd.Parameters["@Y"].Value = double.IsNaN(item.YCoordinate) ? 0.0 : item.YCoordinate;
                        mInsertUserWindowPreferenceCmd.Parameters["@Height"].Value = double.IsNaN(item.Height) ? 0.0 : item.Height;
                        mInsertUserWindowPreferenceCmd.Parameters["@Width"].Value = double.IsNaN(item.Width) ? 0.0 : item.Width;
                        mInsertUserWindowPreferenceCmd.Parameters["@Comments"].Value = item.Comments == null ? "" : item.Comments;
                        int ret = mInsertUserWindowPreferenceCmd.ExecuteNonQuery();
                        if (ret > 0)
                        {
                            isSuccess = true;
                        }
                    }
                }

                VayuConnection.Close();

                return isSuccess;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<string> GetPreferenceNames(string mUser)
        {
            List<string> prefNames = new List<string>();
            if (VayuConnection == null)
            {
                LoadDB();
            }
            mSelectPreferenceNames.Parameters[0].Value = mUser;
            try
            {
                if (mSelectPreferenceNames.Connection.State.Equals(ConnectionState.Closed))
                {
                    mSelectPreferenceNames.Connection.Open();
                }
                IDataReader reader = mSelectPreferenceNames.ExecuteReader();
                while (reader.Read())
                {
                    prefNames.Add(reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString().ToUpper());
                }
                if (!reader.IsClosed)
                {
                    reader.Close();
                }
            }
            catch
            {
            }
            finally
            {
                if (mSelectPreferenceNames.Connection.State.Equals(ConnectionState.Open))
                {
                    mSelectPreferenceNames.Connection.Close();
                }
            }
            return prefNames;
        }

        public List<DataItem> GetPreferenceDetails(string preferenceName, string mUser)
        {
            if (VayuConnection == null)
            {
                LoadDB();
            }
            List<DataItem> prefList = new List<DataItem>();
            try
            {
                mSelectUserWindowPreference.Parameters["@PrefName"].Value = preferenceName;
                mSelectUserWindowPreference.Parameters["@User"].Value = mUser;
                if (VayuConnection.State.Equals(ConnectionState.Closed))
                {
                    VayuConnection.Open();
                }
                IDataReader reader = mSelectUserWindowPreference.ExecuteReader();
                while (reader.Read())
                {
                    prefList.Add(new DataItem
                    {
                        FormName = reader.GetValue(0).ToString(),
                        Height = reader.IsDBNull(1) ? 500 : Convert.ToDouble(reader.GetValue(1)),
                        Width = reader.IsDBNull(2) ? 500 : Convert.ToDouble(reader.GetValue(2)),
                        XCoordinate = reader.IsDBNull(3) ? 500 : Convert.ToDouble(reader.GetValue(3)),
                        YCoordinate = reader.IsDBNull(4) ? 500 : Convert.ToDouble(reader.GetValue(4)),
                        Comments = reader.IsDBNull(5) ? "" : reader.GetValue(5).ToString()
                    });
                }
                if (!reader.IsClosed)
                {
                    reader.Close();
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (VayuConnection.State.Equals(ConnectionState.Open))
                {
                    VayuConnection.Close();
                }
            }
            return prefList;
        }

        public bool InsertScreen(string PreferenceName, string UserName)
        {
            bool status = false;
            try
            {
                if (VayuConnection.State == ConnectionState.Open)
                {
                    VayuConnection.Close();
                }
                VayuConnection.Open();
                mInsertScreenCommand.Parameters["@UserName"].Value = UserName;
                mInsertScreenCommand.Parameters["@PreferenceName"].Value = PreferenceName;
                mInsertScreenCommand.Parameters["@SavedTime"].Value = DateTime.Today;
                int count = mInsertScreenCommand.ExecuteNonQuery();
                if (count > 0)
                    status = true;

            }
            catch
            {

            }
            return status;
        }
    }
}
