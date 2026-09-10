using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;
using Vayu.DBLibrary;

namespace Vayu.ConstraintAnalyzer.Model
{
    public class MySqlCommand
    {
        /// <summary>
        /// Gets or sets the connection.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public SqlConnection Connection { get; set; }

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public SqlCommand Command { get; set; }

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        /// <value>
        /// The command text.
        /// </value>
        public string CommandText
        {
            get { return Command.CommandText; }
            set { Command.CommandText = value; }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public SqlParameterCollection Parameters
        {
            get { return Command.Parameters; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlCommand"/> class.
        /// </summary>
        public MySqlCommand()
        {
            Command = new SqlCommand();
            Command.Connection = new VayuDBConnection().GetInstance().GetSqlConnection();
        }

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <returns></returns>
        public MySqlDataReader ExecuteReader()
        {
            if (Command.Connection.State == ConnectionState.Closed)
            {
                Command.Connection.Open();
            }

            SqlDataReader reader = Command.ExecuteReader();
            MySqlDataReader myReader = new MySqlDataReader(reader, this);
            return myReader;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MySqlDataReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDataReader"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="mycmd">The mycmd.</param>
        public MySqlDataReader(SqlDataReader reader, MySqlCommand mycmd)
        {
            Command = mycmd;
            DataReader = reader;
        }
        /// <summary>
        /// Gets or sets the data reader.
        /// </summary>
        /// <value>
        /// The data reader.
        /// </value>
        public SqlDataReader DataReader { get; set; }
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public MySqlCommand Command { get; set; }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified i.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public object this[int i] { get { return DataReader[i]; } }

        /// <summary>
        /// Reads this instance.
        /// </summary>
        /// <returns></returns>
        public bool Read()
        {
            return DataReader.Read();
        }

        /// <summary>
        /// Determines whether [is database null] [the specified i].
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns>
        ///   <c>true</c> if [is database null] [the specified i]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDBNull(int i) { return DataReader.IsDBNull(i); }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public string GetString(int i) { return DataReader.GetString(i); }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close() { DataReader.Close(); Command.Command.Connection.Close(); }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public object GetValue(int i) { return DataReader.GetValue(i); }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.ConstraintContingencyAnalysis.Model.IDataService" />
    public class DataService : IDataService
    {
        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private SqlConnection VayuConnection;

        #region SQL Commands

        /// <summary>
        /// The m get dates
        /// </summary>
        private MySqlCommand mGetDates;
        private MySqlCommand mGetErcotDates;
        /// <summary>
        /// The m get hourly rt impact
        /// </summary>
        private MySqlCommand mGetHourlyRTImpact;
        private MySqlCommand mGetErcotHourlyRTImpact;
        /// <summary>
        /// The m get this hour rt impact
        /// </summary>
        private MySqlCommand mGetThisHourRTImpact;
        private MySqlCommand mGetErcotThisHourRTImpact;
        /// <summary>
        /// The m get maximum impact hour today
        /// </summary>
        private MySqlCommand mGetMaxImpactHourToday;
        private MySqlCommand mGetErcotMaxImpactHourToday;
        /// <summary>
        /// The m get rt shadows
        /// </summary>
        private MySqlCommand mGetRTShadows;
        private MySqlCommand mGetErcotRTShadows;
        /// <summary>
        /// The m get this hour rt shadows
        /// </summary>
        private MySqlCommand mGetThisHourRTShadows;
        private MySqlCommand mGetThisErcotHourRTShadows;
        /// <summary>
        /// The m get rt shifted
        /// </summary>
        private MySqlCommand mGetRTShifted;
        private MySqlCommand mGetErcotRTShifted;
        /// <summary>
        /// The m get this hour rt shifted
        /// </summary>
        private MySqlCommand mGetThisHourRTShifted;
        private MySqlCommand mGetThisErcotHourRTShifted;
        /// <summary>
        /// The m get da shadows
        /// </summary>
        private MySqlCommand mGetDAShadows;
        private MySqlCommand mGetErcotDAShadows;
        /// <summary>
        /// The m get no Sensitivity constraints
        /// </summary>
        private MySqlCommand mGetNoSensitivityConstraints;
        /// <summary>
        /// The m select best minimum nodes command
        /// </summary>
        private MySqlCommand mSelectBestMinNodesCommand;
        /// <summary>
        /// The m select best maximum nodes command
        /// </summary>
        private MySqlCommand mSelectBestMaxNodesCommand;
        /// <summary>
        /// The m select best up maximum command
        /// </summary>
        private MySqlCommand mSelectBestUpMaxCommand;
        /// <summary>
        /// The m select best up minimum command
        /// </summary>
        private MySqlCommand mSelectBestUpMinCommand;
        /// <summary>
        /// The m select node details command
        /// </summary>
        private MySqlCommand mSelectNodeDetailsCommand;
        /// <summary>
        /// The m get family dates
        /// </summary>
        private MySqlCommand mGetFamilyDates;
        /// <summary>
        /// The m get hourly rt impact family
        /// </summary>
        private MySqlCommand mGetHourlyRTImpactFamily;
        /// <summary>
        /// The m get maximum impact hour today family
        /// </summary>
        private MySqlCommand mGetMaxImpactHourTodayFamily;
        /// <summary>
        /// The m get this hour rt impact family
        /// </summary>
        private MySqlCommand mGetThisHourRTImpactFamily;
        /// <summary>
        /// The m get rt shadows family
        /// </summary>
        private MySqlCommand mGetRTShadowsFamily;
        /// <summary>
        /// The m get this hour rt shadows family
        /// </summary>
        private MySqlCommand mGetThisHourRTShadowsFamily;
        /// <summary>
        /// The m get rt shifted family
        /// </summary>
        private MySqlCommand mGetRTShiftedFamily;
        /// <summary>
        /// The m get this hour rt shifted family
        /// </summary>
        private MySqlCommand mGetThisHourRTShiftedFamily;
        /// <summary>
        /// The m get tagged outage families
        /// </summary>
        private MySqlCommand mGetTaggedOutageFamilies;
        /// <summary>
        /// The m get tagged outage constraints
        /// </summary>
        private MySqlCommand mGetTaggedOutageConstraints;
        /// <summary>
        /// The m get distinct equipment
        /// </summary>
        private MySqlCommand mGetDistinctEquipment;
        private MySqlCommand mGetErcotDistinctEquipment;
        /// <summary>
        /// The m get distinct equipment forced
        /// </summary>
        private MySqlCommand mGetDistinctEquipmentForced;
        /// <summary>
        /// The m select archived current outages
        /// </summary>
        private MySqlCommand mSelectArchivedCurrentOutages;
        private MySqlCommand mSelectErcotArchivedCurrentOutages;
        /// <summary>
        /// The m select archived sched outages
        /// </summary>
        private MySqlCommand mSelectArchivedSchedOutages;
        /// <summary>
        /// The m select tagged outages
        /// </summary>
        private MySqlCommand mSelectTaggedOutages;
        /// <summary>
        /// The m get tagged con nums
        /// </summary>
        private MySqlCommand mGetTaggedConNums;
        /// <summary>
        /// The m get tagged con nums by fam
        /// </summary>
        private MySqlCommand mGetTaggedConNumsByFam;
        /// <summary>
        /// The m get maximum node
        /// </summary>
        private MySqlCommand mGetMaxNode;
        private MySqlCommand mGetErcotMaxNode;
        /// <summary>
        /// The m get nodal zone
        /// </summary>
        private MySqlCommand mGetNodalZone;
        /// <summary>
        /// The m get minimum node
        /// </summary>
        private MySqlCommand mGetMinNode;
        private MySqlCommand mGetErcotMinNode;
        /// <summary>
        /// The m select archived current outages active
        /// </summary>
        private MySqlCommand mSelectArchivedCurrentOutagesActive;

        #endregion

        #region Private Variables

        /// <summary>
        /// The m best node key
        /// </summary>
        private int mBestNodeKey;
        /// <summary>
        /// The m maximum node
        /// </summary>
        private int mMaxNode;
        /// <summary>
        /// The m minimum node
        /// </summary>
        private int mMinNode;
        /// <summary>
        /// The m to zone
        /// </summary>
        private string mToZone;
        /// <summary>
        /// The m from zone
        /// </summary>
        private string mFromZone;
        /// <summary>
        /// The m last day
        /// </summary>
        private DateTime mLastDay;

        #endregion

        #region DataReaders

        /// <summary>
        /// The datereader
        /// </summary>
        private MySqlDataReader Datereader;
        /// <summary>
        /// The reader
        /// </summary>
        private MySqlDataReader reader;
        /// <summary>
        /// The ireader
        /// </summary>
        private MySqlDataReader Ireader;
        /// <summary>
        /// The jreader
        /// </summary>
        private MySqlDataReader Jreader;
        /// <summary>
        /// The freader
        /// </summary>
        private MySqlDataReader Freader;

        #endregion

        /// <summary>
        /// Gets Constraints List.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="RtRadioButton">if set to <c>true</c> [rt RadioButton].</param>
        /// <param name="DaRadioButton">if set to <c>true</c> [da RadioButton].</param>
        /// <param name="ImpactRadioButton">if set to <c>true</c> [impact RadioButton].</param>
        /// <param name="ShiftedRadioButton">if set to <c>true</c> [shifted RadioButton].</param>
        /// <param name="ShadowRadioButton">if set to <c>true</c> [shadow RadioButton].</param>
        /// <param name="DateRangeCheckBox">if set to <c>true</c> [date range CheckBox].</param>
        /// <param name="DatePicker1">The date picker1.</param>
        /// <param name="DatePicker2">The date picker2.</param>
        /// <param name="SelectedFamily">The selected family.</param>
        /// <param name="FamilyCheckBox">if set to <c>true</c> [family CheckBox].</param>
        /// <param name="ConstraintsList">The constraints list.</param>
        /// <returns></returns>
        private List<Model.Constraints> GetConstraints1(Action<List<Model.Constraints>, Exception> callback, bool RtRadioButton, bool DaRadioButton, bool ImpactRadioButton, bool ShiftedRadioButton, bool ShadowRadioButton, bool DateRangeCheckBox, DateTime DatePicker1, DateTime DatePicker2, string SelectedFamily, bool FamilyCheckBox, List<Model.Constraints> ConstraintsList, int Marketkey)
        {
            //RT Shadow on Lookback
            if (RtRadioButton == true && ShadowRadioButton == true && DateRangeCheckBox == true)
            {
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }

                if (FamilyCheckBox == true)
                {
                    mGetFamilyDates.Parameters["@familyName"].Value = SelectedFamily.ToString();
                    mGetFamilyDates.Parameters["@startDate"].Value = DatePicker1.Date;
                    mGetFamilyDates.Parameters["@endDate"].Value = DatePicker2.Date;
                    Datereader = mGetFamilyDates.ExecuteReader();
                }
                else
                {
                    //if (Marketkey == 1)
                    //{
                    //    mGetDates.Parameters["@startDate"].Value = DatePicker1.Date;
                    //    mGetDates.Parameters["@endDate"].Value = DatePicker2.Date;
                    //    Datereader = mGetDates.ExecuteReader();
                    //}
                    //else
                    {
                        mGetErcotDates.Parameters["@startDate"].Value = DatePicker1.Date;
                        mGetErcotDates.Parameters["@endDate"].Value = DatePicker2.Date;
                        Datereader = mGetErcotDates.ExecuteReader();
                    }
                }
                Dictionary<string, Model.Constraints> constraintHash = new Dictionary<string, Model.Constraints>();
                Dictionary<int, string> constraintMaxZone = new Dictionary<int, string>();
                Dictionary<int, string> constraintMinZone = new Dictionary<int, string>();
                while (Datereader.Read())
                {
                    string date = Convert.ToString(Datereader.GetValue(0));
                    DateTime dateRetrieve = Convert.ToDateTime(Datereader.GetValue(0));
                    string dayofweek = Convert.ToString(dateRetrieve.DayOfWeek);
                    if (FamilyCheckBox == true)
                    {
                        mGetRTShadowsFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                        mGetRTShadowsFamily.Parameters["@startDate"].Value = date;
                        reader = mGetRTShadowsFamily.ExecuteReader();
                    }
                    else
                    {
                        if (Marketkey == 1)
                        {
                            mGetRTShadows.Parameters["@startDate"].Value = date;
                            reader = mGetRTShadows.ExecuteReader();
                        }
                        else
                        {
                            mGetErcotRTShadows.Parameters["@startDate"].Value = date;
                            reader = mGetErcotRTShadows.ExecuteReader();
                        }
                    }

                    while (reader.Read())
                    {
                        int hour = Convert.ToInt16(reader.GetValue(5));
                        Model.Constraints constraint = new Model.Constraints();
                        int constraintNum = Convert.ToInt32(reader.GetValue(1));

                        if (constraintHash.ContainsKey(constraintNum + "@" + date))
                        {
                            constraint = constraintHash[constraintNum + "@" + date];
                        }
                        else
                        {
                            constraintHash.Add(constraintNum + "@" + date, constraint);

                            //get from/to zones
                            mToZone = "UNK";
                            mFromZone = "UNK";
                            string test = null;
                            constraintMaxZone.TryGetValue(constraintNum, out test);
                            if (test != null) { }
                            else
                            {
                                MySqlDataReader maxReader = null;
                                if (Marketkey == 1)
                                {
                                    mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                    maxReader = mGetMaxNode.ExecuteReader();
                                }
                                else
                                {
                                    mGetErcotMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                    maxReader = mGetErcotMaxNode.ExecuteReader();
                                }
                                while (maxReader.Read())
                                {
                                    if (maxReader.IsDBNull(0))
                                    {
                                        mMaxNode = 0;
                                    }
                                    else
                                    {
                                        mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                        mToZone = Convert.ToString(maxReader.GetValue(1));
                                        constraintMaxZone.Add(constraintNum, mToZone);
                                    }
                                }
                                maxReader.Close();
                                MySqlDataReader minReader = null;
                                if (Marketkey == 1)
                                {
                                    mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                    minReader = mGetMinNode.ExecuteReader();
                                }
                                else
                                {
                                    mGetErcotMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                    minReader = mGetErcotMinNode.ExecuteReader();
                                }
                                while (minReader.Read())
                                {
                                    if (minReader.IsDBNull(0))
                                    {
                                        mMinNode = 0;
                                    }
                                    else
                                    {
                                        mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                        mFromZone = Convert.ToString(minReader.GetValue(1));
                                        constraintMinZone.Add(constraintNum, mFromZone);
                                    }
                                }
                                minReader.Close();
                            }
                        }
                        constraint.date = date;
                        constraint.constraintNum = constraintNum;
                        string toZone = null; string fromZone = null;
                        constraintMaxZone.TryGetValue(constraintNum, out toZone);
                        constraintMinZone.TryGetValue(constraintNum, out fromZone);
                        constraint.fromZone = fromZone;
                        constraint.toZone = toZone;
                        constraint.monitoredName = reader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                        constraint.contingName = reader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                        if (hour == 1) { constraint.he1Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 2) { constraint.he2Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 3) { constraint.he3Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 4) { constraint.he4Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 5) { constraint.he5Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 6) { constraint.he6Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 7) { constraint.he7Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 8) { constraint.he8Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 9) { constraint.he9Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 10) { constraint.he10Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 11) { constraint.he11Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 12) { constraint.he12Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 13) { constraint.he13Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 14) { constraint.he14Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 15) { constraint.he15Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 16) { constraint.he16Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 17) { constraint.he17Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 18) { constraint.he18Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 19) { constraint.he19Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 20) { constraint.he20Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 21) { constraint.he21Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 22) { constraint.he22Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 23) { constraint.he23Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 24) { constraint.he24Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        constraint.dayofweek = dayofweek;
                    }
                    reader.Close();
                    DateTime currentTime = DateTime.Now;
                    //if today is the date user selected we need to deal with current hour impact estimate
                    if (Convert.ToString(currentTime.Date) == date)
                    {
                        dayofweek = Convert.ToString(currentTime.DayOfWeek);
                        int currentHour = Convert.ToInt32(currentTime.ToString("HH")) + 1;
                        //make it hour ending by adding 1
                        currentHour = currentHour + 1;
                        int maxImpactHour = 0;
                        int currentMinute = Convert.ToInt32(currentTime.ToString("mm"));
                        double currentElapsedInterval = 0;
                        if (FamilyCheckBox == true)
                        {
                            mGetMaxImpactHourTodayFamily.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                            mGetMaxImpactHourTodayFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                            Ireader = mGetMaxImpactHourTodayFamily.ExecuteReader();
                        }
                        else
                        {
                            if (Marketkey == 1)
                            {
                                mGetMaxImpactHourToday.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                                Ireader = mGetMaxImpactHourToday.ExecuteReader();
                            }
                            else
                            {
                                mGetErcotMaxImpactHourToday.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                                Ireader = mGetErcotMaxImpactHourToday.ExecuteReader();
                            }
                        }
                        while (Ireader.Read())
                        {
                            if (!Ireader.IsDBNull(0))
                            {
                                maxImpactHour = Convert.ToInt32(Ireader.GetValue(0));
                            }
                        }
                        Ireader.Close();
                        //when time == -1 we run the query once -- one intervals value
                        if (maxImpactHour - currentHour == -1) //we need to divide sum(SP)*SF by intervals elapsed
                        {
                            currentElapsedInterval = currentMinute / 5;
                            currentElapsedInterval = Math.Floor(currentElapsedInterval);
                            if (FamilyCheckBox == true)
                            {
                                mGetThisHourRTShadowsFamily.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisHourRTShadowsFamily.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetThisHourRTShadowsFamily.Parameters["@currentHour"].Value = currentHour;
                                mGetThisHourRTShadowsFamily.Parameters["@intervals"].Value = currentElapsedInterval;
                                mGetThisHourRTShadowsFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                                Jreader = mGetThisHourRTShadowsFamily.ExecuteReader();
                            }
                            else
                            {
                                if (Marketkey == 1)
                                {
                                    mGetThisHourRTShadows.Parameters["@realTimeDate"].Value = currentTime.Date;
                                    mGetThisHourRTShadows.Parameters["@startHour"].Value = maxImpactHour + 1;
                                    mGetThisHourRTShadows.Parameters["@currentHour"].Value = currentHour;
                                    mGetThisHourRTShadows.Parameters["@intervals"].Value = currentElapsedInterval;
                                    Jreader = mGetThisHourRTShadows.ExecuteReader();
                                }
                                else
                                {
                                    mGetThisErcotHourRTShadows.Parameters["@realTimeDate"].Value = currentTime.Date;
                                    mGetThisErcotHourRTShadows.Parameters["@startHour"].Value = maxImpactHour + 1;
                                    mGetThisErcotHourRTShadows.Parameters["@currentHour"].Value = currentHour;
                                    mGetThisErcotHourRTShadows.Parameters["@intervals"].Value = currentElapsedInterval;
                                    Jreader = mGetThisErcotHourRTShadows.ExecuteReader();
                                }
                            }
                            while (Jreader.Read())
                            {
                                int hour = Convert.ToInt16(Jreader.GetValue(5));
                                Model.Constraints constraint = new Model.Constraints();
                                int constraintNum = Convert.ToInt32(Jreader.GetValue(1));
                                if (constraintHash.ContainsKey(constraintNum + "@" + date))
                                {
                                    constraint = constraintHash[constraintNum + "@" + date];
                                }
                                else
                                {
                                    constraintHash.Add(constraintNum + "@" + date, constraint);

                                    //get from/to zones
                                    mToZone = "UNK";
                                    mFromZone = "UNK";
                                    string test = null;
                                    constraintMaxZone.TryGetValue(constraintNum, out test);
                                    if (test != null) { }
                                    else
                                    {
                                        MySqlDataReader maxReader = null;
                                        if (Marketkey == 1)
                                        {
                                            mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                            maxReader = mGetMaxNode.ExecuteReader();
                                        }
                                        else
                                        {
                                            mGetErcotMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                            maxReader = mGetErcotMaxNode.ExecuteReader();
                                        }
                                        while (maxReader.Read())
                                        {
                                            if (maxReader.IsDBNull(0))
                                            {
                                                mMaxNode = 0;
                                            }
                                            else
                                            {
                                                mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                                mToZone = Convert.ToString(maxReader.GetValue(1));
                                                constraintMaxZone.Add(constraintNum, mToZone);
                                            }
                                        }
                                        maxReader.Close();
                                        MySqlDataReader minReader = null;
                                        if (Marketkey == 1)
                                        {
                                            mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                            minReader = mGetMinNode.ExecuteReader();
                                        }
                                        else
                                        {
                                            mGetErcotMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                            minReader = mGetErcotMinNode.ExecuteReader();
                                        }

                                        while (minReader.Read())
                                        {
                                            if (minReader.IsDBNull(0))
                                            {
                                                mMinNode = 0;
                                            }
                                            else
                                            {
                                                mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                                mFromZone = Convert.ToString(minReader.GetValue(1));
                                                constraintMinZone.Add(constraintNum, mFromZone);
                                            }
                                        }
                                        minReader.Close();
                                    }
                                }
                                constraint.date = date;
                                constraint.constraintNum = constraintNum;
                                string toZone = null; string fromZone = null;
                                constraintMaxZone.TryGetValue(constraintNum, out toZone);
                                constraintMinZone.TryGetValue(constraintNum, out fromZone);
                                constraint.fromZone = fromZone;
                                constraint.toZone = toZone;
                                constraint.monitoredName = Jreader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                                constraint.contingName = Jreader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                                if (hour == 1) { constraint.he1Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 2) { constraint.he2Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 3) { constraint.he3Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 4) { constraint.he4Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 5) { constraint.he5Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 6) { constraint.he6Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 7) { constraint.he7Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 8) { constraint.he8Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 9) { constraint.he9Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 10) { constraint.he10Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 11) { constraint.he11Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 12) { constraint.he12Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 13) { constraint.he13Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 14) { constraint.he14Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 15) { constraint.he15Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 16) { constraint.he16Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 17) { constraint.he17Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 18) { constraint.he18Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 19) { constraint.he19Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 20) { constraint.he20Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 21) { constraint.he21Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 22) { constraint.he22Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 23) { constraint.he23Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 24) { constraint.he24Value = Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                constraint.dayofweek = dayofweek;
                            }
                            Jreader.Close();
                        }
                        if (maxImpactHour - currentHour < -1) // we need to divide sum(SP*SF) by 12 and just assume we scraped all intervals
                        {
                            //select shift factor, constraintrtnum, monitoredtext, contingencytext, sum(SP*SF)/12, datepart(HH, marketdatetime)+1 
                            //where hours between maxImpactHour+1 and currentHour
                            //where date is today
                            if (FamilyCheckBox == true)
                            {
                                mGetThisHourRTShadowsFamily.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisHourRTShadowsFamily.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetThisHourRTShadowsFamily.Parameters["@currentHour"].Value = currentHour;
                                mGetThisHourRTShadowsFamily.Parameters["@intervals"].Value = 12;
                                mGetThisHourRTShadowsFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                                Freader = mGetThisHourRTShadowsFamily.ExecuteReader();
                            }
                            else
                            {
                                if (Marketkey == 1)
                                {
                                    mGetThisHourRTShadows.Parameters["@realTimeDate"].Value = currentTime.Date;
                                    mGetThisHourRTShadows.Parameters["@startHour"].Value = maxImpactHour + 1;
                                    mGetThisHourRTShadows.Parameters["@currentHour"].Value = currentHour;
                                    mGetThisHourRTShadows.Parameters["@intervals"].Value = 12;
                                    Freader = mGetThisHourRTShadows.ExecuteReader();
                                }
                                else
                                {

                                    mGetThisErcotHourRTShadows.Parameters["@realTimeDate"].Value = currentTime.Date;
                                    mGetThisErcotHourRTShadows.Parameters["@startHour"].Value = maxImpactHour + 1;
                                    mGetThisErcotHourRTShadows.Parameters["@currentHour"].Value = currentHour;
                                    mGetThisErcotHourRTShadows.Parameters["@intervals"].Value = 12;
                                    Freader = mGetThisErcotHourRTShadows.ExecuteReader();
                                }
                            }
                            while (Freader.Read())
                            {
                                int hour = Convert.ToInt16(Freader.GetValue(5));
                                Model.Constraints constraint = new Model.Constraints();
                                int constraintNum = Convert.ToInt32(Freader.GetValue(1));
                                if (constraintHash.ContainsKey(constraintNum + "@" + date))
                                {
                                    constraint = constraintHash[constraintNum + "@" + date];
                                }
                                else
                                {
                                    constraintHash.Add(constraintNum + "@" + date, constraint);

                                    //get from/to zones
                                    mToZone = "UNK";
                                    mFromZone = "UNK";
                                    string test = null;
                                    constraintMaxZone.TryGetValue(constraintNum, out test);
                                    if (test != null) { }
                                    else
                                    {
                                        MySqlDataReader maxReader = null;
                                        if (Marketkey == 1)
                                        {
                                            mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                            maxReader = mGetMaxNode.ExecuteReader();
                                        }
                                        else
                                        {
                                            mGetErcotMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                            maxReader = mGetErcotMaxNode.ExecuteReader();
                                        }

                                        while (maxReader.Read())
                                        {
                                            if (maxReader.IsDBNull(0))
                                            {
                                                mMaxNode = 0;
                                            }
                                            else
                                            {
                                                mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                                mToZone = Convert.ToString(maxReader.GetValue(1));
                                                constraintMaxZone.Add(constraintNum, mToZone);
                                            }
                                        }
                                        maxReader.Close();
                                        MySqlDataReader minReader = null;
                                        if (Marketkey == 1)
                                        {
                                            mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                            minReader = mGetMinNode.ExecuteReader();
                                        }
                                        else
                                        {
                                            mGetErcotMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                            minReader = mGetErcotMinNode.ExecuteReader();
                                        }
                                        while (minReader.Read())
                                        {
                                            if (minReader.IsDBNull(0))
                                            {
                                                mMinNode = 0;
                                            }
                                            else
                                            {
                                                mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                                mFromZone = Convert.ToString(minReader.GetValue(1));
                                                constraintMinZone.Add(constraintNum, mFromZone);
                                            }
                                        }
                                        minReader.Close();
                                    }
                                }
                                constraint.date = date;
                                constraint.constraintNum = constraintNum;
                                string toZone = null; string fromZone = null;
                                constraintMaxZone.TryGetValue(constraintNum, out toZone);
                                constraintMinZone.TryGetValue(constraintNum, out fromZone);
                                constraint.fromZone = fromZone;
                                constraint.toZone = toZone;
                                constraint.monitoredName = Freader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                                constraint.contingName = Freader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                                if (hour == 1) { constraint.he1Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 2) { constraint.he2Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 3) { constraint.he3Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 4) { constraint.he4Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 5) { constraint.he5Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 6) { constraint.he6Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 7) { constraint.he7Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 8) { constraint.he8Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 9) { constraint.he9Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 10) { constraint.he10Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 11) { constraint.he11Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 12) { constraint.he12Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 13) { constraint.he13Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 14) { constraint.he14Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 15) { constraint.he15Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 16) { constraint.he16Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 17) { constraint.he17Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 18) { constraint.he18Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 19) { constraint.he19Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 20) { constraint.he20Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 21) { constraint.he21Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 22) { constraint.he22Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 23) { constraint.he23Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 24) { constraint.he24Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                constraint.dayofweek = dayofweek;
                            }
                            Freader.Close();
                        }
                    }
                }
                ConstraintsList = constraintHash.Values.ToList<Model.Constraints>();
                Datereader.Close();
                VayuConnection.Close();
            }

            //RT Shifted on Lookback
            if (RtRadioButton == true && ShiftedRadioButton == true && DateRangeCheckBox == true)
            {
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                if (FamilyCheckBox == true)
                {
                    mGetFamilyDates.Parameters["@familyName"].Value = SelectedFamily.ToString();
                    mGetFamilyDates.Parameters["@startDate"].Value = DatePicker1.Date;
                    mGetFamilyDates.Parameters["@endDate"].Value = DatePicker2.Date;
                    Datereader = mGetFamilyDates.ExecuteReader();
                }
                else
                {
                    //if (Marketkey == 1)
                    //{
                    //    mGetDates.Parameters["@startDate"].Value = DatePicker1.Date;
                    //    mGetDates.Parameters["@endDate"].Value = DatePicker2.Date;
                    //    Datereader = mGetDates.ExecuteReader();
                    //}
                    //else
                    {
                        mGetErcotDates.Parameters["@startDate"].Value = DatePicker1.Date;
                        mGetErcotDates.Parameters["@endDate"].Value = DatePicker2.Date;
                        Datereader = mGetErcotDates.ExecuteReader();
                    }
                }
                Dictionary<string, Model.Constraints> constraintHash = new Dictionary<string, Model.Constraints>();
                Dictionary<int, string> constraintMaxZone = new Dictionary<int, string>();
                Dictionary<int, string> constraintMinZone = new Dictionary<int, string>();
                while (Datereader.Read())
                {
                    string date = Convert.ToString(Datereader.GetValue(0));
                    DateTime dateRetrieve = Convert.ToDateTime(Datereader.GetValue(0));
                    string dayofweek = Convert.ToString(dateRetrieve.DayOfWeek);
                    if (FamilyCheckBox == true)
                    {
                        mGetRTShiftedFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                        mGetRTShiftedFamily.Parameters["@startDate"].Value = date;
                        reader = mGetRTShiftedFamily.ExecuteReader();
                    }
                    else
                    {
                        if (Marketkey == 1)
                        {
                            mGetRTShifted.Parameters["@startDate"].Value = date;
                            reader = mGetRTShifted.ExecuteReader();
                        }
                        else
                        {
                            mGetErcotRTShifted.Parameters["@startDate"].Value = date;
                            reader = mGetErcotRTShifted.ExecuteReader();
                        }
                    }
                    while (reader.Read())
                    {
                        int hour = Convert.ToInt16(reader.GetValue(5));
                        Model.Constraints constraint = new Model.Constraints();
                        int constraintNum = Convert.ToInt32(reader.GetValue(1));

                        if (constraintHash.ContainsKey(constraintNum + "@" + date))
                        {
                            constraint = constraintHash[constraintNum + "@" + date];
                        }
                        else
                        {
                            constraintHash.Add(constraintNum + "@" + date, constraint);

                            //get from/to zones
                            mToZone = "UNK";
                            mFromZone = "UNK";
                            string test = null;
                            constraintMaxZone.TryGetValue(constraintNum, out test);
                            if (test != null) { }
                            else
                            {
                                MySqlDataReader maxReader = null;
                                if (Marketkey == 1)
                                {
                                    mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                    maxReader = mGetMaxNode.ExecuteReader();
                                }
                                else
                                {
                                    mGetErcotMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                    maxReader = mGetErcotMaxNode.ExecuteReader();
                                }
                                while (maxReader.Read())
                                {
                                    if (maxReader.IsDBNull(0))
                                    {
                                        mMaxNode = 0;
                                    }
                                    else
                                    {
                                        mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                        mToZone = Convert.ToString(maxReader.GetValue(1));
                                        constraintMaxZone.Add(constraintNum, mToZone);
                                    }
                                }
                                maxReader.Close();
                                MySqlDataReader minReader = null;
                                if (Marketkey == 1)
                                {
                                    mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                    minReader = mGetMinNode.ExecuteReader();
                                }
                                else
                                {
                                    mGetErcotMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                    minReader = mGetErcotMinNode.ExecuteReader();
                                }
                                while (minReader.Read())
                                {
                                    if (minReader.IsDBNull(0))
                                    {
                                        mMinNode = 0;
                                    }
                                    else
                                    {
                                        mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                        mFromZone = Convert.ToString(minReader.GetValue(1));
                                        constraintMinZone.Add(constraintNum, mFromZone);
                                    }
                                }
                                minReader.Close();
                            }
                        }
                        constraint.date = date;
                        constraint.constraintNum = constraintNum;
                        string toZone = null; string fromZone = null;
                        constraintMaxZone.TryGetValue(constraintNum, out toZone);
                        constraintMinZone.TryGetValue(constraintNum, out fromZone);
                        constraint.fromZone = fromZone;
                        constraint.toZone = toZone;
                        constraint.monitoredName = reader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                        constraint.contingName = reader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                        if (hour == 1) { constraint.he1Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 2) { constraint.he2Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 3) { constraint.he3Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 4) { constraint.he4Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 5) { constraint.he5Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 6) { constraint.he6Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 7) { constraint.he7Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 8) { constraint.he8Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 9) { constraint.he9Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 10) { constraint.he10Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 11) { constraint.he11Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 12) { constraint.he12Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 13) { constraint.he13Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 14) { constraint.he14Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 15) { constraint.he15Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 16) { constraint.he16Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 17) { constraint.he17Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 18) { constraint.he18Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 19) { constraint.he19Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 20) { constraint.he20Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 21) { constraint.he21Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 22) { constraint.he22Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 23) { constraint.he23Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 24) { constraint.he24Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        constraint.dayofweek = dayofweek;
                    }
                    reader.Close();
                    DateTime currentTime = DateTime.Now;
                    //if today is the date user selected we need to deal with current hour impact estimate
                    if (Convert.ToString(currentTime.Date) == date)
                    {
                        dayofweek = Convert.ToString(currentTime.DayOfWeek);
                        int currentHour = Convert.ToInt32(currentTime.ToString("HH")) + 1;
                        //make it hour ending by adding 1
                        currentHour = currentHour + 1;
                        int maxImpactHour = 0;
                        int currentMinute = Convert.ToInt32(currentTime.ToString("mm"));
                        double currentElapsedInterval = 0;
                        if (FamilyCheckBox == true)
                        {
                            mGetMaxImpactHourTodayFamily.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                            mGetMaxImpactHourTodayFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                            Ireader = mGetMaxImpactHourTodayFamily.ExecuteReader();
                        }
                        else
                        {
                            if (Marketkey == 1)
                            {
                                mGetMaxImpactHourToday.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                                Ireader = mGetMaxImpactHourToday.ExecuteReader();
                            }
                            else
                            {
                                mGetErcotMaxImpactHourToday.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                                Ireader = mGetErcotMaxImpactHourToday.ExecuteReader();
                            }
                        }
                        while (Ireader.Read())
                        {
                            if (!Ireader.IsDBNull(0))
                            {
                                maxImpactHour = Convert.ToInt32(Ireader.GetValue(0));
                            }
                        }
                        Ireader.Close();
                        //when time == -1 we run the query once -- one intervals value
                        if (maxImpactHour - currentHour == -1) //we need to divide sum(SP)*SF by intervals elapsed
                        {
                            currentElapsedInterval = currentMinute / 5;
                            currentElapsedInterval = Math.Floor(currentElapsedInterval);
                            if (FamilyCheckBox == true)
                            {
                                mGetThisHourRTShiftedFamily.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisHourRTShiftedFamily.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetThisHourRTShiftedFamily.Parameters["@currentHour"].Value = currentHour;
                                mGetThisHourRTShiftedFamily.Parameters["@intervals"].Value = currentElapsedInterval;
                                mGetThisHourRTShiftedFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                                Jreader = mGetThisHourRTShiftedFamily.ExecuteReader();
                            }
                            else
                            {
                                if (Marketkey == 1)
                                {
                                    mGetThisHourRTShifted.Parameters["@realTimeDate"].Value = currentTime.Date;
                                    mGetThisHourRTShifted.Parameters["@startHour"].Value = maxImpactHour + 1;
                                    mGetThisHourRTShifted.Parameters["@currentHour"].Value = currentHour;
                                    mGetThisHourRTShifted.Parameters["@intervals"].Value = currentElapsedInterval;
                                    Jreader = mGetThisHourRTShifted.ExecuteReader();
                                }
                                else
                                {
                                    mGetThisErcotHourRTShifted.Parameters["@realTimeDate"].Value = currentTime.Date;
                                    mGetThisErcotHourRTShifted.Parameters["@startHour"].Value = maxImpactHour + 1;
                                    mGetThisErcotHourRTShifted.Parameters["@currentHour"].Value = currentHour;
                                    mGetThisErcotHourRTShifted.Parameters["@intervals"].Value = currentElapsedInterval;
                                    Jreader = mGetThisErcotHourRTShifted.ExecuteReader();
                                }
                            }
                            while (Jreader.Read())
                            {
                                int hour = Convert.ToInt16(Jreader.GetValue(5));
                                Model.Constraints constraint = new Model.Constraints();
                                int constraintNum = Convert.ToInt32(Jreader.GetValue(1));
                                if (constraintHash.ContainsKey(constraintNum + "@" + date))
                                {
                                    constraint = constraintHash[constraintNum + "@" + date];
                                }
                                else
                                {
                                    constraintHash.Add(constraintNum + "@" + date, constraint);

                                    //get from/to zones
                                    mToZone = "UNK";
                                    mFromZone = "UNK";
                                    string test = null;
                                    constraintMaxZone.TryGetValue(constraintNum, out test);
                                    if (test != null) { }
                                    else
                                    {
                                        MySqlDataReader maxReader = null;
                                        if (Marketkey == 1)
                                        {
                                            mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                            maxReader = mGetMaxNode.ExecuteReader();
                                        }
                                        else
                                        {
                                            mGetErcotMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                            maxReader = mGetErcotMaxNode.ExecuteReader();
                                        }
                                        while (maxReader.Read())
                                        {
                                            if (maxReader.IsDBNull(0))
                                            {
                                                mMaxNode = 0;
                                            }
                                            else
                                            {
                                                mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                                mToZone = Convert.ToString(maxReader.GetValue(1));
                                                constraintMaxZone.Add(constraintNum, mToZone);
                                            }
                                        }
                                        maxReader.Close();
                                        MySqlDataReader minReader = null;
                                        if (Marketkey == 1)
                                        {
                                            mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                            minReader = mGetMinNode.ExecuteReader();
                                        }
                                        else
                                        {
                                            mGetErcotMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                            minReader = mGetErcotMinNode.ExecuteReader();
                                        }
                                        while (minReader.Read())
                                        {
                                            if (minReader.IsDBNull(0))
                                            {
                                                mMinNode = 0;
                                            }
                                            else
                                            {
                                                mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                                mFromZone = Convert.ToString(minReader.GetValue(1));
                                                constraintMinZone.Add(constraintNum, mFromZone);
                                            }
                                        }
                                        minReader.Close();
                                    }
                                }
                                constraint.date = date;
                                constraint.constraintNum = constraintNum;
                                string toZone = null; string fromZone = null;
                                constraintMaxZone.TryGetValue(constraintNum, out toZone);
                                constraintMinZone.TryGetValue(constraintNum, out fromZone);
                                constraint.fromZone = fromZone;
                                constraint.toZone = toZone;
                                constraint.monitoredName = Jreader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                                constraint.contingName = Jreader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                                if (hour == 1) { constraint.he1Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 2) { constraint.he2Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 3) { constraint.he3Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 4) { constraint.he4Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 5) { constraint.he5Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 6) { constraint.he6Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 7) { constraint.he7Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 8) { constraint.he8Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 9) { constraint.he9Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 10) { constraint.he10Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 11) { constraint.he11Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 12) { constraint.he12Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 13) { constraint.he13Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 14) { constraint.he14Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 15) { constraint.he15Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 16) { constraint.he16Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 17) { constraint.he17Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 18) { constraint.he18Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 19) { constraint.he19Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 20) { constraint.he20Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 21) { constraint.he21Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 22) { constraint.he22Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 23) { constraint.he23Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                if (hour == 24) { constraint.he24Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                                constraint.dayofweek = dayofweek;
                            }
                            Jreader.Close();
                        }
                        if (maxImpactHour - currentHour < -1) // we need to divide sum(SP*SF) by 12 and just assume we scraped all intervals
                        {
                            //select shift factor, constraintrtnum, monitoredtext, contingencytext, sum(SP*SF)/12, datepart(HH, marketdatetime)+1 
                            //where hours between maxImpactHour+1 and currentHour
                            //where date is today
                            if (FamilyCheckBox == true)
                            {
                                mGetThisHourRTShiftedFamily.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisHourRTShiftedFamily.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetThisHourRTShiftedFamily.Parameters["@currentHour"].Value = currentHour;
                                mGetThisHourRTShiftedFamily.Parameters["@intervals"].Value = 12;
                                mGetThisHourRTShiftedFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                                Freader = mGetThisHourRTShiftedFamily.ExecuteReader();
                            }
                            else
                            {
                                if (Marketkey == 1)
                                {
                                    mGetThisHourRTShifted.Parameters["@realTimeDate"].Value = currentTime.Date;
                                    mGetThisHourRTShifted.Parameters["@startHour"].Value = maxImpactHour + 1;
                                    mGetThisHourRTShifted.Parameters["@currentHour"].Value = currentHour;
                                    mGetThisHourRTShifted.Parameters["@intervals"].Value = 12;
                                    Freader = mGetThisHourRTShifted.ExecuteReader();
                                }
                                else
                                {
                                    mGetThisErcotHourRTShifted.Parameters["@realTimeDate"].Value = currentTime.Date;
                                    mGetThisErcotHourRTShifted.Parameters["@startHour"].Value = maxImpactHour + 1;
                                    mGetThisErcotHourRTShifted.Parameters["@currentHour"].Value = currentHour;
                                    mGetThisErcotHourRTShifted.Parameters["@intervals"].Value = 12;
                                    Freader = mGetThisErcotHourRTShifted.ExecuteReader();
                                }
                            }
                            while (Freader.Read())
                            {
                                int hour = Convert.ToInt16(Freader.GetValue(5));
                                Model.Constraints constraint = new Model.Constraints();
                                int constraintNum = Convert.ToInt32(Freader.GetValue(1));
                                if (constraintHash.ContainsKey(constraintNum + "@" + date))
                                {
                                    constraint = constraintHash[constraintNum + "@" + date];
                                }
                                else
                                {
                                    constraintHash.Add(constraintNum + "@" + date, constraint);

                                    //get from/to zones
                                    mToZone = "UNK";
                                    mFromZone = "UNK";
                                    string test = null;
                                    constraintMaxZone.TryGetValue(constraintNum, out test);
                                    if (test != null) { }
                                    else
                                    {
                                        MySqlDataReader maxReader = null;
                                        if (Marketkey == 1)
                                        {
                                            mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                            maxReader = mGetMaxNode.ExecuteReader();
                                        }
                                        else
                                        {
                                            mGetErcotMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                            maxReader = mGetErcotMaxNode.ExecuteReader();
                                        }
                                        while (maxReader.Read())
                                        {
                                            if (maxReader.IsDBNull(0))
                                            {
                                                mMaxNode = 0;
                                            }
                                            else
                                            {
                                                mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                                mToZone = Convert.ToString(maxReader.GetValue(1));
                                                constraintMaxZone.Add(constraintNum, mToZone);
                                            }
                                        }
                                        maxReader.Close();
                                        MySqlDataReader minReader = null;
                                        if (Marketkey == 1)
                                        {
                                            mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                            minReader = mGetMinNode.ExecuteReader();
                                        }
                                        else
                                        {
                                            mGetErcotMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                            minReader = mGetErcotMinNode.ExecuteReader();
                                        }
                                        while (minReader.Read())
                                        {
                                            if (minReader.IsDBNull(0))
                                            {
                                                mMinNode = 0;
                                            }
                                            else
                                            {
                                                mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                                mFromZone = Convert.ToString(minReader.GetValue(1));
                                                constraintMinZone.Add(constraintNum, mFromZone);
                                            }
                                        }
                                        minReader.Close();
                                    }
                                }
                                constraint.date = date;
                                constraint.constraintNum = constraintNum;
                                string toZone = null; string fromZone = null;
                                constraintMaxZone.TryGetValue(constraintNum, out toZone);
                                constraintMinZone.TryGetValue(constraintNum, out fromZone);
                                constraint.fromZone = fromZone;
                                constraint.toZone = toZone;
                                constraint.monitoredName = Freader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                                constraint.contingName = Freader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                                if (hour == 1) { constraint.he1Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 2) { constraint.he2Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 3) { constraint.he3Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 4) { constraint.he4Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 5) { constraint.he5Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 6) { constraint.he6Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 7) { constraint.he7Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 8) { constraint.he8Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 9) { constraint.he9Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 10) { constraint.he10Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 11) { constraint.he11Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 12) { constraint.he12Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 13) { constraint.he13Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 14) { constraint.he14Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 15) { constraint.he15Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 16) { constraint.he16Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 17) { constraint.he17Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 18) { constraint.he18Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 19) { constraint.he19Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 20) { constraint.he20Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 21) { constraint.he21Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 22) { constraint.he22Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 23) { constraint.he23Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                if (hour == 24) { constraint.he24Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                                constraint.dayofweek = dayofweek;
                            }
                            Freader.Close();
                        }
                    }
                }
                ConstraintsList = constraintHash.Values.ToList<Model.Constraints>();
                Datereader.Close();
                VayuConnection.Close();
            }

            //RT Impact no Lookback
            if (RtRadioButton == true && ImpactRadioButton == true && DateRangeCheckBox == false)
            {
                Dictionary<int, Model.Constraints> constraintHash = new Dictionary<int, Model.Constraints>();
                Dictionary<int, string> constraintMaxZone = new Dictionary<int, string>();
                Dictionary<int, string> constraintMinZone = new Dictionary<int, string>();
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                if (FamilyCheckBox == true)
                {
                    mGetHourlyRTImpactFamily.Parameters["@startDate"].Value = DatePicker1.Date;
                    mGetHourlyRTImpactFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                    reader = mGetHourlyRTImpactFamily.ExecuteReader();
                }
                else
                {
                    if (Marketkey == 1)
                    {
                        mGetHourlyRTImpact.Parameters["@startDate"].Value = DatePicker1.Date;
                        reader = mGetHourlyRTImpact.ExecuteReader();
                    }
                    else
                    {
                        mGetErcotHourlyRTImpact.Parameters["@startDate"].Value = DatePicker1.Date;
                        reader = mGetErcotHourlyRTImpact.ExecuteReader();
                    }
                }
                //use date to populate outage info
                DateTime day = Convert.ToDateTime(DatePicker1.Date);
                DateTime Today = DateTime.Now;
                List<string> EquipmentList = new List<string>();
                MySqlDataReader mreader = null;
                if (Marketkey == 1)
                {
                    mGetDistinctEquipment.Parameters["@day"].Value = day.Date;
                    mreader = mGetDistinctEquipment.ExecuteReader();
                }
                else
                {
                    mGetErcotDistinctEquipment.Parameters["@day"].Value = day.Date;
                    mreader = mGetErcotDistinctEquipment.ExecuteReader();
                }
                while (mreader.Read())
                {
                    string equipment = mreader.GetString(0);
                    EquipmentList.Add(equipment);
                }
                mreader.Close();
                MySqlDataReader archiveReader = null;
                if (Marketkey == 1)
                {
                    mSelectArchivedCurrentOutages.Parameters["@day"].Value = day.Date;
                    archiveReader = mSelectArchivedCurrentOutages.ExecuteReader();


                    while (archiveReader.Read())
                    {
                        string equipment = archiveReader.GetString(0);
                        EquipmentList.Add(equipment);
                    }
                    archiveReader.Close();

                    mSelectArchivedSchedOutages.Parameters["@day"].Value = day.Date;
                    MySqlDataReader overReader = mSelectArchivedSchedOutages.ExecuteReader();
                    while (overReader.Read())
                    {
                        string result = null;
                        result = EquipmentList.Where(s => s == overReader.GetString(0)).FirstOrDefault();
                        if (result != null) { }
                        else
                        {
                            string missing = overReader.GetString(0);
                            EquipmentList.Add(missing);
                        }
                    }
                    overReader.Close();

                    mSelectArchivedCurrentOutagesActive.Parameters["@day"].Value = day.Date;
                    MySqlDataReader forcedActiveReader = mSelectArchivedCurrentOutagesActive.ExecuteReader();
                    while (forcedActiveReader.Read())
                    {
                        string result = null;
                        result = EquipmentList.Where(s => s == forcedActiveReader.GetString(0)).FirstOrDefault();
                        if (result != null) { }
                        else
                        {
                            string activeForced = forcedActiveReader.GetString(0);
                            EquipmentList.Add(activeForced);
                        }
                    }
                    forcedActiveReader.Close();

                    if (Today.Date == day.Date)
                    {
                        MySqlDataReader greader = mGetDistinctEquipmentForced.ExecuteReader();
                        while (greader.Read())
                        {
                            string unschequipment = greader.GetString(0);
                            EquipmentList.Add(unschequipment);
                        }
                        greader.Close();
                    }
                    EquipmentList.Sort();
                }
                //Rajkumar

                //pull in all tags and look to see if they are out of service
                List<string> TaggedListOut = new List<string>();
                //MySqlDataReader tagReader = mSelectTaggedOutages.ExecuteReader();
                //while (tagReader.Read())
                //{
                //    string result = null;
                //    string taggedEquip = tagReader.GetString(0);
                //    result = EquipmentList.Where(s => s == taggedEquip).FirstOrDefault();
                //    if (result != null)
                //    {
                //        TaggedListOut.Add(taggedEquip);
                //    }
                //}
                //tagReader.Close();
                //make list of all constraint nums affected by todays tagged outages
                List<int> LinkedConstraintNums = new List<int>();
                foreach (string tag in TaggedListOut)
                {
                    //query as though it is a number -- when its 0/isdbull do not add to LinkedConstraintNums
                    mGetTaggedConNums.Parameters["@equipment"].Value = tag;
                    MySqlDataReader wreader = mGetTaggedConNums.ExecuteReader();
                    while (wreader.Read())
                    {
                        if (wreader.IsDBNull(0)) { }
                        else
                        {
                            int constNum = Convert.ToInt32(wreader.GetValue(0));
                            LinkedConstraintNums.Add(constNum);
                        }
                    }
                    wreader.Close();
                    //query as though it is a family inner join with RTFamily to get all constaint nums and add them -- error check for isdbnull
                    mGetTaggedConNumsByFam.Parameters["@equipment"].Value = tag;
                    MySqlDataReader famtryReader = mGetTaggedConNumsByFam.ExecuteReader();
                    while (famtryReader.Read())
                    {
                        if (famtryReader.IsDBNull(0)) { }
                        else
                        {
                            int constNum = Convert.ToInt32(famtryReader.GetValue(0));
                            LinkedConstraintNums.Add(constNum);
                        }
                    }
                    famtryReader.Close();
                }
                while (reader.Read())
                {
                    int hour = Convert.ToInt16(reader.GetValue(5));
                    DateTime dateRetrieve = Convert.ToDateTime(DatePicker1.Date);
                    string date = Convert.ToString(DatePicker1.Date);
                    string dayofweek = Convert.ToString(dateRetrieve.DayOfWeek);
                    Model.Constraints constraint = new Model.Constraints();
                    int constraintNum = Convert.ToInt32(reader.GetValue(1));
                    if (constraintHash.ContainsKey(constraintNum))
                    {
                        constraint = constraintHash[constraintNum];
                    }
                    else
                    {
                        constraintHash.Add(constraintNum, constraint);
                        int search = 0;
                        search = LinkedConstraintNums.Where(s => s == constraintNum).FirstOrDefault();
                        if (search > 0) { constraint.color = "yellow"; }
                        //get from/to zones
                        mToZone = "UNK";
                        mFromZone = "UNK";
                        string test = null;
                        constraintMaxZone.TryGetValue(constraintNum, out test);
                        if (test != null) { }
                        else
                        {
                            MySqlDataReader maxReader = null;
                            if (Marketkey == 1)
                            {
                                mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                maxReader = mGetMaxNode.ExecuteReader();
                            }
                            else
                            {
                                mGetErcotMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                maxReader = mGetErcotMaxNode.ExecuteReader();
                            }
                            while (maxReader.Read())
                            {
                                if (maxReader.IsDBNull(0))
                                {
                                    mMaxNode = 0;
                                }
                                else
                                {
                                    mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                    mToZone = Convert.ToString(maxReader.GetValue(1));
                                    constraintMaxZone.Add(constraintNum, mToZone);
                                }
                            }
                            maxReader.Close();
                            MySqlDataReader minReader = null;
                            if (Marketkey == 1)
                            {
                                mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                minReader = mGetMinNode.ExecuteReader();
                            }
                            else
                            {
                                mGetErcotMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                minReader = mGetErcotMinNode.ExecuteReader();
                            }
                            while (minReader.Read())
                            {
                                if (minReader.IsDBNull(0))
                                {
                                    mMinNode = 0;
                                }
                                else
                                {
                                    mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                    mFromZone = Convert.ToString(minReader.GetValue(1));
                                    constraintMinZone.Add(constraintNum, mFromZone);
                                }
                            }
                            minReader.Close();
                        }
                    }
                    constraint.date = date;
                    constraint.constraintNum = constraintNum;
                    string toZone = null; string fromZone = null;
                    constraintMaxZone.TryGetValue(constraintNum, out toZone);
                    constraintMinZone.TryGetValue(constraintNum, out fromZone);
                    constraint.fromZone = fromZone;
                    constraint.toZone = toZone;
                    constraint.monitoredName = reader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                    constraint.contingName = reader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                    if (hour == 1) { constraint.he1Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 2) { constraint.he2Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 3) { constraint.he3Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 4) { constraint.he4Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 5) { constraint.he5Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 6) { constraint.he6Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 7) { constraint.he7Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 8) { constraint.he8Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 9) { constraint.he9Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 10) { constraint.he10Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 11) { constraint.he11Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 12) { constraint.he12Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 13) { constraint.he13Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 14) { constraint.he14Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 15) { constraint.he15Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 16) { constraint.he16Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 17) { constraint.he17Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 18) { constraint.he18Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 19) { constraint.he19Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 20) { constraint.he20Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 21) { constraint.he21Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 22) { constraint.he22Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 23) { constraint.he23Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 24) { constraint.he24Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    constraint.dayofweek = dayofweek;
                }
                VayuConnection.Close();
                DateTime currentTime = DateTime.Now;
                //if today is the date user selected we need to deal with current hour impact estimate
                if (currentTime.Date == DatePicker1.Date)
                {
                    int currentHour = Convert.ToInt32(currentTime.ToString("HH")) + 1;
                    string dayofweek = Convert.ToString(currentTime.DayOfWeek);
                    //make it hour ending by adding 1
                    currentHour = currentHour + 1;
                    int maxImpactHour = 0;
                    int currentMinute = Convert.ToInt32(currentTime.ToString("mm"));
                    double currentElapsedInterval = 0;
                    if (VayuConnection.State == ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    if (FamilyCheckBox == true)
                    {
                        mGetMaxImpactHourTodayFamily.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                        mGetMaxImpactHourTodayFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                        Ireader = mGetMaxImpactHourTodayFamily.ExecuteReader();
                    }
                    else
                    {
                        if (Marketkey == 1)
                        {
                            mGetMaxImpactHourToday.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                            Ireader = mGetMaxImpactHourToday.ExecuteReader();
                        }
                        else
                        {
                            mGetErcotMaxImpactHourToday.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                            Ireader = mGetErcotMaxImpactHourToday.ExecuteReader();
                        }
                    }
                    while (Ireader.Read())
                    {
                        if (!Ireader.IsDBNull(0))
                        {
                            maxImpactHour = Convert.ToInt32(Ireader.GetValue(0));
                        }
                    }
                    Ireader.Close();
                    if (VayuConnection.State == ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    //when time == -1 we run the query once -- one intervals value
                    if (maxImpactHour - currentHour == -1) //we need to divide sum(SP)*SF by intervals elapsed
                    {
                        currentElapsedInterval = currentMinute / 5;
                        currentElapsedInterval = Math.Floor(currentElapsedInterval);
                        if (VayuConnection.State == ConnectionState.Closed)
                        {
                            VayuConnection.Open();
                        }
                        if (FamilyCheckBox == true)
                        {
                            mGetThisHourRTImpactFamily.Parameters["@realTimeDate"].Value = currentTime.Date;
                            mGetThisHourRTImpactFamily.Parameters["@startHour"].Value = maxImpactHour + 1;
                            mGetThisHourRTImpactFamily.Parameters["@currentHour"].Value = currentHour;
                            mGetThisHourRTImpactFamily.Parameters["@intervals"].Value = currentElapsedInterval;
                            mGetThisHourRTImpactFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                            Jreader = mGetThisHourRTImpactFamily.ExecuteReader();
                        }
                        else
                        {
                            mGetThisHourRTImpact.Parameters["@realTimeDate"].Value = currentTime.Date;
                            mGetThisHourRTImpact.Parameters["@startHour"].Value = maxImpactHour + 1;
                            mGetThisHourRTImpact.Parameters["@currentHour"].Value = currentHour;
                            mGetThisHourRTImpact.Parameters["@intervals"].Value = currentElapsedInterval;
                            Jreader = mGetThisHourRTImpact.ExecuteReader();
                        }
                        while (Jreader.Read())
                        {
                            int hour = Convert.ToInt16(Jreader.GetValue(5));
                            string date = Convert.ToString(currentTime.Date);
                            Model.Constraints constraint = new Model.Constraints();
                            int constraintNum = Convert.ToInt32(Jreader.GetValue(1));
                            if (constraintHash.ContainsKey(constraintNum))
                            {
                                constraint = constraintHash[constraintNum];
                            }
                            else
                            {
                                constraintHash.Add(constraintNum, constraint);
                                int search = 0;
                                search = LinkedConstraintNums.Where(s => s == constraintNum).FirstOrDefault();
                                if (search > 0) { constraint.color = "yellow"; }
                                //get from/to zones
                                mToZone = "UNK";
                                mFromZone = "UNK";
                                string test = null;
                                constraintMaxZone.TryGetValue(constraintNum, out test);
                                if (test != null) { }
                                else
                                {
                                    mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                    MySqlDataReader maxReader = mGetMaxNode.ExecuteReader();
                                    while (maxReader.Read())
                                    {
                                        if (maxReader.IsDBNull(0))
                                        {
                                            mMaxNode = 0;
                                        }
                                        else
                                        {
                                            mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                            mToZone = Convert.ToString(maxReader.GetValue(1));
                                            constraintMaxZone.Add(constraintNum, mToZone);
                                        }
                                    }
                                    maxReader.Close();
                                    mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                    MySqlDataReader minReader = mGetMinNode.ExecuteReader();
                                    while (minReader.Read())
                                    {
                                        if (minReader.IsDBNull(0))
                                        {
                                            mMinNode = 0;
                                        }
                                        else
                                        {
                                            mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                            mFromZone = Convert.ToString(minReader.GetValue(1));
                                            constraintMinZone.Add(constraintNum, mFromZone);
                                        }
                                    }
                                    minReader.Close();
                                }
                            }
                            constraint.date = date;
                            constraint.constraintNum = constraintNum;
                            string toZone = null; string fromZone = null;
                            constraintMaxZone.TryGetValue(constraintNum, out toZone);
                            constraintMinZone.TryGetValue(constraintNum, out fromZone);
                            constraint.fromZone = fromZone;
                            constraint.toZone = toZone;
                            constraint.monitoredName = Jreader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                            constraint.contingName = Jreader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                            if (hour == 1) { constraint.he1Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 2) { constraint.he2Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 3) { constraint.he3Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 4) { constraint.he4Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 5) { constraint.he5Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 6) { constraint.he6Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 7) { constraint.he7Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 8) { constraint.he8Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 9) { constraint.he9Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 10) { constraint.he10Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 11) { constraint.he11Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 12) { constraint.he12Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 13) { constraint.he13Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 14) { constraint.he14Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 15) { constraint.he15Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 16) { constraint.he16Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 17) { constraint.he17Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 18) { constraint.he18Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 19) { constraint.he19Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 20) { constraint.he20Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 21) { constraint.he21Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 22) { constraint.he22Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 23) { constraint.he23Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 24) { constraint.he24Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            constraint.dayofweek = dayofweek;
                        }
                        if (VayuConnection.State == ConnectionState.Closed)
                        {
                            VayuConnection.Open();
                        }
                        Jreader.Close();
                    }
                    //most recent constraint scrape as compared to maximum hour
                    //when time is less than -1 we run the query twice -- two intervals values
                    if (maxImpactHour - currentHour < -1) // we need to divide sum(SP*SF) by 12 and just assume we scraped all intervals
                    {
                        if (VayuConnection.State == ConnectionState.Closed)
                        {
                            VayuConnection.Open();
                        }
                        //select shift factor, constraintrtnum, monitoredtext, contingencytext, sum(SP*SF)/12, datepart(HH, marketdatetime)+1 
                        //where hours between maxImpactHour+1 and currentHour
                        //where date is today
                        if (FamilyCheckBox == true)
                        {
                            mGetThisHourRTImpactFamily.Parameters["@realTimeDate"].Value = currentTime.Date;
                            mGetThisHourRTImpactFamily.Parameters["@startHour"].Value = maxImpactHour + 1;
                            mGetThisHourRTImpactFamily.Parameters["@currentHour"].Value = currentHour;
                            mGetThisHourRTImpactFamily.Parameters["@intervals"].Value = 12;
                            mGetThisHourRTImpactFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                            Freader = mGetThisHourRTImpactFamily.ExecuteReader();
                        }
                        else
                        {
                            if (Marketkey == 1)
                            {
                                mGetThisHourRTImpact.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisHourRTImpact.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetThisHourRTImpact.Parameters["@currentHour"].Value = currentHour;
                                mGetThisHourRTImpact.Parameters["@intervals"].Value = 12;
                                Freader = mGetThisHourRTImpact.ExecuteReader();
                            }
                            else
                            {
                                mGetErcotThisHourRTImpact.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetErcotThisHourRTImpact.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetErcotThisHourRTImpact.Parameters["@currentHour"].Value = currentHour;
                                mGetErcotThisHourRTImpact.Parameters["@intervals"].Value = 12;
                                Freader = mGetErcotThisHourRTImpact.ExecuteReader();
                            }
                        }
                        while (Freader.Read())
                        {
                            int hour = Convert.ToInt16(Freader.GetValue(5));
                            string date = Convert.ToString(currentTime.Date);
                            Model.Constraints constraint = new Model.Constraints();
                            int constraintNum = Convert.ToInt32(Freader.GetValue(1));
                            if (constraintHash.ContainsKey(constraintNum))
                            {
                                constraint = constraintHash[constraintNum];
                            }
                            else
                            {
                                constraintHash.Add(constraintNum, constraint);
                                int search = 0;
                                search = LinkedConstraintNums.Where(s => s == constraintNum).FirstOrDefault();
                                if (search > 0) { constraint.color = "yellow"; }
                                //get from/to zones
                                mToZone = "UNK";
                                mFromZone = "UNK";
                                string test = null;
                                constraintMaxZone.TryGetValue(constraintNum, out test);
                                if (test != null) { }
                                else
                                {
                                    MySqlDataReader maxReader = null;
                                    if (Marketkey == 1)
                                    {
                                        mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                        maxReader = mGetMaxNode.ExecuteReader();
                                    }
                                    else
                                    {
                                        mGetErcotMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                        maxReader = mGetErcotMaxNode.ExecuteReader();
                                    }
                                    while (maxReader.Read())
                                    {
                                        if (maxReader.IsDBNull(0))
                                        {
                                            mMaxNode = 0;
                                        }
                                        else
                                        {
                                            mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                            mToZone = Convert.ToString(maxReader.GetValue(1));
                                            constraintMaxZone.Add(constraintNum, mToZone);
                                        }
                                    }
                                    maxReader.Close();
                                    MySqlDataReader minReader = null;
                                    if (Marketkey == 1)
                                    {
                                        mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                        minReader = mGetMinNode.ExecuteReader();
                                    }
                                    else
                                    {
                                        mGetErcotMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                        minReader = mGetErcotMinNode.ExecuteReader();
                                    }
                                    while (minReader.Read())
                                    {
                                        if (minReader.IsDBNull(0))
                                        {
                                            mMinNode = 0;
                                        }
                                        else
                                        {
                                            mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                            mFromZone = Convert.ToString(minReader.GetValue(1));
                                            constraintMinZone.Add(constraintNum, mFromZone);
                                        }
                                    }
                                    minReader.Close();
                                }
                            }
                            constraint.date = date;
                            constraint.constraintNum = constraintNum;
                            string toZone = null; string fromZone = null;
                            constraintMaxZone.TryGetValue(constraintNum, out toZone);
                            constraintMinZone.TryGetValue(constraintNum, out fromZone);
                            constraint.fromZone = fromZone;
                            constraint.toZone = toZone;
                            constraint.monitoredName = Freader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                            constraint.contingName = Freader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                            if (hour == 1) { constraint.he1Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 2) { constraint.he2Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 3) { constraint.he3Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 4) { constraint.he4Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 5) { constraint.he5Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 6) { constraint.he6Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 7) { constraint.he7Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 8) { constraint.he8Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 9) { constraint.he9Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 10) { constraint.he10Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 11) { constraint.he11Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 12) { constraint.he12Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 13) { constraint.he13Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 14) { constraint.he14Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 15) { constraint.he15Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 16) { constraint.he16Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 17) { constraint.he17Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 18) { constraint.he18Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 19) { constraint.he19Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 20) { constraint.he20Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 21) { constraint.he21Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 22) { constraint.he22Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 23) { constraint.he23Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 24) { constraint.he24Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            constraint.dayofweek = dayofweek;
                        }
                        VayuConnection.Close();
                        Freader.Close();
                    }
                }
                ConstraintsList = constraintHash.Values.ToList<Model.Constraints>();
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                reader.Close();
                VayuConnection.Close();
            }

            //RT Shadows no Lookback
            if (RtRadioButton == true && ShadowRadioButton == true && DateRangeCheckBox == false)
            {
                Dictionary<int, Model.Constraints> constraintHash = new Dictionary<int, Model.Constraints>();
                Dictionary<int, string> constraintMaxZone = new Dictionary<int, string>();
                Dictionary<int, string> constraintMinZone = new Dictionary<int, string>();
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                if (FamilyCheckBox == true)
                {
                    mGetRTShadowsFamily.Parameters["@startDate"].Value = DatePicker1.Date;
                    mGetRTShadowsFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                    reader = mGetRTShadowsFamily.ExecuteReader();
                }
                else
                {
                    if (Marketkey == 1)
                    {
                        mGetRTShadows.Parameters["@startDate"].Value = DatePicker1.Date;
                        reader = mGetRTShadows.ExecuteReader();
                    }
                    else
                    {
                        mGetErcotRTShadows.Parameters["@startDate"].Value = DatePicker1.Date;
                        reader = mGetErcotRTShadows.ExecuteReader();
                    }
                }
                DateTime dateRetrieve = Convert.ToDateTime(DatePicker1.Date);
                string dayofweek = Convert.ToString(dateRetrieve.DayOfWeek);
                while (reader.Read())
                {
                    int hour = Convert.ToInt16(reader.GetValue(5));
                    string date = Convert.ToString(DatePicker1.Date);
                    Model.Constraints constraint = new Model.Constraints();
                    int constraintNum = Convert.ToInt32(reader.GetValue(1));
                    if (constraintHash.ContainsKey(constraintNum))
                    {
                        constraint = constraintHash[constraintNum];
                    }
                    else
                    {
                        constraintHash.Add(constraintNum, constraint);

                        //get from/to zones
                        mToZone = "UNK";
                        mFromZone = "UNK";
                        string test = null;
                        constraintMaxZone.TryGetValue(constraintNum, out test);
                        if (test != null) { }
                        else
                        {
                            MySqlDataReader maxReader = null;
                            if (Marketkey == 1)
                            {
                                mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                maxReader = mGetMaxNode.ExecuteReader();
                            }
                            else
                            {
                                mGetErcotMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                maxReader = mGetErcotMaxNode.ExecuteReader();
                            }
                            while (maxReader.Read())
                            {
                                if (maxReader.IsDBNull(0))
                                {
                                    mMaxNode = 0;
                                }
                                else
                                {
                                    mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                    mToZone = Convert.ToString(maxReader.GetValue(1));
                                    constraintMaxZone.Add(constraintNum, mToZone);
                                }
                            }
                            maxReader.Close();
                            MySqlDataReader minReader = null;
                            if (Marketkey == 1)
                            {
                                mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                minReader = mGetMinNode.ExecuteReader();
                            }
                            else
                            {
                                mGetErcotMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                minReader = mGetErcotMinNode.ExecuteReader();
                            }
                            while (minReader.Read())
                            {
                                if (minReader.IsDBNull(0))
                                {
                                    mMinNode = 0;
                                }
                                else
                                {
                                    mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                    mFromZone = Convert.ToString(minReader.GetValue(1));
                                    constraintMinZone.Add(constraintNum, mFromZone);
                                }
                            }
                            minReader.Close();
                        }
                    }
                    constraint.date = date;
                    constraint.constraintNum = constraintNum;
                    string toZone = null; string fromZone = null;
                    constraintMaxZone.TryGetValue(constraintNum, out toZone);
                    constraintMinZone.TryGetValue(constraintNum, out fromZone);
                    constraint.fromZone = fromZone;
                    constraint.toZone = toZone;
                    constraint.monitoredName = reader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                    constraint.contingName = reader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                    if (hour == 1) { constraint.he1Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 2) { constraint.he2Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 3) { constraint.he3Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 4) { constraint.he4Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 5) { constraint.he5Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 6) { constraint.he6Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 7) { constraint.he7Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 8) { constraint.he8Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 9) { constraint.he9Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 10) { constraint.he10Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 11) { constraint.he11Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 12) { constraint.he12Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 13) { constraint.he13Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 14) { constraint.he14Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 15) { constraint.he15Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 16) { constraint.he16Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 17) { constraint.he17Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 18) { constraint.he18Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 19) { constraint.he19Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 20) { constraint.he20Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 21) { constraint.he21Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 22) { constraint.he22Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 23) { constraint.he23Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 24) { constraint.he24Value = reader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    constraint.dayofweek = dayofweek;
                }
                VayuConnection.Close();
                DateTime currentTime = DateTime.Now;
                //if today is the date user selected we need to deal with current hour impact estimate
                if (currentTime.Date == DatePicker1.Date)
                {
                    int currentHour = Convert.ToInt32(currentTime.ToString("HH")) + 1;
                    //make it hour ending by adding 1
                    currentHour = currentHour + 1;
                    int maxImpactHour = 0;
                    int currentMinute = Convert.ToInt32(currentTime.ToString("mm"));
                    double currentElapsedInterval = 0;
                    if (VayuConnection.State == ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    if (FamilyCheckBox == true)
                    {
                        mGetMaxImpactHourTodayFamily.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                        mGetMaxImpactHourTodayFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                        Ireader = mGetMaxImpactHourTodayFamily.ExecuteReader();
                    }
                    else
                    {
                        if (Marketkey == 1)
                        {
                            mGetMaxImpactHourToday.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                            Ireader = mGetMaxImpactHourToday.ExecuteReader();
                        }
                        else
                        {
                            mGetErcotMaxImpactHourToday.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                            Ireader = mGetErcotMaxImpactHourToday.ExecuteReader();
                        }
                    }
                    while (Ireader.Read())
                    {
                        if (!Ireader.IsDBNull(0))
                        {
                            maxImpactHour = Convert.ToInt32(Ireader.GetValue(0));
                        }
                    }
                    Ireader.Close();
                    VayuConnection.Close();
                    //when time == -1 we run the query once -- one intervals value
                    if (maxImpactHour - currentHour == -1) //we need to divide sum(SP)*SF by intervals elapsed
                    {
                        currentElapsedInterval = currentMinute / 5;
                        currentElapsedInterval = Math.Floor(currentElapsedInterval);
                        if (VayuConnection.State == ConnectionState.Closed)
                        {
                            VayuConnection.Open();
                        }
                        if (FamilyCheckBox == true)
                        {
                            mGetThisHourRTShadowsFamily.Parameters["@realTimeDate"].Value = currentTime.Date;
                            mGetThisHourRTShadowsFamily.Parameters["@startHour"].Value = maxImpactHour + 1;
                            mGetThisHourRTShadowsFamily.Parameters["@currentHour"].Value = currentHour;
                            mGetThisHourRTShadowsFamily.Parameters["@intervals"].Value = currentElapsedInterval;
                            mGetThisHourRTShadowsFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                            Jreader = mGetThisHourRTShadowsFamily.ExecuteReader();
                        }
                        else
                        {
                            if (Marketkey == 1)
                            {
                                mGetThisHourRTShadows.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisHourRTShadows.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetThisHourRTShadows.Parameters["@currentHour"].Value = currentHour;
                                mGetThisHourRTShadows.Parameters["@intervals"].Value = currentElapsedInterval;
                                Jreader = mGetThisHourRTShadows.ExecuteReader();
                            }
                            else
                            {

                                mGetThisErcotHourRTShadows.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisErcotHourRTShadows.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetThisErcotHourRTShadows.Parameters["@currentHour"].Value = currentHour;
                                mGetThisErcotHourRTShadows.Parameters["@intervals"].Value = currentElapsedInterval;
                                Jreader = mGetThisErcotHourRTShadows.ExecuteReader();
                            }
                        }
                        while (Jreader.Read())
                        {
                            int hour = Convert.ToInt16(Jreader.GetValue(5));
                            string date = Convert.ToString(currentTime.Date);
                            Model.Constraints constraint = new Model.Constraints();
                            int constraintNum = Convert.ToInt32(Jreader.GetValue(1));
                            if (constraintHash.ContainsKey(constraintNum))
                            {
                                constraint = constraintHash[constraintNum];
                            }
                            else
                            {
                                constraintHash.Add(constraintNum, constraint);

                                //get from/to zones
                                mToZone = "UNK";
                                mFromZone = "UNK";
                                string test = null;
                                constraintMaxZone.TryGetValue(constraintNum, out test);
                                if (test != null) { }
                                else
                                {
                                    mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                    MySqlDataReader maxReader = mGetMaxNode.ExecuteReader();
                                    while (maxReader.Read())
                                    {
                                        if (maxReader.IsDBNull(0))
                                        {
                                            mMaxNode = 0;
                                        }
                                        else
                                        {
                                            mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                            mToZone = Convert.ToString(maxReader.GetValue(1));
                                            constraintMaxZone.Add(constraintNum, mToZone);
                                        }
                                    }
                                    maxReader.Close();
                                    mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                    MySqlDataReader minReader = mGetMinNode.ExecuteReader();
                                    while (minReader.Read())
                                    {
                                        if (minReader.IsDBNull(0))
                                        {
                                            mMinNode = 0;
                                        }
                                        else
                                        {
                                            mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                            mFromZone = Convert.ToString(minReader.GetValue(1));
                                            constraintMinZone.Add(constraintNum, mFromZone);
                                        }
                                    }
                                    minReader.Close();
                                }
                            }
                            constraint.date = date;
                            constraint.constraintNum = constraintNum;
                            string toZone = null; string fromZone = null;
                            constraintMaxZone.TryGetValue(constraintNum, out toZone);
                            constraintMinZone.TryGetValue(constraintNum, out fromZone);
                            constraint.fromZone = fromZone;
                            constraint.toZone = toZone;
                            constraint.monitoredName = Jreader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                            constraint.contingName = Jreader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                            if (hour == 1) { constraint.he1Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 2) { constraint.he2Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 3) { constraint.he3Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 4) { constraint.he4Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 5) { constraint.he5Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 6) { constraint.he6Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 7) { constraint.he7Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 8) { constraint.he8Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 9) { constraint.he9Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 10) { constraint.he10Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 11) { constraint.he11Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 12) { constraint.he12Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 13) { constraint.he13Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 14) { constraint.he14Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 15) { constraint.he15Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 16) { constraint.he16Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 17) { constraint.he17Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 18) { constraint.he18Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 19) { constraint.he19Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 20) { constraint.he20Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 21) { constraint.he21Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 22) { constraint.he22Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 23) { constraint.he23Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 24) { constraint.he24Value = Jreader.IsDBNull(4) ? "" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            constraint.dayofweek = dayofweek;
                        }
                        VayuConnection.Close();
                        Jreader.Close();
                    }
                    //most recent constraint scrape as compared to maximum hour
                    //when time is less than -1 we run the query twice -- two intervals values
                    if (maxImpactHour - currentHour < -1) // we need to divide sum(SP*SF) by 12 and just assume we scraped all intervals
                    {
                        //select shift factor, constraintrtnum, monitoredtext, contingencytext, sum(SP*SF)/12, datepart(HH, marketdatetime)+1 
                        //where hours between maxImpactHour+1 and currentHour
                        //where date is today
                        if (VayuConnection.State == ConnectionState.Closed)
                        {
                            VayuConnection.Open();
                        }
                        if (FamilyCheckBox == true)
                        {
                            mGetThisHourRTShadowsFamily.Parameters["@realTimeDate"].Value = currentTime.Date;
                            mGetThisHourRTShadowsFamily.Parameters["@startHour"].Value = maxImpactHour + 1;
                            mGetThisHourRTShadowsFamily.Parameters["@currentHour"].Value = currentHour;
                            mGetThisHourRTShadowsFamily.Parameters["@intervals"].Value = 12;
                            mGetThisHourRTShadowsFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                            Freader = mGetThisHourRTShadowsFamily.ExecuteReader();
                        }
                        else
                        {
                            if (Marketkey == 1)
                            {
                                mGetThisHourRTShadows.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisHourRTShadows.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetThisHourRTShadows.Parameters["@currentHour"].Value = currentHour;
                                mGetThisHourRTShadows.Parameters["@intervals"].Value = 12;
                                Freader = mGetThisHourRTShadows.ExecuteReader();
                            }
                            else
                            {

                                mGetThisErcotHourRTShadows.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisErcotHourRTShadows.Parameters["@startHour"].Value = maxImpactHour;
                                mGetThisErcotHourRTShadows.Parameters["@currentHour"].Value = currentHour;
                                mGetThisErcotHourRTShadows.Parameters["@intervals"].Value = 12;
                                Freader = mGetThisErcotHourRTShadows.ExecuteReader();
                            }
                        }
                        while (Freader.Read())
                        {
                            int hour = Convert.ToInt16(Freader.GetValue(5));
                            string date = Convert.ToString(currentTime.Date);
                            Model.Constraints constraint = new Model.Constraints();
                            int constraintNum = Convert.ToInt32(Freader.GetValue(1));
                            if (constraintHash.ContainsKey(constraintNum))
                            {
                                constraint = constraintHash[constraintNum];
                            }
                            else
                            {
                                constraintHash.Add(constraintNum, constraint);

                                //get from/to zones
                                mToZone = "UNK";
                                mFromZone = "UNK";
                                string test = null;
                                constraintMaxZone.TryGetValue(constraintNum, out test);
                                if (test != null) { }
                                else
                                {
                                    mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                    MySqlDataReader maxReader = mGetMaxNode.ExecuteReader();
                                    while (maxReader.Read())
                                    {
                                        if (maxReader.IsDBNull(0))
                                        {
                                            mMaxNode = 0;
                                        }
                                        else
                                        {
                                            mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                            mToZone = Convert.ToString(maxReader.GetValue(1));
                                            constraintMaxZone.Add(constraintNum, mToZone);
                                        }
                                    }
                                    maxReader.Close();
                                    mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                    MySqlDataReader minReader = mGetMinNode.ExecuteReader();
                                    while (minReader.Read())
                                    {
                                        if (minReader.IsDBNull(0))
                                        {
                                            mMinNode = 0;
                                        }
                                        else
                                        {
                                            mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                            mFromZone = Convert.ToString(minReader.GetValue(1));
                                            constraintMinZone.Add(constraintNum, mFromZone);
                                        }
                                    }
                                    minReader.Close();
                                }
                            }
                            constraint.date = date;
                            constraint.constraintNum = constraintNum;
                            string toZone = null; string fromZone = null;
                            constraintMaxZone.TryGetValue(constraintNum, out toZone);
                            constraintMinZone.TryGetValue(constraintNum, out fromZone);
                            constraint.fromZone = fromZone;
                            constraint.toZone = toZone;
                            constraint.monitoredName = Freader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                            constraint.contingName = Freader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                            if (hour == 1) { constraint.he1Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 2) { constraint.he2Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 3) { constraint.he3Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 4) { constraint.he4Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 5) { constraint.he5Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 6) { constraint.he6Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 7) { constraint.he7Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 8) { constraint.he8Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 9) { constraint.he9Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 10) { constraint.he10Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 11) { constraint.he11Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 12) { constraint.he12Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 13) { constraint.he13Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 14) { constraint.he14Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 15) { constraint.he15Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 16) { constraint.he16Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 17) { constraint.he17Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 18) { constraint.he18Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 19) { constraint.he19Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 20) { constraint.he20Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 21) { constraint.he21Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 22) { constraint.he22Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 23) { constraint.he23Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 24) { constraint.he24Value = Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            constraint.dayofweek = dayofweek;
                        }
                        VayuConnection.Close();
                        Freader.Close();
                    }
                }
                ConstraintsList = constraintHash.Values.ToList<Model.Constraints>();
                reader.Close();
                VayuConnection.Close();
            }

            //RT Shifted no Lookback
            if (RtRadioButton == true && ShiftedRadioButton == true && DateRangeCheckBox == false)
            {
                Dictionary<int, Model.Constraints> constraintHash = new Dictionary<int, Model.Constraints>();
                Dictionary<int, string> constraintMaxZone = new Dictionary<int, string>();
                Dictionary<int, string> constraintMinZone = new Dictionary<int, string>();
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                if (FamilyCheckBox == true)
                {
                    mGetRTShiftedFamily.Parameters["@startDate"].Value = DatePicker1.Date;
                    mGetRTShiftedFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                    reader = mGetRTShiftedFamily.ExecuteReader();
                }
                else
                {
                    if (Marketkey == 1)
                    {
                        mGetRTShifted.Parameters["@startDate"].Value = DatePicker1.Date;
                        reader = mGetRTShifted.ExecuteReader();
                    }
                    else
                    {
                        mGetErcotRTShifted.Parameters["@startDate"].Value = DatePicker1.Date;
                        reader = mGetErcotRTShifted.ExecuteReader();
                    }
                }
                while (reader.Read())
                {
                    int hour = Convert.ToInt16(reader.GetValue(5));
                    string dayofweek = Convert.ToString(DatePicker1.DayOfWeek);
                    string date = Convert.ToString(DatePicker1.Date);
                    Model.Constraints constraint = new Model.Constraints();
                    int constraintNum = Convert.ToInt32(reader.GetValue(1));
                    if (constraintHash.ContainsKey(constraintNum))
                    {
                        constraint = constraintHash[constraintNum];
                    }
                    else
                    {
                        constraintHash.Add(constraintNum, constraint);

                        //get from/to zones
                        mToZone = "UNK";
                        mFromZone = "UNK";
                        string test = null;
                        constraintMaxZone.TryGetValue(constraintNum, out test);
                        if (test != null) { }
                        else
                        {
                            MySqlDataReader maxReader = null;
                            if (Marketkey == 1)
                            {
                                mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                maxReader = mGetMaxNode.ExecuteReader();
                            }
                            else
                            {
                                mGetErcotMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                maxReader = mGetErcotMaxNode.ExecuteReader();
                            }
                            while (maxReader.Read())
                            {
                                if (maxReader.IsDBNull(0))
                                {
                                    mMaxNode = 0;
                                }
                                else
                                {
                                    mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                    mToZone = Convert.ToString(maxReader.GetValue(1));
                                    constraintMaxZone.Add(constraintNum, mToZone);
                                }
                            }
                            maxReader.Close();
                            MySqlDataReader minReader = null;
                            if (Marketkey == 1)
                            {
                                mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                minReader = mGetMinNode.ExecuteReader();
                            }
                            else
                            {
                                mGetErcotMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                minReader = mGetErcotMinNode.ExecuteReader();
                            }
                            while (minReader.Read())
                            {
                                if (minReader.IsDBNull(0))
                                {
                                    mMinNode = 0;
                                }
                                else
                                {
                                    mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                    mFromZone = Convert.ToString(minReader.GetValue(1));
                                    constraintMinZone.Add(constraintNum, mFromZone);
                                }
                            }
                            minReader.Close();
                        }
                    }
                    constraint.date = date;
                    constraint.constraintNum = constraintNum;
                    string toZone = null; string fromZone = null;
                    constraintMaxZone.TryGetValue(constraintNum, out toZone);
                    constraintMinZone.TryGetValue(constraintNum, out fromZone);
                    constraint.fromZone = fromZone;
                    constraint.toZone = toZone;
                    constraint.monitoredName = reader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                    constraint.contingName = reader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                    if (hour == 1) { constraint.he1Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 2) { constraint.he2Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 3) { constraint.he3Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 4) { constraint.he4Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 5) { constraint.he5Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 6) { constraint.he6Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 7) { constraint.he7Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 8) { constraint.he8Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 9) { constraint.he9Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 10) { constraint.he10Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 11) { constraint.he11Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 12) { constraint.he12Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 13) { constraint.he13Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 14) { constraint.he14Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 15) { constraint.he15Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 16) { constraint.he16Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 17) { constraint.he17Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 18) { constraint.he18Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 19) { constraint.he19Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 20) { constraint.he20Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 21) { constraint.he21Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 22) { constraint.he22Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 23) { constraint.he23Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 24) { constraint.he24Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    constraint.dayofweek = dayofweek;
                }
                VayuConnection.Close();
                DateTime currentTime = DateTime.Now;
                //if today is the date user selected we need to deal with current hour impact estimate
                if (currentTime.Date == DatePicker1.Date)
                {
                    int currentHour = Convert.ToInt32(currentTime.ToString("HH")) + 1;
                    //make it hour ending by adding 1
                    currentHour = currentHour + 1;
                    int maxImpactHour = 0;
                    int currentMinute = Convert.ToInt32(currentTime.ToString("mm"));
                    double currentElapsedInterval = 0;
                    if (VayuConnection.State == ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    if (FamilyCheckBox == true)
                    {
                        mGetMaxImpactHourTodayFamily.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                        mGetMaxImpactHourTodayFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                        Ireader = mGetMaxImpactHourTodayFamily.ExecuteReader();
                    }
                    else
                    {
                        if (Marketkey == 1)
                        {
                            mGetMaxImpactHourToday.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                            Ireader = mGetMaxImpactHourToday.ExecuteReader();
                        }
                        else
                        {
                            mGetErcotMaxImpactHourToday.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                            Ireader = mGetErcotMaxImpactHourToday.ExecuteReader();
                        }
                    }
                    while (Ireader.Read())
                    {
                        if (DBNull.Value != Ireader.GetValue(0))
                        {
                            maxImpactHour = Convert.ToInt32(Ireader.GetValue(0));
                        }
                    }
                    Ireader.Close();
                    VayuConnection.Close();
                    //when time == -1 we run the query once -- one intervals value
                    if (maxImpactHour - currentHour == -1) //we need to divide sum(SP)*SF by intervals elapsed
                    {
                        currentElapsedInterval = currentMinute / 5;
                        currentElapsedInterval = Math.Floor(currentElapsedInterval);
                        if (VayuConnection.State == ConnectionState.Closed)
                        {
                            VayuConnection.Open();
                        }
                        if (FamilyCheckBox == true)
                        {
                            mGetThisHourRTShiftedFamily.Parameters["@realTimeDate"].Value = currentTime.Date;
                            mGetThisHourRTShiftedFamily.Parameters["@startHour"].Value = maxImpactHour + 1;
                            mGetThisHourRTShiftedFamily.Parameters["@currentHour"].Value = currentHour;
                            mGetThisHourRTShiftedFamily.Parameters["@intervals"].Value = currentElapsedInterval;
                            mGetThisHourRTShiftedFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                            Jreader = mGetThisHourRTShiftedFamily.ExecuteReader();
                        }
                        else
                        {
                            mGetThisHourRTShifted.Parameters["@realTimeDate"].Value = currentTime.Date;
                            mGetThisHourRTShifted.Parameters["@startHour"].Value = maxImpactHour + 1;
                            mGetThisHourRTShifted.Parameters["@currentHour"].Value = currentHour;
                            mGetThisHourRTShifted.Parameters["@intervals"].Value = currentElapsedInterval;
                            Jreader = mGetThisHourRTShifted.ExecuteReader();
                        }
                        while (Jreader.Read())
                        {
                            int hour = Convert.ToInt16(Jreader.GetValue(5));
                            string date = Convert.ToString(currentTime.Date);
                            Model.Constraints constraint = new Model.Constraints();
                            int constraintNum = Convert.ToInt32(Jreader.GetValue(1));
                            if (constraintHash.ContainsKey(constraintNum))
                            {
                                constraint = constraintHash[constraintNum];
                            }
                            else
                            {
                                constraintHash.Add(constraintNum, constraint);

                                //get from/to zones
                                mToZone = "UNK";
                                mFromZone = "UNK";
                                string test = null;
                                constraintMaxZone.TryGetValue(constraintNum, out test);
                                if (test != null) { }
                                else
                                {
                                    mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                    MySqlDataReader maxReader = mGetMaxNode.ExecuteReader();
                                    while (maxReader.Read())
                                    {
                                        if (maxReader.IsDBNull(0))
                                        {
                                            mMaxNode = 0;
                                        }
                                        else
                                        {
                                            mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                            mToZone = Convert.ToString(maxReader.GetValue(1));
                                            constraintMaxZone.Add(constraintNum, mToZone);
                                        }
                                    }
                                    maxReader.Close();
                                    mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                    MySqlDataReader minReader = mGetMinNode.ExecuteReader();
                                    while (minReader.Read())
                                    {
                                        if (minReader.IsDBNull(0))
                                        {
                                            mMinNode = 0;
                                        }
                                        else
                                        {
                                            mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                            mFromZone = Convert.ToString(minReader.GetValue(1));
                                            constraintMinZone.Add(constraintNum, mFromZone);
                                        }
                                    }
                                    minReader.Close();
                                }
                            }
                            constraint.date = date;
                            constraint.constraintNum = constraintNum;
                            string toZone = null; string fromZone = null;
                            constraintMaxZone.TryGetValue(constraintNum, out toZone);
                            constraintMinZone.TryGetValue(constraintNum, out fromZone);
                            constraint.fromZone = fromZone;
                            constraint.toZone = toZone;
                            constraint.monitoredName = Jreader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                            constraint.contingName = Jreader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                            if (hour == 1) { constraint.he1Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 2) { constraint.he2Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 3) { constraint.he3Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 4) { constraint.he4Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 5) { constraint.he5Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 6) { constraint.he6Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 7) { constraint.he7Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 8) { constraint.he8Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 9) { constraint.he9Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 10) { constraint.he10Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 11) { constraint.he11Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 12) { constraint.he12Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 13) { constraint.he13Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 14) { constraint.he14Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 15) { constraint.he15Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 16) { constraint.he16Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 17) { constraint.he17Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 18) { constraint.he18Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 19) { constraint.he19Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 20) { constraint.he20Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 21) { constraint.he21Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 22) { constraint.he22Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 23) { constraint.he23Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            if (hour == 24) { constraint.he24Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 0)); }
                            constraint.dayofweek = Convert.ToString(currentTime.DayOfWeek);
                        }
                        VayuConnection.Close();
                        Jreader.Close();
                    }
                    //most recent constraint scrape as compared to maximum hour
                    //when time is less than -1 we run the query twice -- two intervals values
                    if (maxImpactHour - currentHour < -1) // we need to divide sum(SP*SF) by 12 and just assume we scraped all intervals
                    {
                        if (VayuConnection.State == ConnectionState.Closed)
                        {
                            VayuConnection.Open();
                        }
                        //select shift factor, constraintrtnum, monitoredtext, contingencytext, sum(SP*SF)/12, datepart(HH, marketdatetime)+1 
                        //where hours between maxImpactHour+1 and currentHour
                        //where date is today
                        if (FamilyCheckBox == true)
                        {
                            mGetThisHourRTShiftedFamily.Parameters["@realTimeDate"].Value = currentTime.Date;
                            mGetThisHourRTShiftedFamily.Parameters["@startHour"].Value = maxImpactHour + 1;
                            mGetThisHourRTShiftedFamily.Parameters["@currentHour"].Value = currentHour;
                            mGetThisHourRTShiftedFamily.Parameters["@intervals"].Value = 12;
                            mGetThisHourRTShiftedFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                            Freader = mGetThisHourRTShiftedFamily.ExecuteReader();
                        }
                        else
                        {
                            if (Marketkey == 1)
                            {
                                mGetThisHourRTShifted.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisHourRTShifted.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetThisHourRTShifted.Parameters["@currentHour"].Value = currentHour;
                                mGetThisHourRTShifted.Parameters["@intervals"].Value = 12;
                                Freader = mGetThisHourRTShifted.ExecuteReader();
                            }
                            else
                            {
                                mGetThisErcotHourRTShifted.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisErcotHourRTShifted.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetThisErcotHourRTShifted.Parameters["@currentHour"].Value = currentHour;
                                mGetThisErcotHourRTShifted.Parameters["@intervals"].Value = 12;
                                Freader = mGetThisErcotHourRTShifted.ExecuteReader();
                            }
                        }
                        while (Freader.Read())
                        {
                            int hour = Convert.ToInt16(Freader.GetValue(5));
                            string date = Convert.ToString(currentTime.Date);
                            Model.Constraints constraint = new Model.Constraints();
                            int constraintNum = Convert.ToInt32(Freader.GetValue(1));
                            if (constraintHash.ContainsKey(constraintNum))
                            {
                                constraint = constraintHash[constraintNum];
                            }
                            else
                            {
                                constraintHash.Add(constraintNum, constraint);

                                //get from/to zones
                                mToZone = "UNK";
                                mFromZone = "UNK";
                                string test = null;
                                constraintMaxZone.TryGetValue(constraintNum, out test);
                                if (test != null) { }
                                else
                                {
                                    MySqlDataReader maxReader = null;
                                    if (Marketkey == 1)
                                    {
                                        mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                        maxReader = mGetMaxNode.ExecuteReader();
                                    }
                                    else
                                    {
                                        mGetErcotMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                        maxReader = mGetErcotMaxNode.ExecuteReader();
                                    }
                                    while (maxReader.Read())
                                    {
                                        if (maxReader.IsDBNull(0))
                                        {
                                            mMaxNode = 0;
                                        }
                                        else
                                        {
                                            mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                            mToZone = Convert.ToString(maxReader.GetValue(1));
                                            constraintMaxZone.Add(constraintNum, mToZone);
                                        }
                                    }
                                    maxReader.Close();
                                    MySqlDataReader minReader = null;
                                    if (Marketkey == 1)
                                    {
                                        mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                        minReader = mGetMinNode.ExecuteReader();
                                    }
                                    else
                                    {
                                        mGetErcotMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                        minReader = mGetErcotMinNode.ExecuteReader();
                                    }
                                    while (minReader.Read())
                                    {
                                        if (minReader.IsDBNull(0))
                                        {
                                            mMinNode = 0;
                                        }
                                        else
                                        {
                                            mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                            mFromZone = Convert.ToString(minReader.GetValue(1));
                                            constraintMinZone.Add(constraintNum, mFromZone);
                                        }
                                    }
                                    minReader.Close();
                                }
                            }
                            constraint.date = date;
                            constraint.constraintNum = constraintNum;
                            string toZone = null; string fromZone = null;
                            constraintMaxZone.TryGetValue(constraintNum, out toZone);
                            constraintMinZone.TryGetValue(constraintNum, out fromZone);
                            constraint.fromZone = fromZone;
                            constraint.toZone = toZone;
                            constraint.monitoredName = Freader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                            constraint.contingName = Freader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                            if (hour == 1) { constraint.he1Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 2) { constraint.he2Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 3) { constraint.he3Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 4) { constraint.he4Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 5) { constraint.he5Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 6) { constraint.he6Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 7) { constraint.he7Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 8) { constraint.he8Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 9) { constraint.he9Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 10) { constraint.he10Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 11) { constraint.he11Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 12) { constraint.he12Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 13) { constraint.he13Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 14) { constraint.he14Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 15) { constraint.he15Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 16) { constraint.he16Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 17) { constraint.he17Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 18) { constraint.he18Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 19) { constraint.he19Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 20) { constraint.he20Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 21) { constraint.he21Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 22) { constraint.he22Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 23) { constraint.he23Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            if (hour == 24) { constraint.he24Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 0)); }
                            constraint.dayofweek = Convert.ToString(DatePicker1.DayOfWeek);
                        }
                        VayuConnection.Close();
                        Freader.Close();
                    }
                }
                ConstraintsList = constraintHash.Values.ToList<Model.Constraints>();
                reader.Close();
                VayuConnection.Close();
            }

            //DA Shadow on Lookback
            if (DaRadioButton == true && ShadowRadioButton == true && DateRangeCheckBox == true)
            {
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                mGetDates.Parameters["@startDate"].Value = DatePicker1.Date;
                mGetDates.Parameters["@endDate"].Value = DatePicker2.Date;
                Dictionary<string, Model.Constraints> constraintHash = new Dictionary<string, Model.Constraints>();
                Dictionary<int, string> constraintMaxZone = new Dictionary<int, string>();
                Dictionary<int, string> constraintMinZone = new Dictionary<int, string>();
                MySqlDataReader Datereader = mGetDates.ExecuteReader();
                while (Datereader.Read())
                {
                    string date = Convert.ToString(Datereader.GetValue(0));
                    DateTime retrieveDate = Convert.ToDateTime(Datereader.GetValue(0));
                    mGetDAShadows.Parameters["@startDate"].Value = date;
                    MySqlDataReader reader = mGetDAShadows.ExecuteReader();
                    while (reader.Read())
                    {
                        int hour = Convert.ToInt16(reader.GetValue(5));
                        Model.Constraints constraint = new Model.Constraints();
                        int constraintNum = Convert.ToInt32(reader.GetValue(1));
                        string monitoredName = reader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                        string contingName = reader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                        string key = monitoredName + contingName;

                        if (constraintHash.ContainsKey(key + "@" + date))
                        {
                            constraint = constraintHash[key + "@" + date];
                        }
                        else
                        {
                            constraintHash.Add(key + "@" + date, constraint);
                        }
                        constraint.date = date;
                        constraint.constraintNum = constraintNum;
                        constraint.monitoredName = reader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                        constraint.contingName = reader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                        if (hour == 1) { constraint.he1Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 2) { constraint.he2Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 3) { constraint.he3Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 4) { constraint.he4Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 5) { constraint.he5Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 6) { constraint.he6Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 7) { constraint.he7Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 8) { constraint.he8Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 9) { constraint.he9Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 10) { constraint.he10Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 11) { constraint.he11Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 12) { constraint.he12Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 13) { constraint.he13Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 14) { constraint.he14Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 15) { constraint.he15Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 16) { constraint.he16Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 17) { constraint.he17Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 18) { constraint.he18Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 19) { constraint.he19Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 20) { constraint.he20Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 21) { constraint.he21Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 22) { constraint.he22Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 23) { constraint.he23Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        if (hour == 24) { constraint.he24Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                        constraint.dayofweek = Convert.ToString(retrieveDate.DayOfWeek);
                    }
                    reader.Close();
                }
                ConstraintsList = constraintHash.Values.ToList<Model.Constraints>();
                Datereader.Close();
                VayuConnection.Close();
            }

            //DA Shadow no Lookback
            if (DaRadioButton == true && ShadowRadioButton == true && DateRangeCheckBox == false)
            {
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }

                Dictionary<string, Model.Constraints> constraintHash = new Dictionary<string, Model.Constraints>();
                Dictionary<int, string> constraintMaxZone = new Dictionary<int, string>();
                Dictionary<int, string> constraintMinZone = new Dictionary<int, string>();
                string date = Convert.ToString(DatePicker1.Date);
                MySqlDataReader reader = null;
                if (Marketkey == 1)
                {
                    mGetDAShadows.Parameters["@startDate"].Value = date;
                    reader = mGetDAShadows.ExecuteReader();
                }
                else
                {
                    mGetErcotDAShadows.Parameters["@startDate"].Value = date;
                    reader = mGetErcotDAShadows.ExecuteReader();
                }
                while (reader.Read())
                {
                    int hour = Convert.ToInt16(reader.GetValue(5));
                    Model.Constraints constraint = new Model.Constraints();
                    int constraintNum = Convert.ToInt32(reader.GetValue(1));
                    string monitoredName = reader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                    string contingName = reader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                    string key = monitoredName + contingName;

                    if (constraintHash.ContainsKey(key + "@" + date))
                    {
                        constraint = constraintHash[key + "@" + date];
                    }
                    else
                    {
                        constraintHash.Add(key + "@" + date, constraint);
                    }
                    constraint.date = date;
                    constraint.constraintNum = constraintNum;
                    constraint.monitoredName = reader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                    constraint.contingName = reader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                    if (hour == 1) { constraint.he1Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 2) { constraint.he2Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 3) { constraint.he3Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 4) { constraint.he4Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 5) { constraint.he5Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 6) { constraint.he6Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 7) { constraint.he7Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 8) { constraint.he8Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 9) { constraint.he9Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 10) { constraint.he10Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 11) { constraint.he11Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 12) { constraint.he12Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 13) { constraint.he13Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 14) { constraint.he14Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 15) { constraint.he15Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 16) { constraint.he16Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 17) { constraint.he17Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 18) { constraint.he18Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 19) { constraint.he19Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 20) { constraint.he20Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 21) { constraint.he21Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 22) { constraint.he22Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 23) { constraint.he23Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    if (hour == 24) { constraint.he24Value = Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 0)); }
                    constraint.dayofweek = Convert.ToString(DatePicker1.DayOfWeek);
                }
                reader.Close();
                ConstraintsList = constraintHash.Values.ToList<Model.Constraints>();
                VayuConnection.Close();
            }
            callback(ConstraintsList, null);
            return ConstraintsList;
        }

        #region Public Methods

        /// <summary>
        /// Gets the exposure node minimum details.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="constraint">The constraint.</param>
        public void GetExposureNodeMinDetails(Action<List<SourceSinkData>, Exception> callback, Model.Constraints constraint)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            List<SourceSinkData> sourceSinkNodeList = new List<SourceSinkData>();

            mSelectBestMinNodesCommand.Parameters["@constraintNum"].Value = constraint.constraintNum;
            MySqlDataReader mreader = mSelectBestMinNodesCommand.ExecuteReader();
            while (mreader.Read())
            {
                mBestNodeKey = Convert.ToInt32(mreader.GetValue(0));
            }
            mreader.Close();

            mSelectNodeDetailsCommand.Parameters["@NodeKey"].Value = mBestNodeKey;
            MySqlDataReader Zreader = mSelectNodeDetailsCommand.ExecuteReader();
            SourceSinkData sourceSink = new SourceSinkData();
            sourceSink.Source = new PricingNode();
            if (Zreader.Read())
            {
                if (Zreader[2] != null)
                {
                    sourceSink.Source.ExternalNodeId = int.Parse(Zreader[2].ToString());
                }
                sourceSink.Source.NodeTypeKey = int.Parse(Zreader[3].ToString());
                sourceSink.Source.NodeName = Convert.ToString(Zreader.GetValue(1));
                sourceSink.Source.Zone = Convert.ToString(Zreader.GetValue(4));
            }
            else
            {
                sourceSink.Source.ExternalNodeId = 0;
                sourceSink.Source.NodeTypeKey = 1;
            }
            sourceSink.Source.MarketKey = 1;
            sourceSink.Source.NodeKey = mBestNodeKey;

            sourceSinkNodeList.Add(sourceSink);
            Zreader.Close();
            VayuConnection.Close();

            callback(sourceSinkNodeList, null);
        }
        /// <summary>
        /// Gets the exposure node maximum details.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="constraint">The constraint.</param>
        public void GetExposureNodeMaxDetails(Action<List<SourceSinkData>, Exception> callback, Model.Constraints constraint)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            List<SourceSinkData> sourceSinkNodeList = new List<SourceSinkData>();

            mSelectBestMaxNodesCommand.Parameters["@constraintNum"].Value = constraint.constraintNum;
            MySqlDataReader mreader = mSelectBestMaxNodesCommand.ExecuteReader();
            while (mreader.Read())
            {
                mBestNodeKey = Convert.ToInt32(mreader.GetValue(0));
            }
            mreader.Close();

            mSelectNodeDetailsCommand.Parameters["@NodeKey"].Value = mBestNodeKey;
            MySqlDataReader Zreader = mSelectNodeDetailsCommand.ExecuteReader();
            SourceSinkData sourceSink = new SourceSinkData();
            sourceSink.Sink = new PricingNode();
            if (Zreader.Read())
            {
                if (Zreader[2] != null)
                {
                    sourceSink.Sink.ExternalNodeId = int.Parse(Zreader[2].ToString());
                }
                sourceSink.Sink.NodeTypeKey = int.Parse(Zreader[3].ToString());
                sourceSink.Sink.NodeName = Convert.ToString(Zreader.GetValue(1));
                sourceSink.Sink.Zone = Convert.ToString(Zreader.GetValue(4));
            }
            else
            {
                sourceSink.Sink.ExternalNodeId = 0;
                sourceSink.Sink.NodeTypeKey = 1;
            }
            sourceSink.Sink.MarketKey = 1;
            sourceSink.Sink.NodeKey = mBestNodeKey;

            sourceSinkNodeList.Add(sourceSink);
            Zreader.Close();
            VayuConnection.Close();

            callback(sourceSinkNodeList, null);
        }
        /// <summary>
        /// Gets the exposure for Uptos minimum details.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="constraint">The constraint.</param>
        public void GetExposureUpMinDetails(Action<List<SourceSinkData>, Exception> callback, Model.Constraints constraint)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            List<SourceSinkData> sourceSinkNodeList = new List<SourceSinkData>();

            mSelectBestUpMinCommand.Parameters["@constraintNum"].Value = constraint.constraintNum;
            MySqlDataReader mreader = mSelectBestUpMinCommand.ExecuteReader();
            while (mreader.Read())
            {
                mBestNodeKey = Convert.ToInt32(mreader.GetValue(0));
            }
            mreader.Close();

            mSelectNodeDetailsCommand.Parameters["@NodeKey"].Value = mBestNodeKey;
            MySqlDataReader Zreader = mSelectNodeDetailsCommand.ExecuteReader();
            SourceSinkData sourceSink = new SourceSinkData();
            sourceSink.Source = new PricingNode();
            if (Zreader.Read())
            {
                if (Zreader[2] != null)
                {
                    sourceSink.Source.ExternalNodeId = int.Parse(Zreader[2].ToString());
                }
                sourceSink.Source.NodeTypeKey = int.Parse(Zreader[3].ToString());
                sourceSink.Source.NodeName = Convert.ToString(Zreader.GetValue(1));
                sourceSink.Source.Zone = Convert.ToString(Zreader.GetValue(4));
            }
            else
            {
                sourceSink.Source.ExternalNodeId = 0;
                sourceSink.Source.NodeTypeKey = 1;
            }
            sourceSink.Source.MarketKey = 1;
            sourceSink.Source.NodeKey = mBestNodeKey;

            sourceSinkNodeList.Add(sourceSink);
            Zreader.Close();
            VayuConnection.Close();

            callback(sourceSinkNodeList, null);
        }
        /// <summary>
        /// Gets the exposure for Uptos maximum details.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="constraint">The constraint.</param>
        public void GetExposureUpMaxDetails(Action<List<SourceSinkData>, Exception> callback, Model.Constraints constraint)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            List<SourceSinkData> sourceSinkNodeList = new List<SourceSinkData>();

            mSelectBestUpMaxCommand.Parameters["@constraintNum"].Value = constraint.constraintNum;
            MySqlDataReader mreader = mSelectBestUpMaxCommand.ExecuteReader();
            while (mreader.Read())
            {
                mBestNodeKey = Convert.ToInt32(mreader.GetValue(0));
            }
            mreader.Close();

            mSelectNodeDetailsCommand.Parameters["@NodeKey"].Value = mBestNodeKey;
            MySqlDataReader Zreader = mSelectNodeDetailsCommand.ExecuteReader();
            SourceSinkData sourceSink = new SourceSinkData();
            sourceSink.Sink = new PricingNode();
            if (Zreader.Read())
            {
                if (Zreader[2] != null)
                {
                    sourceSink.Sink.ExternalNodeId = int.Parse(Zreader[2].ToString());
                }
                sourceSink.Sink.NodeTypeKey = int.Parse(Zreader[3].ToString());
                sourceSink.Sink.NodeName = Convert.ToString(Zreader.GetValue(1));
                sourceSink.Sink.Zone = Convert.ToString(Zreader.GetValue(4));
            }
            else
            {
                sourceSink.Sink.ExternalNodeId = 0;
                sourceSink.Sink.NodeTypeKey = 1;
            }
            sourceSink.Sink.MarketKey = 1;
            sourceSink.Sink.NodeKey = mBestNodeKey;

            sourceSinkNodeList.Add(sourceSink);
            Zreader.Close();
            VayuConnection.Close();

            callback(sourceSinkNodeList, null);
        }
        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //myqueries
            mGetDates = new MySqlCommand();
            mGetDates.CommandText = "SELECT distinct convert(date, MarketDateTime) FROM Vayu..ConstraintRT " +
            "WHERE convert(date, MarketDateTime) between @startDate and @endDate " +
            "ORDER BY convert(date, MarketDateTime) asc";
            mGetDates.Parameters.AddWithValue("@startDate", "startDate");
            mGetDates.Parameters.AddWithValue("@endDate", "endDate");
            mGetDates.Connection = VayuConnection;
            //
            mGetErcotDates = new MySqlCommand();
            mGetErcotDates.CommandText = "SELECT distinct convert(date, MarketDateTime) FROM Vayu..ConstraintRT WHERE convert(date, MarketDateTime) " +
                                         " between @startDate and @endDate ORDER BY convert(date, MarketDateTime) asc";
            mGetErcotDates.Parameters.AddWithValue("@startDate", "startDate");
            mGetErcotDates.Parameters.AddWithValue("@endDate", "endDate");
            mGetErcotDates.Connection = VayuConnection;

            mGetHourlyRTImpact = new MySqlCommand();
            mGetHourlyRTImpact.CommandText = "SELECT B.shiftFactor, B.ConstraintRTNum, MonitoredText, ContingencyText, Impact/12, hour " +
            "FROM RTImpact as A inner join RTMasterConstraint as B " +
            "ON A.ConstraintRTNum = B.ConstraintRTNum WHERE date = @startDate " +
            "ORDER BY date asc";
            mGetHourlyRTImpact.Parameters.AddWithValue("@startDate", "startDate");
            mGetHourlyRTImpact.Connection = VayuConnection;

            //
            mGetErcotHourlyRTImpact = new MySqlCommand();
            mGetErcotHourlyRTImpact.CommandText = "SELECT B.shiftFactor, B.ConstraintRTNum, MonitoredText, ContingencyText, Impact/12, hour FROM Vayu..RTImpact as A  " +
                                                 " inner join Vayu..RTMasterConstraint as B ON A.ConstraintRTNum = B.ConstraintRTNum WHERE date = @startDate ORDER BY date asc";
            mGetErcotHourlyRTImpact.Parameters.AddWithValue("@startDate", "startDate");
            mGetErcotHourlyRTImpact.Connection = VayuConnection;

            mGetThisHourRTImpact = new MySqlCommand();
            mGetThisHourRTImpact.CommandText = "SELECT convert(date, A.MarketDateTime), B.ConstraintRTNum, MonitoredText, B.ContingencyText, " +
            "(SUM(A.shadowprice)*B.shiftfactor)/@intervals, DATEPART(HH, A.marketdatetime)+1 " +
            "FROM Vayu..ConstraintRT as A inner join RTMasterConstraint as B " +
            "ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText " +
            "WHERE convert(date, A.MarketDateTime) = @realTimeDate and DATEPART(HH, A.MarketDateTime)+1 between @startHour and @currentHour " +
            "GROUP BY B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime), convert(date, A.MarketDateTime) " +
            "ORDER BY convert(date, A.MarketDateTime) asc";
            mGetThisHourRTImpact.Parameters.AddWithValue("@intervals", "intervals");
            mGetThisHourRTImpact.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mGetThisHourRTImpact.Parameters.AddWithValue("@startHour", "startHour");
            mGetThisHourRTImpact.Parameters.AddWithValue("@currentHour", "currentHour");
            mGetThisHourRTImpact.Connection = VayuConnection;

            //
            mGetErcotThisHourRTImpact = new MySqlCommand();
            mGetErcotThisHourRTImpact.CommandText = "SELECT convert(date, A.MarketDateTime), B.ConstraintRTNum, MonitoredText, B.ContingencyText,  " +
                                                    " (SUM(A.shadowprice)*B.shiftfactor)/12, DATEPART(HH, A.marketdatetime)+1  " +
                                                    " FROM Vayu..ConstraintRT as A inner join Vayu..RTMasterConstraint as B  " +
                                                    " ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText  " +
                                                    " WHERE convert(date, A.MarketDateTime) >=  @realTimeDate and DATEPART(HH, A.MarketDateTime)+1 between @startHour and @currentHour  " +
                                                    " GROUP BY B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime), convert(date, A.MarketDateTime)  " +
                                                    " ORDER BY convert(date, A.MarketDateTime) asc ";
            mGetErcotThisHourRTImpact.Parameters.AddWithValue("@intervals", "intervals");
            mGetErcotThisHourRTImpact.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mGetErcotThisHourRTImpact.Parameters.AddWithValue("@startHour", "startHour");
            mGetErcotThisHourRTImpact.Parameters.AddWithValue("@currentHour", "currentHour");
            mGetErcotThisHourRTImpact.Connection = VayuConnection;

            mGetMaxImpactHourToday = new MySqlCommand();
            mGetMaxImpactHourToday.CommandText = "SELECT max(hour) from RTImpact where date = @mostRecentImpactDate";
            mGetMaxImpactHourToday.Parameters.AddWithValue("@mostRecentImpactDate", "mostRecentImpactDate");
            mGetMaxImpactHourToday.Connection = VayuConnection;

            //
            mGetErcotMaxImpactHourToday = new MySqlCommand();
            mGetErcotMaxImpactHourToday.CommandText = "SELECT MAX(Hour) from Vayu..RTImpact where date =@mostRecentImpactDate";
            mGetErcotMaxImpactHourToday.Parameters.AddWithValue("@mostRecentImpactDate", "mostRecentImpactDate");
            mGetErcotMaxImpactHourToday.Connection = VayuConnection;

            //This wont pull constraints with no Sensitivity
            mGetRTShadows = new MySqlCommand();
            mGetRTShadows.CommandText = "SELECT B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, sum(abs(A.ShadowPrice))/12, DATEPART(HH, A.MarketDateTime)+1 " +
            "FROM Vayu..ConstraintRT as A inner join RTMasterConstraint as B " +
            "ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText " +
            "WHERE convert(date, A.MarketDateTime) = @startDate " +
            "GROUP BY B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime)+1 ";
            mGetRTShadows.Parameters.AddWithValue("@startDate", "startDate");
            mGetRTShadows.Connection = VayuConnection;
            //
            mGetErcotRTShadows = new MySqlCommand();
            mGetErcotRTShadows.CommandText = "SELECT B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, sum(abs(A.ShadowPrice))/12, DATEPART(HH, A.MarketDateTime)+1  " +
                                           " FROM Vayu..ConstraintRT as A inner join Vayu..RTMasterConstraint as B ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText " +
                                           " WHERE convert(date, A.MarketDateTime) =@startDate GROUP BY B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime)+1 ";
            mGetErcotRTShadows.Parameters.AddWithValue("@startDate", "startDate");
            mGetErcotRTShadows.Connection = VayuConnection;

            //1 is placeholder for shift factor and 0 is placeholder for constraintnum
            mGetDAShadows = new MySqlCommand();
            mGetDAShadows.CommandText = "SELECT 1, 0, ConstraintText as ConstraintName, ContingencyText as ContingencyName, ShadowPrice as MarginalValue, DATEPART(HH, MarketDateTime)+1 " +
            " FROM Vayu..ConstraintDA WHERE convert(date, MarketDateTime) = @startDate GROUP BY ConstraintText, ContingencyText, ShadowPrice, DATEPART(HH, MarketDateTime)+1 ";
            mGetDAShadows.Parameters.AddWithValue("@startDate", "startDate");
            mGetDAShadows.Connection = VayuConnection;

            //
            mGetErcotDAShadows = new MySqlCommand();
            mGetErcotDAShadows.CommandText = "SELECT 1, 0, ConstraintText as ConstraintName, ContingencyText as ContingencyName, ShadowPrice as MarginalValue, DATEPART(HH, MarketDateTime)+1  FROM Vayu..ConstraintDA  " +
                                            " WHERE convert(date, MarketDateTime) =@startDate GROUP BY ConstraintText, ContingencyText, ShadowPrice, DATEPART(HH, MarketDateTime)+1 ";
            mGetErcotDAShadows.Parameters.AddWithValue("@startDate", "startDate");
            mGetErcotDAShadows.Connection = VayuConnection;

            mGetThisHourRTShadows = new MySqlCommand();
            mGetThisHourRTShadows.CommandText = "SELECT B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, sum(abs(A.ShadowPrice))/@intervals, DATEPART(HH, A.MarketDateTime)+1 " +
            "FROM Vayu..ConstraintRT as A inner join RTMasterConstraint as B " +
            "ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText " +
            "WHERE convert(date, A.MarketDateTime) = @realTimeDate and DATEPART(HH, A.MarketDateTime)+1 between @startHour and @currentHour " +
            "GROUP BY B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime)+1 ";
            mGetThisHourRTShadows.Parameters.AddWithValue("@intervals", "intervals");
            mGetThisHourRTShadows.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mGetThisHourRTShadows.Parameters.AddWithValue("@startHour", "startHour");
            mGetThisHourRTShadows.Parameters.AddWithValue("@currentHour", "currentHour");
            mGetThisHourRTShadows.Connection = VayuConnection;
            //
            mGetThisErcotHourRTShadows = new MySqlCommand();
            mGetThisErcotHourRTShadows.CommandText = "SELECT B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, sum(abs(A.ShadowPrice))/@intervals, DATEPART(HH, A.MarketDateTime)+1 " +
                                                    " FROM Vayu..ConstraintRT as A inner join Vayu..RTMasterConstraint as B ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText  " +
                                                    " WHERE convert(date, A.MarketDateTime) = @realTimeDate and DATEPART(HH, A.MarketDateTime)+1 between @startHour and @currentHour " +
                                                    " GROUP BY B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime)+1";
            mGetThisErcotHourRTShadows.Parameters.AddWithValue("@intervals", "intervals");
            mGetThisErcotHourRTShadows.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mGetThisErcotHourRTShadows.Parameters.AddWithValue("@startHour", "startHour");
            mGetThisErcotHourRTShadows.Parameters.AddWithValue("@currentHour", "currentHour");
            mGetThisErcotHourRTShadows.Connection = VayuConnection;


            //This wont pull constraints with no Sensitivity
            mGetRTShifted = new MySqlCommand();
            mGetRTShifted.CommandText = "SELECT B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, (sum(abs(A.ShadowPrice))*B.ShiftFactor)/12, DATEPART(HH, A.MarketDateTime)+1 " +
            "FROM Vayu..ConstraintRT as A inner join RTMasterConstraint as B " +
            "ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText " +
            "WHERE convert(date, A.MarketDateTime) = @startDate " +
            "GROUP BY B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime)+1 ";
            mGetRTShifted.Parameters.AddWithValue("@startDate", "startDate");
            mGetRTShifted.Connection = VayuConnection;

            //
            mGetErcotRTShifted = new MySqlCommand();
            mGetErcotRTShifted.CommandText = "SELECT B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, (sum(abs(A.ShadowPrice))*B.ShiftFactor)/12, DATEPART(HH, A.MarketDateTime)+1  " +
                                            " FROM Vayu..ConstraintRT as A inner join Vayu..RTMasterConstraint as B  ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText  " +
                                            " WHERE convert(date, A.MarketDateTime) =@startDate GROUP BY B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime)+1";
            mGetErcotRTShifted.Parameters.AddWithValue("@startDate", "startDate");
            mGetErcotRTShifted.Connection = VayuConnection;

            mGetThisHourRTShifted = new MySqlCommand();
            mGetThisHourRTShifted.CommandText = "SELECT B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, (sum(abs(A.ShadowPrice))*B.ShiftFactor)/@intervals, DATEPART(HH, A.MarketDateTime)+1 " +
            "FROM Vayu..ConstraintRT as A inner join RTMasterConstraint as B " +
            "ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText " +
            "WHERE convert(date, A.MarketDateTime) = @realTimeDate and DATEPART(HH, A.MarketDateTime)+1 between @startHour and @currentHour " +
            "GROUP BY B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime)+1 ";
            mGetThisHourRTShifted.Parameters.AddWithValue("@intervals", "intervals");
            mGetThisHourRTShifted.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mGetThisHourRTShifted.Parameters.AddWithValue("@startHour", "startHour");
            mGetThisHourRTShifted.Parameters.AddWithValue("@currentHour", "currentHour");
            mGetThisHourRTShifted.Connection = VayuConnection;

            //
            mGetThisErcotHourRTShifted = new MySqlCommand();
            mGetThisErcotHourRTShifted.CommandText = "SELECT B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, (sum(abs(A.ShadowPrice))*B.ShiftFactor)/@intervals, DATEPART(HH, A.MarketDateTime)+1  " +
                                                   " FROM Vayu..ConstraintRT as A inner join Vayu..RTMasterConstraint as B ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText " +
                                                   " WHERE convert(date, A.MarketDateTime) =@realTimeDate  and DATEPART(HH, A.MarketDateTime)+1 between  @startHour and @currentHour " +
                                                   " GROUP BY B.ShiftFactor, B.ConstraintRTNum, MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime)+1 ";
            mGetThisErcotHourRTShifted.Parameters.AddWithValue("@intervals", "intervals");
            mGetThisErcotHourRTShifted.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mGetThisErcotHourRTShifted.Parameters.AddWithValue("@startHour", "startHour");
            mGetThisErcotHourRTShifted.Parameters.AddWithValue("@currentHour", "currentHour");
            mGetThisErcotHourRTShifted.Connection = VayuConnection;

            mGetNoSensitivityConstraints = new MySqlCommand();
            mGetNoSensitivityConstraints.CommandText = "SELECT distinct B.shiftfactor, B.ConstraintRTNum, A.ConstraintText, A.ContingencyText, 'X', datepart(HH, A.MarketDateTime)+1, convert(date, A.MarketDateTime) " +
            "FROM Vayu..ConstraintRT as A LEFT JOIN RTMasterConstraint as B ON A.ContingencyText = B.ContingencyText and B.MonitoredText = A.ConstraintText " +
            "WHERE CONVERT(date, A.marketdatetime) = @realTimeDate and DATEPART(HH, A.MarketDateTime)+1 between @startHour and @currentHour and A.ConstraintText != 'None' and B.shiftfactor is null";
            mGetNoSensitivityConstraints.Parameters.AddWithValue("@startHour", "startHour");
            mGetNoSensitivityConstraints.Parameters.AddWithValue("@currentHour", "currentHour");
            mGetNoSensitivityConstraints.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mGetNoSensitivityConstraints.Parameters.AddWithValue("@startDate", "startDate");
            mGetNoSensitivityConstraints.Connection = VayuConnection;

            mSelectBestMinNodesCommand = new MySqlCommand();
            mSelectBestMinNodesCommand.CommandText = "SELECT top 1 NodeKey, Sensitivity FROM RTMasterVector_new WHERE ConstraintRTNum = @constraintNum ORDER BY Sensitivity asc";
            mSelectBestMinNodesCommand.Parameters.AddWithValue("@constraintNum", "constraintNum");
            mSelectBestMinNodesCommand.Connection = VayuConnection;

            mSelectBestMaxNodesCommand = new MySqlCommand();
            mSelectBestMaxNodesCommand.CommandText = "SELECT top 1 NodeKey, Sensitivity FROM RTMasterVector_new WHERE ConstraintRTNum = @constraintNum ORDER BY Sensitivity desc";
            mSelectBestMaxNodesCommand.Parameters.AddWithValue("@constraintNum", "constraintNum");
            mSelectBestMaxNodesCommand.Connection = VayuConnection;

            mSelectNodeDetailsCommand = new MySqlCommand();
            mSelectNodeDetailsCommand.CommandText = "Select top 1 NodeKey, NodeName, ExternalNodeID, NodeTypeKey, Zone FROM Node WHERE Marketkey = 1 And NodeKey = @NodeKey";
            mSelectNodeDetailsCommand.Parameters.AddWithValue("@NodeKey", "NodeKey");
            mSelectNodeDetailsCommand.Connection = VayuConnection;

            mSelectBestUpMaxCommand = new MySqlCommand();
            mSelectBestUpMaxCommand.CommandText = "SELECT top 1 NodeKey, Sensitivity FROM RTMasterVector_new A INNER JOIN EESPathList B on A.NodeKey = B.SinkNodeKey " +
            "WHERE ConstraintRTNum = @constraintNum ORDER BY Sensitivity desc";
            mSelectBestUpMaxCommand.Parameters.AddWithValue("@constraintNum", "constraintNum");
            mSelectBestUpMaxCommand.Connection = VayuConnection;

            mSelectBestUpMinCommand = new MySqlCommand();
            mSelectBestUpMinCommand.CommandText = "SELECT top 1 NodeKey, Sensitivity FROM RTMasterVector_new A INNER JOIN EESPathList B on A.NodeKey = B.SourceNodeKey " +
            "WHERE ConstraintRTNum = @constraintNum ORDER BY Sensitivity asc";
            mSelectBestUpMinCommand.Parameters.AddWithValue("@constraintNum", "constraintNum");
            mSelectBestUpMinCommand.Connection = VayuConnection;

            mGetFamilyDates = new MySqlCommand();
            mGetFamilyDates.CommandText = "SELECT distinct convert(date, A.MarketDateTime) FROM Vayu..ConstraintRT as A " +
            "INNER JOIN RTMasterConstraint as B ON B.MonitoredText = A.ConstraintText and A.ContingencyText = B.ContingencyText " +
            "INNER JOIN RTFamily as C on B.ConstraintRTNum = C.ConstraintRTNum " +
            "WHERE convert(date, A.MarketDateTime) between @startDate and @endDate AND C.MonitoredText = @familyName " +
            "ORDER BY convert(date, A.MarketDateTime) asc";
            mGetFamilyDates.Parameters.AddWithValue("@familyName", "familyName");
            mGetFamilyDates.Parameters.AddWithValue("@startDate", "startDate");
            mGetFamilyDates.Parameters.AddWithValue("@endDate", "endDate");
            mGetFamilyDates.Connection = VayuConnection;

            mGetHourlyRTImpactFamily = new MySqlCommand();
            mGetHourlyRTImpactFamily.CommandText = "SELECT B.shiftFactor, B.ConstraintRTNum, B.MonitoredText, ContingencyText, Impact/12, hour " +
            "FROM RTImpact as A inner join RTMasterConstraint as B ON A.ConstraintRTNum = B.ConstraintRTNum " +
            "INNER JOIN RTFamily as C on B.ConstraintRTNum = C.ConstraintRTNum WHERE date = @startDate and C.MonitoredText = @familyName " +
            "ORDER BY date asc";
            mGetHourlyRTImpactFamily.Parameters.AddWithValue("@startDate", "startDate");
            mGetHourlyRTImpactFamily.Parameters.AddWithValue("@familyName", "familyName");
            mGetHourlyRTImpactFamily.Connection = VayuConnection;

            mGetMaxImpactHourTodayFamily = new MySqlCommand();
            mGetMaxImpactHourTodayFamily.CommandText = "SELECT max(hour) from RTImpact as A " +
            "INNER JOIN RTFamily as B on A.ConstraintRTNum = B.ConstraintRTNum " +
            " WHERE date = convert(date, @mostRecentImpactDate) and B.MonitoredText = @familyName";
            mGetMaxImpactHourTodayFamily.Parameters.AddWithValue("@mostRecentImpactDate", "mostRecentImpactDate");
            mGetMaxImpactHourTodayFamily.Parameters.AddWithValue("@familyName", "familyName");
            mGetMaxImpactHourTodayFamily.Connection = VayuConnection;

            mGetThisHourRTImpactFamily = new MySqlCommand();
            mGetThisHourRTImpactFamily.CommandText = "SELECT convert(date, A.MarketDateTime), B.ConstraintRTNum, B.MonitoredText, B.ContingencyText, " +
            "(SUM(A.shadowprice)*B.shiftfactor)/@intervals, DATEPART(HH, A.marketdatetime)+1 " +
            "FROM Vayu..ConstraintRT as A inner join RTMasterConstraint as B " +
            "ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText " +
            "INNER JOIN RTFamily as C on B.ConstraintRTNum = C.ConstraintRTNum " +
            "WHERE convert(date, A.MarketDateTime) = @realTimeDate and DATEPART(HH, A.MarketDateTime)+1 between @startHour and @currentHour and C.MonitoredText = @familyName " +
            "GROUP BY B.ShiftFactor, B.ConstraintRTNum, B.MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime), convert(date, A.MarketDateTime) " +
            "ORDER BY convert(date, A.MarketDateTime) asc";
            mGetThisHourRTImpactFamily.Parameters.AddWithValue("@intervals", "intervals");
            mGetThisHourRTImpactFamily.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mGetThisHourRTImpactFamily.Parameters.AddWithValue("@startHour", "startHour");
            mGetThisHourRTImpactFamily.Parameters.AddWithValue("@currentHour", "currentHour");
            mGetThisHourRTImpactFamily.Parameters.AddWithValue("@familyName", "familyName");
            mGetThisHourRTImpactFamily.Connection = VayuConnection;

            mGetRTShadowsFamily = new MySqlCommand();
            mGetRTShadowsFamily.CommandText = "SELECT B.ShiftFactor, B.ConstraintRTNum, B.MonitoredText, B.ContingencyText, sum(abs(A.ShadowPrice))/12, DATEPART(HH, A.MarketDateTime)+1 " +
            "FROM Vayu..ConstraintRT as A inner join RTMasterConstraint as B " +
            "INNER JOIN RTFamily as C on B.ConstraintRTNum = C.ConstraintRTNum " +
            "ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText " +
            "WHERE convert(date, A.MarketDateTime) = @startDate and C.MonitoredText = @familyName " +
            "GROUP BY B.ShiftFactor, B.ConstraintRTNum, B.MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime)+1 ";
            mGetRTShadowsFamily.Parameters.AddWithValue("@startDate", "startDate");
            mGetRTShadowsFamily.Parameters.AddWithValue("@familyName", "familyName");
            mGetRTShadowsFamily.Connection = VayuConnection;

            mGetThisHourRTShadowsFamily = new MySqlCommand();
            mGetThisHourRTShadowsFamily.CommandText = "SELECT B.ShiftFactor, B.ConstraintRTNum, B.MonitoredText, B.ContingencyText, sum(abs(A.ShadowPrice))/@intervals, DATEPART(HH, A.MarketDateTime)+1 " +
            "FROM Vayu..ConstraintRT as A inner join RTMasterConstraint as B " +
            "ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText " +
            "INNER JOIN RTFamily as C on B.ConstraintRTNum = C.ConstraintRTNum " +
            "WHERE convert(date, A.MarketDateTime) = @realTimeDate and DATEPART(HH, A.MarketDateTime)+1 between @startHour and @currentHour " +
            "and C.MonitoredText = @familyName " +
            "GROUP BY B.ShiftFactor, B.ConstraintRTNum, B.MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime)+1 ";
            mGetThisHourRTShadowsFamily.Parameters.AddWithValue("@intervals", "intervals");
            mGetThisHourRTShadowsFamily.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mGetThisHourRTShadowsFamily.Parameters.AddWithValue("@startHour", "startHour");
            mGetThisHourRTShadowsFamily.Parameters.AddWithValue("@currentHour", "currentHour");
            mGetThisHourRTShadowsFamily.Parameters.AddWithValue("@familyName", "familyName");
            mGetThisHourRTShadowsFamily.Connection = VayuConnection;

            mGetRTShiftedFamily = new MySqlCommand();
            mGetRTShiftedFamily.CommandText = "SELECT B.ShiftFactor, B.ConstraintRTNum, B.MonitoredText, B.ContingencyText, (sum(abs(A.ShadowPrice))*B.ShiftFactor)/12, DATEPART(HH, A.MarketDateTime)+1 " +
            "FROM Vayu..ConstraintRT as A inner join RTMasterConstraint as B ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText " +
            "INNER JOIN RTFamily as C on B.ConstraintRTNum = C.ConstraintRTNum WHERE convert(date, A.MarketDateTime) = @startDate and C.MonitoredText = @familyName " +
            "GROUP BY B.ShiftFactor, B.ConstraintRTNum, B.MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime)+1 ";
            mGetRTShiftedFamily.Parameters.AddWithValue("@startDate", "startDate");
            mGetRTShiftedFamily.Parameters.AddWithValue("@familyName", "familyName");
            mGetRTShiftedFamily.Connection = VayuConnection;

            mGetThisHourRTShiftedFamily = new MySqlCommand();
            mGetThisHourRTShiftedFamily.CommandText = "SELECT B.ShiftFactor, B.ConstraintRTNum, B.MonitoredText, B.ContingencyText, (sum(abs(A.ShadowPrice))*B.ShiftFactor)/@intervals, DATEPART(HH, A.MarketDateTime)+1 " +
            "FROM Vayu..ConstraintRT as A inner join RTMasterConstraint as B ON A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText INNER JOIN RTFamily as C on B.ConstraintRTNum = C.ConstraintRTNum " +
            "WHERE convert(date, A.MarketDateTime) = @realTimeDate and DATEPART(HH, A.MarketDateTime)+1 between @startHour and @currentHour and C.MonitoredText = @familyName " +
            "GROUP BY B.ShiftFactor, B.ConstraintRTNum, B.MonitoredText, B.ContingencyText, DATEPART(HH, A.MarketDateTime)+1 ";
            mGetThisHourRTShiftedFamily.Parameters.AddWithValue("@intervals", "intervals");
            mGetThisHourRTShiftedFamily.Parameters.AddWithValue("@realTimeDate", "realTimeDate");
            mGetThisHourRTShiftedFamily.Parameters.AddWithValue("@startHour", "startHour");
            mGetThisHourRTShiftedFamily.Parameters.AddWithValue("@currentHour", "currentHour");
            mGetThisHourRTShiftedFamily.Parameters.AddWithValue("@familyName", "familyName");
            mGetThisHourRTShiftedFamily.Connection = VayuConnection;

            //myqueries
            mGetDistinctEquipment = new MySqlCommand();
            mGetDistinctEquipment.CommandText = "SELECT distinct equipment FROM pjm_rt_outages WHERE CONVERT(date, startDate) <= @day and CONVERT(date, endDate) >= @day and EquipmentType in( 'LINE', 'XFMR' ) " +
            "and OutageStatus = 'Active' and OpenClose = 0 and RemovedDate is null " +
            "ORDER BY equipment asc ";
            mGetDistinctEquipment.Parameters.AddWithValue("@day", "day");
            mGetDistinctEquipment.Connection = VayuConnection;

            //
            mGetErcotDistinctEquipment = new MySqlCommand();
            mGetErcotDistinctEquipment.CommandText = "SELECT distinct EquipmentName,OutageStatus FROM Vayu..Ercot_rt_outages WHERE CONVERT(date, PlannedStartDate) <=@day  and CONVERT(date, PlannedEndtDate) >=@day " +
                                                     " ORDER BY EquipmentName asc ";
            mGetErcotDistinctEquipment.Parameters.AddWithValue("@day", "day");
            mGetErcotDistinctEquipment.Connection = VayuConnection;

            mGetDistinctEquipmentForced = new MySqlCommand();
            mGetDistinctEquipmentForced.CommandText = "SELECT distinct equipment from pjm_current_rt_outages WHERE TicketID = 0 and EquipmentType in ('XFMR', 'LINE') and Status = 'Active' and exitstamp is null";
            mGetDistinctEquipmentForced.Connection = VayuConnection;
            //does this make sense?  trying to pull unsched outages that finished 
            mSelectArchivedCurrentOutages = new MySqlCommand();
            mSelectArchivedCurrentOutages.CommandText = "SELECT distinct equipment from pjm_current_rt_outages WHERE TicketID = 0 and EquipmentType in ('XFMR', 'LINE') and Status = 'Complete' " +
            "and convert(date, stamp) <= @day and convert(date, exitstamp) >=@day ";
            mSelectArchivedCurrentOutages.Parameters.AddWithValue("@day", "day");
            mSelectArchivedCurrentOutages.Connection = VayuConnection;
            //using as a catchall for when pjm_rt_outages doesnt have an outage
            mSelectArchivedSchedOutages = new MySqlCommand();
            mSelectArchivedSchedOutages.CommandText = "SELECT distinct equipment from pjm_current_rt_outages WHERE TicketID > 0 and EquipmentType in ('XFMR', 'LINE') and Status = 'Complete' " +
            "and convert(date, stamp) <= @day and convert(date, exitstamp) >=@day order by equipment asc ";
            mSelectArchivedSchedOutages.Parameters.AddWithValue("@day", "day");
            mSelectArchivedSchedOutages.Connection = VayuConnection;

            mSelectTaggedOutages = new MySqlCommand();
            mSelectTaggedOutages.CommandText = "SELECT distinct Equipment FROM ConstraintOutageTagging";
            mSelectTaggedOutages.Connection = VayuConnection;

            mGetTaggedConNums = new MySqlCommand();
            mGetTaggedConNums.CommandText = "SELECT distinct ConstraintRTNum FROM ConstraintOutageTagging WHERE Equipment = @equipment";
            mGetTaggedConNums.Parameters.AddWithValue("@equipment", "equipment");
            mGetTaggedConNums.Connection = VayuConnection;

            mGetTaggedConNumsByFam = new MySqlCommand();
            mGetTaggedConNumsByFam.CommandText = "SELECT distinct c.ConstraintRTNum FROM ConstraintOutageTagging a INNER JOIN RTFamily as c on a.Family = c.MonitoredText " +
            "WHERE a.Equipment = @equipment ";
            mGetTaggedConNumsByFam.Parameters.AddWithValue("@equipment", "equipment");
            mGetTaggedConNumsByFam.Connection = VayuConnection;

            mGetMaxNode = new MySqlCommand();
            mGetMaxNode.CommandText = "SELECT top 1 A.Nodekey, B.Zone FROM RTMasterVector_new A INNER JOIN Node B ON A.Nodekey = B.Nodekey and B.MarketKey = 1 " +
            "WHERE ConstraintRTNum = @constraintnum and Zone not in ('Load', 'External', '') and Zone is not null ORDER BY Sensitivity desc";
            mGetMaxNode.Parameters.AddWithValue("@constraintnum", "constraintnum");
            mGetMaxNode.Connection = VayuConnection;

            //
            mGetErcotMaxNode = new MySqlCommand();
            mGetErcotMaxNode.CommandText = "SELECT top 1 A.Nodekey, B.Zone FROM Vayu..RTMasterVector_new A INNER JOIN Vayu..Node B ON A.Nodekey = B.Nodekey and B.MarketKey = 9 " +
                                           " WHERE ConstraintRTNum = @constraintnum and Zone not in ('Load', 'External', '') and Zone is not null ORDER BY Sensitivity desc";
            mGetErcotMaxNode.Parameters.AddWithValue("@constraintnum", "constraintnum");
            mGetErcotMaxNode.Connection = VayuConnection;

            mGetMinNode = new MySqlCommand();
            mGetMinNode.CommandText = "SELECT top 1 A.Nodekey, B.Zone FROM RTMasterVector_new A INNER JOIN Node B ON A.Nodekey = B.Nodekey and B.MarketKey = 1 " +
            "WHERE ConstraintRTNum = @constraintnum and Zone not in ('Load', 'External', '') and Zone is not null ORDER BY Sensitivity asc";
            mGetMinNode.Parameters.AddWithValue("@constraintnum", "constraintnum");
            mGetMinNode.Connection = VayuConnection;

            //
            mGetErcotMinNode = new MySqlCommand();
            mGetErcotMinNode.CommandText = "SELECT top 1 A.Nodekey, B.Zone FROM Vayu..RTMasterVector_new A INNER JOIN Vayu..Node B ON A.Nodekey = B.Nodekey and B.MarketKey = 9 " +
                                           " WHERE ConstraintRTNum =@constraintnum  and Zone not in ('Load', 'External', '') and Zone is not null ORDER BY Sensitivity desc";
            mGetErcotMinNode.Parameters.AddWithValue("@constraintnum", "constraintnum");
            mGetErcotMinNode.Connection = VayuConnection;

            mSelectArchivedCurrentOutagesActive = new MySqlCommand();
            mSelectArchivedCurrentOutagesActive.CommandText = "SELECT distinct equipment from pjm_current_rt_outages WHERE TicketID = 0 and EquipmentType in ('XFMR', 'LINE') and Status = 'Active' " +
            "and convert(date, stamp) <= @day ";
            mSelectArchivedCurrentOutagesActive.Parameters.AddWithValue("@day", "day");
            mSelectArchivedCurrentOutagesActive.Connection = VayuConnection;

        }
        /// <summary>
        /// Gets Constraints List.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="RtRadioButton">if set to <c>true</c> [rt RadioButton].</param>
        /// <param name="DaRadioButton">if set to <c>true</c> [da RadioButton].</param>
        /// <param name="ImpactRadioButton">if set to <c>true</c> [impact RadioButton].</param>
        /// <param name="ShiftedRadioButton">if set to <c>true</c> [shifted RadioButton].</param>
        /// <param name="ShadowRadioButton">if set to <c>true</c> [shadow RadioButton].</param>
        /// <param name="DateRangeCheckBox">if set to <c>true</c> [date range CheckBox].</param>
        /// <param name="DatePicker1">The date picker1.</param>
        /// <param name="DatePicker2">The date picker2.</param>
        /// <param name="SelectedFamily">The selected family.</param>
        /// <param name="FamilyCheckBox">if set to <c>true</c> [family CheckBox].</param>
        public void GetConstraints(Action<List<Model.Constraints>, Exception> callback, bool RtRadioButton, bool DaRadioButton, bool ImpactRadioButton, bool ShiftedRadioButton,
            bool ShadowRadioButton, bool DateRangeCheckBox, DateTime DatePicker1, DateTime DatePicker2, string SelectedFamily, bool FamilyCheckBox, int Marketkey)
        {
            List<Model.Constraints> ConstraintsList = new List<Model.Constraints>();
            //RT Impact on Lookback
            if (RtRadioButton == true && ImpactRadioButton == true && DateRangeCheckBox == true)
            {
                if (VayuConnection.State == ConnectionState.Open)
                    VayuConnection.Close();
                VayuConnection.Open();
                if (FamilyCheckBox == true)
                {
                    mGetFamilyDates.Parameters["@familyName"].Value = SelectedFamily.ToString();
                    mGetFamilyDates.Parameters["@startDate"].Value = DatePicker1.Date;
                    mGetFamilyDates.Parameters["@endDate"].Value = DatePicker2.Date;
                    Datereader = mGetFamilyDates.ExecuteReader();
                }
                else
                {
                    if (Marketkey == 1)
                    {
                        mGetDates.Parameters["@startDate"].Value = DatePicker1.Date;
                        mGetDates.Parameters["@endDate"].Value = DatePicker2.Date;
                        Datereader = mGetDates.ExecuteReader();
                    }
                    else
                    {
                        mGetErcotDates.Parameters["@startDate"].Value = DatePicker1.Date;
                        mGetErcotDates.Parameters["@endDate"].Value = DatePicker2.Date;
                        Datereader = mGetErcotDates.ExecuteReader();
                    }
                }
                Dictionary<string, Model.Constraints> constraintHash = new Dictionary<string, Model.Constraints>();
                Dictionary<int, string> constraintMaxZone = new Dictionary<int, string>();
                Dictionary<int, string> constraintMinZone = new Dictionary<int, string>();

                while (Datereader.Read())
                {
                    string date = Convert.ToString(Datereader.GetValue(0));
                    //new code
                    DateTime Today = DateTime.Now;
                    DateTime day = Convert.ToDateTime(Datereader.GetValue(0));
                    List<string> EquipmentList = new List<string>();
                    MySqlDataReader mreader = null;
                    if (Marketkey == 1)
                    {
                        mGetDistinctEquipment.Parameters["@day"].Value = day.Date;
                        mreader = mGetDistinctEquipment.ExecuteReader();
                    }
                    else
                    {
                        mGetErcotDistinctEquipment.Parameters["@day"].Value = day.Date;
                        mreader = mGetErcotDistinctEquipment.ExecuteReader();
                    }
                    while (mreader.Read())
                    {
                        string equipment = mreader.GetString(0);
                        EquipmentList.Add(equipment);
                    }
                    mreader.Close();
                    //BEGIN SLOW CODE
                    MySqlDataReader archiveReader = null;
                    if (Marketkey == 1)
                    {
                        mSelectArchivedCurrentOutages.Parameters["@day"].Value = day.Date;
                        archiveReader = mSelectArchivedCurrentOutages.ExecuteReader();
                        while (archiveReader.Read())
                        {
                            string equipment = archiveReader.GetString(0);
                            EquipmentList.Add(equipment);
                        }
                        archiveReader.Close();

                        mSelectArchivedSchedOutages.Parameters["@day"].Value = day.Date;
                        MySqlDataReader overReader = mSelectArchivedSchedOutages.ExecuteReader();
                        while (overReader.Read())
                        {
                            string result = null;
                            result = EquipmentList.Where(s => s == overReader.GetString(0)).FirstOrDefault();
                            if (result != null) { }
                            else
                            {
                                string missing = overReader.GetString(0);
                                EquipmentList.Add(missing);
                            }
                        }
                        overReader.Close();

                        mSelectArchivedCurrentOutagesActive.Parameters["@day"].Value = day.Date;
                        MySqlDataReader forcedActiveReader = mSelectArchivedCurrentOutagesActive.ExecuteReader();
                        while (forcedActiveReader.Read())
                        {
                            string result = null;
                            result = EquipmentList.Where(s => s == forcedActiveReader.GetString(0)).FirstOrDefault();
                            if (result != null) { }
                            else
                            {
                                string activeForced = forcedActiveReader.GetString(0);
                                EquipmentList.Add(activeForced);
                            }
                        }
                        forcedActiveReader.Close();

                        if (Today.Date == day.Date)
                        {
                            MySqlDataReader greader = mGetDistinctEquipmentForced.ExecuteReader();
                            while (greader.Read())
                            {
                                string result = null;
                                result = EquipmentList.Where(s => s == greader.GetString(0)).FirstOrDefault();
                                if (result != null) { }
                                else
                                {
                                    string unschequipment = greader.GetString(0);
                                    EquipmentList.Add(unschequipment);
                                }
                            }
                            greader.Close();
                        }
                    }
                    EquipmentList.Sort();

                    //pull in all tags and look to see if they are out of service
                    List<string> TaggedListOut = new List<string>();
                    //MySqlDataReader tagReader = mSelectTaggedOutages.ExecuteReader();
                    //while (tagReader.Read())
                    //{
                    //    string result = null;
                    //    string taggedEquip = tagReader.GetString(0);
                    //    result = EquipmentList.Where(s => s == taggedEquip).FirstOrDefault();
                    //    if (result != null)
                    //    {
                    //        TaggedListOut.Add(taggedEquip);
                    //    }
                    //}
                    //tagReader.Close();
                    //make list of all constraint nums affected by todays tagged outages
                    List<int> LinkedConstraintNums = new List<int>();
                    foreach (string tag in TaggedListOut)
                    {
                        //query as though it is a number -- when its 0/isdbull do not add to LinkedConstraintNums
                        mGetTaggedConNums.Parameters["@equipment"].Value = tag;
                        MySqlDataReader wreader = mGetTaggedConNums.ExecuteReader();
                        while (wreader.Read())
                        {
                            if (wreader.IsDBNull(0)) { }
                            else
                            {
                                int constNum = Convert.ToInt32(wreader.GetValue(0));
                                LinkedConstraintNums.Add(constNum);
                            }
                        }
                        wreader.Close();
                        //query as though it is a family inner join with RTFamily to get all constaint nums and add them -- error check for isdbnull
                        mGetTaggedConNumsByFam.Parameters["@equipment"].Value = tag;
                        MySqlDataReader famtryReader = mGetTaggedConNumsByFam.ExecuteReader();
                        while (famtryReader.Read())
                        {
                            if (famtryReader.IsDBNull(0)) { }
                            else
                            {
                                int constNum = Convert.ToInt32(famtryReader.GetValue(0));
                                LinkedConstraintNums.Add(constNum);
                            }
                        }
                        famtryReader.Close();
                    }
                    DateTime dateRetrieve = Convert.ToDateTime(Datereader.GetValue(0));
                    string dayofweek = Convert.ToString(dateRetrieve.DayOfWeek);
                    if (FamilyCheckBox == true)
                    {
                        mGetHourlyRTImpactFamily.Parameters["@startDate"].Value = date;
                        mGetHourlyRTImpactFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                        reader = mGetHourlyRTImpactFamily.ExecuteReader();
                    }
                    else
                    {
                        if (Marketkey == 1)
                        {
                            mGetHourlyRTImpact.Parameters["@startDate"].Value = date;
                            reader = mGetHourlyRTImpact.ExecuteReader();
                        }
                        else
                        {
                            mGetErcotHourlyRTImpact.Parameters["@startDate"].Value = date;
                            reader = mGetErcotHourlyRTImpact.ExecuteReader();
                        }
                    }
                    while (reader.Read())
                    {
                        int hour = Convert.ToInt16(reader.GetValue(5));
                        Model.Constraints constraint = new Model.Constraints();
                        int constraintNum = Convert.ToInt32(reader.GetValue(1));

                        if (constraintHash.ContainsKey(constraintNum + "@" + date))
                        {
                            constraint = constraintHash[constraintNum + "@" + date];
                        }
                        else
                        {
                            constraintHash.Add(constraintNum + "@" + date, constraint);
                            int search = 0;
                            search = LinkedConstraintNums.Where(s => s == constraintNum).FirstOrDefault();
                            if (search > 0) { constraint.color = "yellow"; }
                            //END SLOW CODE
                            //get from/to zones
                            mToZone = "UNK";
                            mFromZone = "UNK";
                            string test = null;
                            constraintMaxZone.TryGetValue(constraintNum, out test);
                            if (test != null) { }
                            else
                            {
                                mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                MySqlDataReader maxReader = mGetMaxNode.ExecuteReader();
                                while (maxReader.Read())
                                {
                                    if (maxReader.IsDBNull(0))
                                    {
                                        mMaxNode = 0;
                                    }
                                    else
                                    {
                                        mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                        mToZone = Convert.ToString(maxReader.GetValue(1));
                                        constraintMaxZone.Add(constraintNum, mToZone);
                                    }
                                }
                                maxReader.Close();
                                MySqlDataReader minReader = null;
                                if (Marketkey == 1)
                                {
                                    mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                    minReader = mGetMinNode.ExecuteReader();
                                }
                                else
                                {
                                    mGetErcotMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                    minReader = mGetErcotMinNode.ExecuteReader();
                                }
                                while (minReader.Read())
                                {
                                    if (minReader.IsDBNull(0))
                                    {
                                        mMinNode = 0;
                                    }
                                    else
                                    {
                                        mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                        mFromZone = Convert.ToString(minReader.GetValue(1));
                                        if (!constraintMinZone.ContainsKey(constraintNum))
                                            constraintMinZone.Add(constraintNum, mFromZone);
                                    }
                                }
                                minReader.Close();
                            }
                        }
                        constraint.date = date;
                        constraint.constraintNum = constraintNum;
                        string toZone = null; string fromZone = null;
                        constraintMaxZone.TryGetValue(constraintNum, out toZone);
                        constraintMinZone.TryGetValue(constraintNum, out fromZone);
                        constraint.fromZone = fromZone;
                        constraint.toZone = toZone;
                        constraint.monitoredName = reader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                        constraint.contingName = reader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                        if (hour == 1) { constraint.he1Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 2) { constraint.he2Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 3) { constraint.he3Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 4) { constraint.he4Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 5) { constraint.he5Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 6) { constraint.he6Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 7) { constraint.he7Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 8) { constraint.he8Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 9) { constraint.he9Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 10) { constraint.he10Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 11) { constraint.he11Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 12) { constraint.he12Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 13) { constraint.he13Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 14) { constraint.he14Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 15) { constraint.he15Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 16) { constraint.he16Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 17) { constraint.he17Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 18) { constraint.he18Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 19) { constraint.he19Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 20) { constraint.he20Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 21) { constraint.he21Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 22) { constraint.he22Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 23) { constraint.he23Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        if (hour == 24) { constraint.he24Value = reader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)); }
                        constraint.dayofweek = dayofweek;
                    }
                    reader.Close();
                    DateTime currentTime = DateTime.Now;
                    //if today is the date user selected we need to deal with current hour impact estimate
                    if (Convert.ToString(currentTime.Date) == date)
                    {
                        dayofweek = Convert.ToString(currentTime.DayOfWeek);
                        int currentHour = Convert.ToInt32(currentTime.ToString("HH")) + 1;
                        //make it hour ending by adding 1
                        currentHour = currentHour + 1;
                        int maxImpactHour = 0;
                        int currentMinute = Convert.ToInt32(currentTime.ToString("mm"));
                        double currentElapsedInterval = 0;
                        if (FamilyCheckBox == true)
                        {
                            mGetMaxImpactHourTodayFamily.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                            mGetMaxImpactHourTodayFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                            Ireader = mGetMaxImpactHourTodayFamily.ExecuteReader();
                        }
                        else
                        {
                            if (Marketkey == 1)
                            {
                                mGetMaxImpactHourToday.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                                Ireader = mGetMaxImpactHourToday.ExecuteReader();
                            }
                            else
                            {

                                mGetErcotMaxImpactHourToday.Parameters["@mostRecentImpactDate"].Value = currentTime.Date;
                                Ireader = mGetErcotMaxImpactHourToday.ExecuteReader();
                            }
                        }
                        while (Ireader.Read())
                        {
                            if (!Ireader.IsDBNull(0))
                            {
                                maxImpactHour = Convert.ToInt32(Ireader.GetValue(0));
                            }
                        }
                        Ireader.Close();
                        //when time == -1 we run the query once -- one intervals value
                        if (maxImpactHour - currentHour == -1) //we need to divide sum(SP)*SF by intervals elapsed
                        {
                            currentElapsedInterval = currentMinute / 5;
                            currentElapsedInterval = Math.Floor(currentElapsedInterval);
                            if (FamilyCheckBox == true)
                            {
                                mGetThisHourRTImpactFamily.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisHourRTImpactFamily.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetThisHourRTImpactFamily.Parameters["@currentHour"].Value = currentHour;
                                mGetThisHourRTImpactFamily.Parameters["@intervals"].Value = currentElapsedInterval;
                                mGetThisHourRTImpactFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                                Jreader = mGetThisHourRTImpactFamily.ExecuteReader();
                            }
                            else
                            {
                                mGetThisHourRTImpact.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisHourRTImpact.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetThisHourRTImpact.Parameters["@currentHour"].Value = currentHour;
                                mGetThisHourRTImpact.Parameters["@intervals"].Value = currentElapsedInterval;
                                Jreader = mGetThisHourRTImpact.ExecuteReader();
                            }
                            while (Jreader.Read())
                            {
                                int hour = Convert.ToInt16(Jreader.GetValue(5));
                                Model.Constraints constraint = new Model.Constraints();
                                int constraintNum = Convert.ToInt32(Jreader.GetValue(1));
                                if (constraintHash.ContainsKey(constraintNum + "@" + date))
                                {
                                    constraint = constraintHash[constraintNum + "@" + date];
                                }
                                else
                                {
                                    constraintHash.Add(constraintNum + "@" + date, constraint);

                                    int search = 0;
                                    search = LinkedConstraintNums.Where(s => s == constraintNum).FirstOrDefault();
                                    if (search > 0) { constraint.color = "yellow"; }
                                    //get from/to zones
                                    mToZone = "UNK";
                                    mFromZone = "UNK";
                                    string test = null;
                                    constraintMaxZone.TryGetValue(constraintNum, out test);
                                    if (test != null) { }
                                    else
                                    {
                                        mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                        MySqlDataReader maxReader = mGetMaxNode.ExecuteReader();
                                        while (maxReader.Read())
                                        {
                                            if (maxReader.IsDBNull(0))
                                            {
                                                mMaxNode = 0;
                                            }
                                            else
                                            {
                                                mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                                mToZone = Convert.ToString(maxReader.GetValue(1));
                                                constraintMaxZone.Add(constraintNum, mToZone);
                                            }
                                        }
                                        maxReader.Close();
                                        mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                        MySqlDataReader minReader = mGetMinNode.ExecuteReader();
                                        while (minReader.Read())
                                        {
                                            if (minReader.IsDBNull(0))
                                            {
                                                mMinNode = 0;
                                            }
                                            else
                                            {
                                                mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                                mFromZone = Convert.ToString(minReader.GetValue(1));
                                                constraintMinZone.Add(constraintNum, mFromZone);
                                            }
                                        }
                                        minReader.Close();
                                    }
                                }
                                constraint.date = date;
                                constraint.constraintNum = constraintNum;
                                string toZone = null; string fromZone = null;
                                constraintMaxZone.TryGetValue(constraintNum, out toZone);
                                constraintMinZone.TryGetValue(constraintNum, out fromZone);
                                constraint.fromZone = fromZone;
                                constraint.toZone = toZone;
                                constraint.monitoredName = Jreader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                                constraint.contingName = Jreader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                                if (hour == 1) { constraint.he1Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 2) { constraint.he2Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 3) { constraint.he3Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 4) { constraint.he4Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 5) { constraint.he5Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 6) { constraint.he6Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 7) { constraint.he7Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 8) { constraint.he8Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 9) { constraint.he9Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 10) { constraint.he10Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 11) { constraint.he11Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 12) { constraint.he12Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 13) { constraint.he13Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 14) { constraint.he14Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 15) { constraint.he15Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 16) { constraint.he16Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 17) { constraint.he17Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 18) { constraint.he18Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 19) { constraint.he19Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 20) { constraint.he20Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 21) { constraint.he21Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 22) { constraint.he22Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 23) { constraint.he23Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                if (hour == 24) { constraint.he24Value = Jreader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Jreader.GetValue(4)), 2)); }
                                constraint.dayofweek = dayofweek;
                            }
                            Jreader.Close();
                        }
                        if (maxImpactHour - currentHour < -1) // we need to divide sum(SP*SF) by 12 and just assume we scraped all intervals
                        {
                            dayofweek = Convert.ToString(currentTime.DayOfWeek);
                            //select shift factor, constraintrtnum, monitoredtext, contingencytext, sum(SP*SF)/12, datepart(HH, marketdatetime)+1 
                            //where hours between maxImpactHour+1 and currentHour
                            //where date is today
                            if (FamilyCheckBox == true)
                            {
                                mGetThisHourRTImpactFamily.Parameters["@realTimeDate"].Value = currentTime.Date;
                                mGetThisHourRTImpactFamily.Parameters["@startHour"].Value = maxImpactHour + 1;
                                mGetThisHourRTImpactFamily.Parameters["@currentHour"].Value = currentHour;
                                mGetThisHourRTImpactFamily.Parameters["@intervals"].Value = 12;
                                mGetThisHourRTImpactFamily.Parameters["@familyName"].Value = SelectedFamily.ToString();
                                Freader = mGetThisHourRTImpactFamily.ExecuteReader();
                            }
                            else
                            {
                                if (Marketkey == 1)
                                {
                                    mGetThisHourRTImpact.Parameters["@realTimeDate"].Value = currentTime.Date;
                                    mGetThisHourRTImpact.Parameters["@startHour"].Value = maxImpactHour + 1;
                                    mGetThisHourRTImpact.Parameters["@currentHour"].Value = currentHour;
                                    mGetThisHourRTImpact.Parameters["@intervals"].Value = 12;
                                    Freader = mGetThisHourRTImpact.ExecuteReader();
                                }
                                else
                                {
                                    mGetErcotThisHourRTImpact.Parameters["@realTimeDate"].Value = currentTime.Date;
                                    mGetErcotThisHourRTImpact.Parameters["@startHour"].Value = maxImpactHour + 1;
                                    mGetErcotThisHourRTImpact.Parameters["@currentHour"].Value = currentHour;
                                    mGetErcotThisHourRTImpact.Parameters["@intervals"].Value = 12;
                                    Freader = mGetErcotThisHourRTImpact.ExecuteReader();
                                }
                            }
                            while (Freader.Read())
                            {
                                int hour = Convert.ToInt16(Freader.GetValue(5));
                                Model.Constraints constraint = new Model.Constraints();
                                int constraintNum = Convert.ToInt32(Freader.GetValue(1));
                                if (constraintHash.ContainsKey(constraintNum + "@" + date))
                                {
                                    constraint = constraintHash[constraintNum + "@" + date];
                                }
                                else
                                {
                                    constraintHash.Add(constraintNum + "@" + date, constraint);
                                    int search = 0;
                                    search = LinkedConstraintNums.Where(s => s == constraintNum).FirstOrDefault();
                                    if (search > 0) { constraint.color = "yellow"; }
                                    //get from/to zones
                                    mToZone = "UNK";
                                    mFromZone = "UNK";
                                    string test = null;
                                    constraintMaxZone.TryGetValue(constraintNum, out test);
                                    if (test != null) { }
                                    else
                                    {
                                        MySqlDataReader maxReader = null;
                                        if (Marketkey == 1)
                                        {
                                            mGetMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                            maxReader = mGetMaxNode.ExecuteReader();
                                        }
                                        else
                                        {
                                            mGetErcotMaxNode.Parameters["@constraintnum"].Value = constraintNum;
                                            maxReader = mGetErcotMaxNode.ExecuteReader();
                                        }
                                        while (maxReader.Read())
                                        {
                                            if (maxReader.IsDBNull(0))
                                            {
                                                mMaxNode = 0;
                                            }
                                            else
                                            {
                                                mMaxNode = Convert.ToInt32(maxReader.GetValue(0));
                                                mToZone = Convert.ToString(maxReader.GetValue(1));
                                                constraintMaxZone.Add(constraintNum, mToZone);
                                            }
                                        }
                                        maxReader.Close();
                                        MySqlDataReader minReader = null;
                                        if (Marketkey == 1)
                                        {
                                            mGetMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                            minReader = mGetMinNode.ExecuteReader();
                                        }
                                        else
                                        {
                                            mGetErcotMinNode.Parameters["@constraintnum"].Value = constraintNum;
                                            minReader = mGetErcotMinNode.ExecuteReader();
                                        }
                                        while (minReader.Read())
                                        {
                                            if (minReader.IsDBNull(0))
                                            {
                                                mMinNode = 0;
                                            }
                                            else
                                            {
                                                mMinNode = Convert.ToInt32(minReader.GetValue(0));
                                                mFromZone = Convert.ToString(minReader.GetValue(1));
                                                constraintMinZone.Add(constraintNum, mFromZone);
                                            }
                                        }
                                        minReader.Close();
                                    }
                                }
                                constraint.date = date;
                                constraint.constraintNum = constraintNum;
                                string toZone = null; string fromZone = null;
                                constraintMaxZone.TryGetValue(constraintNum, out toZone);
                                constraintMinZone.TryGetValue(constraintNum, out fromZone);
                                constraint.fromZone = fromZone;
                                constraint.toZone = toZone;
                                constraint.monitoredName = Freader.GetValue(2).ToString().Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                                constraint.contingName = Freader.GetValue(3).ToString().Replace("Contingency", "").TrimStart();
                                if (hour == 1) { constraint.he1Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 2) { constraint.he2Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 3) { constraint.he3Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 4) { constraint.he4Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 5) { constraint.he5Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 6) { constraint.he6Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 7) { constraint.he7Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 8) { constraint.he8Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 9) { constraint.he9Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 10) { constraint.he10Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 11) { constraint.he11Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 12) { constraint.he12Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 13) { constraint.he13Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 14) { constraint.he14Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 15) { constraint.he15Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 16) { constraint.he16Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 17) { constraint.he17Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 18) { constraint.he18Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 19) { constraint.he19Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 20) { constraint.he20Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 21) { constraint.he21Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 22) { constraint.he22Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 23) { constraint.he23Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                if (hour == 24) { constraint.he24Value = Freader.IsDBNull(4) ? "X" : Convert.ToString(Math.Round(Convert.ToDouble(Freader.GetValue(4)), 2)); }
                                constraint.dayofweek = dayofweek;
                            }
                            Freader.Close();
                        }
                    }
                }
                ConstraintsList = constraintHash.Values.ToList<Model.Constraints>();
                Datereader.Close();
                VayuConnection.Close();
            }

            ConstraintsList = GetConstraints1(callback, RtRadioButton, DaRadioButton, ImpactRadioButton, ShiftedRadioButton, ShadowRadioButton, DateRangeCheckBox, DatePicker1, DatePicker2, SelectedFamily, FamilyCheckBox, ConstraintsList, Marketkey);
        }

        #endregion
    }
}
