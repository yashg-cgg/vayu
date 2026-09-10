using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;
using Vayu.PowerMap.ViewModel;

namespace Vayu.PowerMap.ViewModels
{
    public class CongestionViewModel : BindableBase
    {
        public CongestionViewModel()
        {

        }
        /// <summary>
        /// The Sigma database connection
        /// </summary>
        private SqlConnection VayuConnection;
        /// <summary>
        /// The m select constraint command
        /// </summary>
        private SqlCommand mSelectConstraintCommand;
        /// <summary>
        /// The m select maximum load command
        /// </summary>
        private SqlCommand mSelectMaxLoadCommand;
        /// <summary>
        /// The m load hash
        /// </summary>
        private Dictionary<DateTime, double> mLoadHash = new Dictionary<DateTime, double>();

        /// <summary>
        /// The m congestion list
        /// </summary>
        private List<Congestion> mCongestionList;
        /// <summary>
        /// Gets or sets the congestion list.
        /// </summary>
        /// <value>
        /// The congestion list.
        /// </value>
        public List<Congestion> CongestionList
        {
            get
            {
                return mCongestionList;
            }
            set
            {
                mCongestionList = value;
                RaisePropertyChanged("CongestionList");
            }
        }

        /// <summary>
        /// Loads the database commands.
        /// </summary>
        private void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            mSelectConstraintCommand = new SqlCommand();
            mSelectConstraintCommand.CommandText = "select distinct marketdatetime, constrainttext, contingencytext, shadowprice from constraintrt where marketdatetime > @start and marketdatetime < @end and marketkey = 1 and " +
                                                    "order by marketdatetime";
            mSelectConstraintCommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectConstraintCommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectConstraintCommand.CommandTimeout = 30000;
            mSelectConstraintCommand.Connection = VayuConnection;
            //
            mSelectMaxLoadCommand = new SqlCommand();
            mSelectMaxLoadCommand.CommandText = "select max(mw) from loadforecasts where LoadForecastTypeKey = 10 and marketdatetime > @start and marketdatetime <= @end";
            mSelectMaxLoadCommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectMaxLoadCommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectMaxLoadCommand.Connection = VayuConnection;
        }
        /// <summary>
        /// Sets the path.
        /// </summary>
        /// <param name="outageZoneList">The outage zone list.</param>
        public void SetPath(List<OutageZone> outageZoneList)
        {
            loadDBCommands();
            List<string> foundPathList = new List<string>();
            Dictionary<string, OutageZone> pathHash = new Dictionary<string, OutageZone>();
            Dictionary<DateTime, Dictionary<string, Dictionary<string, double>>> marketDateHash = new Dictionary<DateTime, Dictionary<string, Dictionary<string, double>>>();
            VayuConnection.Open();
            string selectPath = "";
            foreach (OutageZone outageZone in outageZoneList)
            {
                if (foundPathList.Contains(outageZone.Outage))
                {
                    continue;
                }
                foundPathList.Add(outageZone.Outage);
                string[] nodes = outageZone.Outage.Split('-');
                string path1 = nodes[0];
                if (outageZone.Outage.ToUpper().IndexOf("XFORMER") != -1)
                {
                    string[] xfrStrings = path1.Split(' ');
                    string first = xfrStrings[0];
                    try
                    {
                        Int32.Parse(first);
                        first = xfrStrings[0] + " " + xfrStrings[1];
                    }
                    catch (Exception ex)
                    {
                    }
                    path1 = first + "%" + outageZone.Voltage + "%XFORMER";
                }
                else
                {
                    if (nodes.Length > 1)
                    {
                        string[] sink = nodes[1].Split(' ');
                        path1 = nodes[0] + "-" + sink[0].Trim();
                    }
                }
                if (selectPath != "")
                {
                    selectPath = selectPath + "%' or contingencytext like '%" + path1;
                }
                else
                {
                    selectPath = "(contingencytext like '%" + path1;
                }
            }
            if (string.IsNullOrEmpty(selectPath))
            {
                selectPath = selectPath + "('%";
            }
            selectPath = selectPath + "%')";
            if (selectPath != "('%%')")
            {
                mSelectConstraintCommand.CommandText = "select distinct marketdatetime, constrainttext, contingencytext, shadowprice from pjm.constraintrt (nolock)  where marketdatetime > @start and " +
                                                        "marketdatetime < @end  and " + selectPath + " order by marketdatetime";
                mSelectConstraintCommand.Parameters["@start"].Value = DateTime.Today.AddYears(-5);
                mSelectConstraintCommand.Parameters["@end"].Value = DateTime.Today.AddDays(1);
                SqlDataReader reader = mSelectConstraintCommand.ExecuteReader();
                while (reader.Read())
                {
                    DateTime marketDate = reader.GetDateTime(0).Date;
                    string constraintName = reader.GetString(1);
                    string contingencyName = reader.GetString(2);
                    double shadowPrice = reader.IsDBNull(3) ? 0 : (double)reader.GetDecimal(3);
                    Dictionary<string, Dictionary<string, double>> constraintHash = new Dictionary<string, Dictionary<string, double>>();
                    if (marketDateHash.ContainsKey(marketDate))
                    {
                        constraintHash = marketDateHash[marketDate];
                        marketDateHash.Remove(marketDate);
                    }
                    Dictionary<string, double> contingencyHash = new Dictionary<string, double>();
                    if (constraintHash.ContainsKey(constraintName))
                    {
                        contingencyHash = constraintHash[constraintName];
                        constraintHash.Remove(constraintName);
                    }
                    if (contingencyHash.ContainsKey(contingencyName))
                    {
                        shadowPrice += contingencyHash[contingencyName];
                        contingencyHash.Remove(contingencyName);
                    }
                    contingencyHash.Add(contingencyName, shadowPrice);
                    constraintHash.Add(constraintName, contingencyHash);
                    marketDateHash.Add(marketDate, constraintHash);
                    if (!pathHash.ContainsKey(constraintName + contingencyName))
                    {
                        OutageZone path = null;
                        foreach (OutageZone compPath in outageZoneList)
                        {
                            if (compPath.Outage.ToUpper().IndexOf("XFORMER") != -1)
                            {
                                string[] nodes = compPath.Outage.Split(' ');
                                if (contingencyName.IndexOf(nodes[0]) != -1 && contingencyName.IndexOf("XFORMER") != -1 && contingencyName.IndexOf(compPath.Voltage.ToString()) != -1)
                                {
                                    path = compPath;
                                    break;
                                }
                            }
                            else
                            {
                                string[] nodes = compPath.Outage.Split('-');
                                string path1 = nodes[0];
                                if (nodes.Length > 1)
                                {
                                    string[] sink = nodes[1].Split(' ');
                                    path1 = nodes[0] + "-" + sink[0].Trim();
                                }
                                if (contingencyName.IndexOf(path1) != -1)
                                {
                                    path = compPath;
                                    break;
                                }
                            }
                        }
                        pathHash.Add(constraintName + contingencyName, path);
                    }
                }
                reader.Close();
                VayuConnection.Close();
                List<Congestion> congestionList = new List<Congestion>();
                List<DateTime> marketDateList = marketDateHash.Keys.ToList<DateTime>();
                marketDateList.Sort();
                marketDateList.Reverse();
                foreach (DateTime marketDate in marketDateList)
                {
                    double? mw = null;
                    VayuConnection.Open();
                    mSelectMaxLoadCommand.Parameters["@start"].Value = marketDate;
                    mSelectMaxLoadCommand.Parameters["@end"].Value = marketDate.AddDays(1);
                    reader = mSelectMaxLoadCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            mw = (double)reader.GetDecimal(0);
                        }
                    }
                    reader.Close();
                    VayuConnection.Close();
                    Dictionary<string, Dictionary<string, double>> constraintHash = marketDateHash[marketDate];
                    List<string> constraintList = constraintHash.Keys.ToList<string>();
                    constraintList.Sort();
                    foreach (string constraint in constraintList)
                    {
                        Dictionary<string, double> contingencyHash = constraintHash[constraint];
                        List<string> contingencyList = contingencyHash.Keys.ToList<string>();
                        contingencyList.Sort();
                        foreach (string contingency in contingencyList)
                        {
                            Congestion congestion = new Congestion();
                            OutageZone outageZone = pathHash[constraint + contingency];
                            congestion.Equipment = outageZone.Outage;
                            congestion.Zone = outageZone.Zone;
                            congestion.MarketDate = marketDate;
                            congestion.ConstraintName = constraint;
                            congestion.ContingencyName = contingency;
                            congestion.ShadowPrice = contingencyHash[contingency];
                            congestion.MaxLoad = mw;
                            congestionList.Add(congestion);
                        }
                    }
                }
                CongestionList = congestionList;
            }
        }
    }
}
