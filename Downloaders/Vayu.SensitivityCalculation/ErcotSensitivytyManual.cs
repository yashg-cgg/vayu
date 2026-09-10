using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Vayu.SensitivityCalculation;
using Vayu.NodePriceLibrary;
using System.Data;
using Vayu.CommonAccessLibrary;

namespace Vayu.SensitivityCalculation
{
    class ErcotSensitivytyManual
    {
        private SqlConnection VayuDbConnErcot;
        private Dictionary<string, double> mSensitivityHash = new Dictionary<string, double>();
        private SqlCommand mUpdateErcotConstraintCommand;
        //string ercotConnString = new VayuDBConnection().GetInstance().GetSqlConnection();
        private SqlConnection ercotConnString = new VayuDBConnection().GetInstance().GetSqlConnection();

        public void Run()
        {
            try
            {
                Console.WriteLine("Fetching COnstraint List");
                Dictionary<int, Congestion> unknownCOnstraintDict = GetUnsavedConstraints();
                foreach (var item in unknownCOnstraintDict)
                {
                    int constraintId = item.Key;
                    Console.WriteLine("Calculating for Constraint" + constraintId.ToString());
                    if (constraintId != 183)
                    {
                        //  continue;
                    }
                    Congestion cong = item.Value;
                    GetMinConstraintCount(constraintId, cong.ConstraintName, cong.ContingencyName);
                }
                Console.WriteLine("Completed for All Constraints");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        private void UpdateRtMasterConstraints(DateTime marketDatetime, string constraint, string contingency, double maxLmp, double minLmp, List<Congestion> constraintList, int constraintCount)
        {
            double totalShadowPrice = constraintList.Sum(a => a.ShadowPrice);
            Congestion cong = constraintList.Find(a => a.ConstraintName == constraint && a.ContingencyName == contingency);
            double shadowPrice = cong.ShadowPrice;
            VayuDbConnErcot = ercotConnString;
            ConstraintElement ce = new ConstraintElement();
            ce.Monitor = constraint;
            ce.Contingency = contingency;
            ce.ShadowPrice = shadowPrice;
            double shiftFactor = minLmp == 0 && maxLmp == 0 ? 0 : (Math.Abs(maxLmp - minLmp) / totalShadowPrice) * (shadowPrice / totalShadowPrice);
            ce.ShiftFactor = minLmp == 0 && maxLmp == 0 ? 0 : (Math.Abs(maxLmp - minLmp) / totalShadowPrice) * (shadowPrice / totalShadowPrice);
            ce.Score = 0;
            ce.ImpactRatio = 1;
            ce.Impact = shadowPrice * shiftFactor;
            ce.UpdateTime = DateTime.Now;
            ce.MarketDateTimeInterval = marketDatetime;

            if (VayuDbConnErcot.State == ConnectionState.Open)
            {
                VayuDbConnErcot.Close();
            }
            VayuDbConnErcot.Open();
            mUpdateErcotConstraintCommand = new SqlCommand();
            mUpdateErcotConstraintCommand.Connection = VayuDbConnErcot;
            mUpdateErcotConstraintCommand.CommandText = "update Vayu..RTMasterConstraint set marketdatetime = @marketdatetime, shadowprice = @shadowprice, numberconstraints = @numberconstraints, " +
                "shiftfactor = @shiftfactor, dollarimpact = @dollarimpact, updatetime = @updatetime where monitoredtext = @MonitoredText and contingencytext = @ContingencyText";
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@marketdatetime", ce.MarketDateTimeInterval);
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@shadowprice", ce.ShadowPrice);
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@numberconstraints", constraintCount);
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@shiftfactor", ce.ShiftFactor);
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@dollarimpact", ce.Impact);
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@updatetime", DateTime.Now);
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@MonitoredText", constraint);
            mUpdateErcotConstraintCommand.Parameters.AddWithValue("@ContingencyText", ce.Contingency);
            int Updatecount = mUpdateErcotConstraintCommand.ExecuteNonQuery();
            VayuDbConnErcot.Close();
        }

        private Dictionary<int, Congestion> GetUnsavedConstraints()
        {
            Dictionary<int, Congestion> constraintDict = new Dictionary<int, Congestion>();
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
               
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select b.ConstraintRTNum , a.ConstraintText , a.ContingencyText from Vayu..ConstraintRT a join Vayu..RTMasterConstraint b on " +
                                   " a.ConstraintText = b.MonitoredText  and a.ContingencyText = b.ContingencyText where a.MarketDateTime > '6/20/2019' and a.MarketDateTime < '6/21/2019'and a.ShadowPrice > 0";

                    cmd.Connection = con;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Congestion cong = new Congestion();
                        int constraintId = Convert.ToInt32(rdr.GetValue(0));
                        cong.ConstraintName = rdr.GetString(1);
                        cong.ContingencyName = rdr.GetString(2);
                        if (!constraintDict.ContainsKey(constraintId))
                            constraintDict.Add(constraintId, cong);
                    }
                    rdr.Close();
                }

                con.Close();
            }
            return constraintDict;
        }

        public void RunManual(int constraintId, string constraint, string contingency)
        {
            GetMinConstraintCount(constraintId, constraint, contingency);
        }

        private void GetMinConstraintCount(int constraintId, string constraint, string contingency)
        {
            try
            {
                Dictionary<DateTime, int> constraintCountDict = new Dictionary<DateTime, int>();
                List<DateTime> constraintDateList = new List<DateTime>();
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "select MarketDateTime, COUNT(*)  from Vayu..Constraintda (nolock) " +
                          " where MarketDateTime in  (select MarketDateTime from Vayu..Constraintda (nolock) " +
                          "  where ContingencyText = '" + contingency + "' and ConstraintText = '" + constraint + "' and abs(shadowprice) > 0 ) and marketdatetime > " +
                           " '2018-04-1'  and ABS( ShadowPrice ) > 0 group by MarketDateTime order by COUNT(*), MarketDateTime desc";
                        cmd.Connection = con;
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            DateTime date = Convert.ToDateTime(rdr.GetValue(0));
                            int constcount = Convert.ToInt32(rdr.GetValue(1));
                            constraintDateList.Add(date);
                            constraintCountDict.Add(date, constcount);
                        }
                        rdr.Close();
                    }

                    con.Close();
                }
                if (constraintDateList.Count == 0)
                    return;
                foreach (DateTime date in constraintDateList)
                {
                    DateTime minCountDate = date;
                    // constraintCountDict.Remove(minCountDate);
                    List<Congestion> constraintList = GetConstraints(minCountDate);
                    int constraintCount = constraintList.Count();
                    int existingCount = GetInsertedConstraintCount(minCountDate);
                    if ((existingCount == constraintCount || existingCount == constraintCount - 1)&& constraintCount!=0)
                    {
                        try
                        {
                            Node[] priceNodes = GetErcotCongestions(minCountDate);
                            double maxCongestion = priceNodes.Max(a => a.LmpTimePriceList.Max(b => b.Lmp.Congestion));
                            double minCongestion = priceNodes.Min(a => a.LmpTimePriceList.Min(b => b.Lmp.Congestion));
                            UpdateRtMasterConstraints(minCountDate, constraint, contingency, maxCongestion, minCongestion, constraintList, existingCount);
                            List<Element> elementList = new List<Element>();
                            foreach (Node item in priceNodes)
                            {
                                Element element = new Element();
                                element.Nodekey = item.NodeId;
                                List<Element> result = elementList.Where(t => t.Nodekey.Equals(item.NodeId)).ToList();
                                if (result.Count > 0)
                                {
                                    int index = elementList.FindIndex(t => t.Nodekey.Equals(item.NodeId));
                                    elementList.RemoveAt(index);
                                }
                                List<LmpTimePrice> RTTimePriceList = item.LmpTimePriceList;
                                double congestion = RTTimePriceList[0].Lmp.Congestion;
                                element.Congestion = congestion;
                                element.SensitivityHash = CalculateSensitivity(constraintList, congestion, element.Nodekey);
                                elementList.Add(element);

                            }
                            Console.WriteLine("Inserting Vector for constraint" + constraintId.ToString());
                            InsertVector(elementList, constraintId, constraint, contingency);
                            break;
                        }
                        catch (Exception ex)
                        {

                            continue;
                        }
                    }
                    else
                    {
                        continue;
                       
                    }
                }

                //

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private int GetInsertedConstraintCount(DateTime minCountDate)
        {
            int count = 0;
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
               
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select COUNT(*) from Vayu..ConstraintRT a join Vayu..RTMasterConstraint b on a.ConstraintText = b.MonitoredText and a.ContingencyText = b.ContingencyText where a.MarketDateTime = '" + minCountDate.ToString() + "' " +
                        " and b.ConstraintRTNum in (select distinct ConstraintRTNum  from Vayu..RTMasterVector group by ConstraintRTNum having COUNT(*) > 500) and a.ShadowPrice > 0";
                    cmd.Connection = con;
                    count = Convert.ToInt32(cmd.ExecuteScalar());
                }
                con.Close();
            }
            return count;
        }

        private List<int> GetErcotSensetivities(int constraintID)
        {
            List<int> nodeList = new List<int>();
            try
            {
                using (SqlConnection DBConnectionVayu = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    
                    SqlCommand cmd = DBConnectionVayu.CreateCommand();
                    cmd.Connection = DBConnectionVayu;
                    cmd.CommandText = "select distinct NodeKey from Vayu..RTMasterVector where ConstraintRTNum =" + constraintID;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        int nodeKey = Convert.ToInt32(rdr.GetValue(0));
                        nodeList.Add(nodeKey);
                    }
                    DBConnectionVayu.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return nodeList;
        }
        private void InsertVector(List<Element> elementlist, int constraintID, string constraintName, string contingencyName)
        {
            try
            {
                DataTable mVectorTable = new DataTable();
                mVectorTable.Columns.Add("ConstraintRTNum");
                mVectorTable.Columns.Add("NodeKey");
                mVectorTable.Columns.Add("Sensitivity");
                mVectorTable.Columns.Add("SensitivityNormal");
                // mVectorTable.Columns.Add();

                List<int> keyList = new List<int>();
                string sensKey = constraintName + "?" + contingencyName;
                DataTable ercotVectTab = mVectorTable.Clone();

                //  else if (mMarket == "ERCOT")
                {
                    //     if (nodeSensListErcot.Count < elementlist.Count)
                    {
                        foreach (Element elm in elementlist)
                        {
                            if (elm.Nodekey == 57529)
                            {

                            }
                            if (!elm.SensitivityHash.ContainsKey(sensKey))
                            {
                                //   continue;
                                DataRow row1 = mVectorTable.NewRow();
                                row1["ConstraintRTNum"] = constraintID;
                                row1["NodeKey"] = elm.Nodekey;
                                row1["Sensitivity"] = 0;
                                row1["SensitivityNormal"] = 0;
                                mVectorTable.Rows.Add(row1);
                            }
                            else
                            {
                                DataRow row = mVectorTable.NewRow();
                                row["ConstraintRTNum"] = constraintID;
                                row["NodeKey"] = elm.Nodekey;
                                row["Sensitivity"] = Math.Round(elm.SensitivityHash[sensKey], 4);
                                row["SensitivityNormal"] = 0;
                                mVectorTable.Rows.Add(row);
                            }

                        }
                    }
                }
                if (mVectorTable.Rows.Count == 0)
                {
                    return;
                }
                using (SqlConnection DBConnectionVayu = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    
                    SqlCommand deleteVectorCommand = DBConnectionVayu.CreateCommand();
                    deleteVectorCommand.CommandText = " delete Vayu..RTMasterVector where constraintrtnum = @constraintrtnum and source is null  ";
                    deleteVectorCommand.Connection = DBConnectionVayu;
                    deleteVectorCommand.Parameters.AddWithValue("@constraintrtnum", "constraintrtnum");
                    deleteVectorCommand.Parameters["@constraintrtnum"].Value = constraintID;
                    deleteVectorCommand.ExecuteNonQuery();


                    List<int> nodeSensListErcot = GetErcotSensetivities(constraintID);

                    foreach (DataRow dr in mVectorTable.Rows)
                    {
                        int nodekey = Convert.ToInt32(dr["NodeKey"]);
                        if (!nodeSensListErcot.Contains(nodekey))
                        {
                            ercotVectTab.Rows.Add(dr.ItemArray);
                        }

                    }
                    try
                    {
                        Console.WriteLine(DateTime.Now + " Inside Bulk Copy Vector");

#if TEST
                SqlBulkCopy bulkCopy = new SqlBulkCopy(DBConnectionVayu);
                bulkCopy.BulkCopyTimeout = 60 * 10;
                bulkCopy.DestinationTableName = mMarket == "MISO" ? "miso.RTMasterVector" : "dummy..RTMasterVectorTest";
#else
                        SqlBulkCopy bulkCopy = new SqlBulkCopy(DBConnectionVayu);
                        bulkCopy.BulkCopyTimeout = 60 * 10;
                        bulkCopy.DestinationTableName = "Vayu..RTMasterVector";
#endif
                        if (DBConnectionVayu.State == ConnectionState.Closed)
                            DBConnectionVayu.Open();

                        bulkCopy.WriteToServer(ercotVectTab);
                        Console.WriteLine("inserted Vector");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    Console.WriteLine(DateTime.Now + " Outside Bulk Copy Vector");
                    DBConnectionVayu.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private Dictionary<string, double> CalculateSensitivity(List<Congestion> congestionList, double congestionValue, long nodeKey)
        {
            Dictionary<string, double> sensitivityHash = new Dictionary<string, double>(); ;

            using (SqlConnection DBConnectionVayu = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                
                SqlCommand selectSensitivityCommand = new SqlCommand();
                selectSensitivityCommand.CommandText = "select sensitivity from Vayu..RTMasterVector (NOLOCK) where nodekey = @nodekey and constraintrtnum = " +
                                            "(select constraintrtnum from Vayu..RTMasterConstraint where monitoredtext = @monitoredtext and contingencytext = @contingencytext)";
                selectSensitivityCommand.Connection = DBConnectionVayu;
                selectSensitivityCommand.Parameters.AddWithValue("@monitoredtext", "monitoredtext");
                selectSensitivityCommand.Parameters.AddWithValue("@contingencytext", "contingencytext");
                selectSensitivityCommand.Parameters.AddWithValue("@nodekey", "nodekey");
                double sensitivityShadow = 0;
                double shadow = 0;
                string sensKey = null;
                foreach (Congestion congestion in congestionList)
                {
                    bool found = false;
                    if (congestionList.Count > 1)
                    {
                        string key = congestion.ConstraintName + congestion.ContingencyName + nodeKey;
                        if (!mSensitivityHash.ContainsKey(key))
                        {
                            selectSensitivityCommand.Parameters["@monitoredtext"].Value = congestion.ConstraintName;
                            selectSensitivityCommand.Parameters["@contingencytext"].Value = congestion.ContingencyName;
                            selectSensitivityCommand.Parameters["@nodekey"].Value = nodeKey;
                            SqlDataReader reader = selectSensitivityCommand.ExecuteReader();
                            while (reader.Read())
                            {
                                double tempSens = (double)reader.GetDecimal(0);
                                mSensitivityHash.Add(key, tempSens);
                            }
                            reader.Close();
                        }
                        if (mSensitivityHash.ContainsKey(key))
                        {
                            double tempSens = mSensitivityHash[key];
                            sensitivityShadow += (tempSens * congestion.ShadowPrice);
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        sensKey = congestion.ConstraintName + "?" + congestion.ContingencyName;
                        shadow = congestion.ShadowPrice;
                    }
                }
                if (shadow != 0)
                {
                    double sensitivity = (congestionValue - sensitivityShadow) / shadow;
                    sensitivityHash.Add(sensKey, sensitivity);
                }
                DBConnectionVayu.Close();
            }

            return sensitivityHash;
        }

        private Node[] GetErcotCongestions(DateTime startDate)
        {
            List<Node> priceList = new List<Node>();
            try
            {
                using (SqlConnection VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                   
                    SqlCommand cmd = VayuDBConnection.CreateCommand();
                    cmd.CommandText = "select NodeKey , lmp , Congestion  from Vayu..NodeLMPMin where MarketDate = '" + startDate.Date.ToString("yyyy-MM-dd") + "' and MarketHour = " + startDate.Hour.ToString() + " and MarketMin = " + startDate.Minute.ToString(); //+ " and second = " + startDate.Second.ToString();
                    cmd.Connection = VayuDBConnection;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Node node = new Node();
                        node.LmpTimePriceList = new List<LmpTimePrice>();
                        node.NodeId = rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr.GetValue(0));
                        LmpTimePrice timePrice = new LmpTimePrice();
                        timePrice.MarketTime = startDate;
                        timePrice.Lmp = new LMP();
                        // lmp.Price = rdr.IsDBNull(1) ? double.NaN : Convert.ToDouble(rdr.GetValue(1));
                        timePrice.Lmp.Price = rdr.IsDBNull(1) ? double.NaN : Convert.ToDouble(rdr.GetValue(1));
                        //  lmp.Congestion = rdr.IsDBNull(1) ? double.NaN : Convert.ToDouble(rdr.GetValue(2));
                        timePrice.Lmp.Congestion = rdr.IsDBNull(1) ? double.NaN : Convert.ToDouble(rdr.GetValue(2));
                        node.LmpTimePriceList.Add(timePrice);
                        priceList.Add(node);
                    }
                    VayuDBConnection.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Node[] priceArr = priceList.ToArray();
            return priceArr;
        }
        private List<Congestion> GetConstraints(DateTime minCountDate)
        {
            List<Congestion> constraintList = new List<Congestion>();
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
               
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select distinct ConstraintText , ContingencyText , shadowPrice from Vayu..ConstraintRT where MarketDateTime = '" + minCountDate.ToString() + "' and shadowPrice <> 0";

                    cmd.Connection = con;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Congestion cong = new Congestion();
                        cong.ConstraintName = rdr.GetString(0);
                        cong.ContingencyName = rdr.GetString(1);
                        cong.ShadowPrice = Convert.ToInt32(rdr.GetValue(2));
                        constraintList.Add(cong);
                    }
                    rdr.Close();
                }
                con.Close();
            }
            return constraintList;
        }
    }
}
