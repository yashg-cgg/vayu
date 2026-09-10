using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vayu.CommonAccessLibrary;
using Vayu.LatestConstraintsInformationLibrary;

namespace Vayu.HourlyConstraintShadowPrice
{
    public class CalculateHourlyShadowPrice
    {

        List<Constraint> lstConstraintDetails;
        SqlConnection VayuConnection;
        private SqlCommand mGetMaxShadowPriceCommand;
        private Dictionary<string, Dictionary<DateTime, double>> mAvgHash = null;
        public Dictionary<string, double> loadDictHash = new Dictionary<string, double>();
        private SqlCommand mSelectloadDailyCommand;
        private SqlCommand mSelectloadHourlyCommand;
        private SqlCommand mDeleteNodeHErcotCommand;


        public void InitDB()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
        }
        public void CalculateRT()
        {
            DateTime fromDate = DateTime.Today.AddDays(-2);
            for (DateTime date = fromDate; date <= DateTime.Today; date.AddDays(0))
            {
                List<Constraint> tempList = FillAvgHashByService(9, false, date, date.AddDays(1));

                List<Constraint> tempdatalist = CalculateShadowPrices(tempList, 9, false);

                if (tempdatalist.Count > 0)
                {
                    SaveData(tempdatalist, false);
                }
                Console.WriteLine("Data Inserted for" + date);
                date = date.AddDays(1);
            }
        }

        public void CalculateDA()
        {
            DateTime fromDate = DateTime.Today.AddDays(-20);
            for (DateTime date = fromDate; date <= DateTime.Today; date.AddDays(0))
            {
                List<Constraint> tempList = FillAvgHashByService(9, true, date, date.AddDays(1));

                List<Constraint> tempdatalist = CalculateShadowPrices(tempList, 9, true);

                if (tempdatalist.Count > 0)
                {
                    SaveData(tempdatalist, true);
                }
                Console.WriteLine("Data Inserted for" + date);
                date = date.AddDays(1);
            }
        }

        private void SaveData(List<Constraint> list , bool isDa)
        {
            InitDB();
            DataTable dtLmph = new DataTable();
            dtLmph.Columns.Add("MarketDateTime");
            dtLmph.Columns.Add("ConstraintText");
            dtLmph.Columns.Add("ContingencyText");
            dtLmph.Columns.Add("ShadowPrice");
            dtLmph.Columns.Add("MaxShadowPrice");

            foreach (Constraint item in list)
            {
                String Constraint = item.ConstraintText;
                string Contingency = item.ContingencyText;
                double? MaxSP = item.MaxShadowPrice;
                DateTime date = item.ConstraintDate.Date;
                //date = date.AddHours(24);
                if(item.HE1!= null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(1);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE1;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE2 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(2);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE2;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE3 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(3);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE3;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE4 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(4);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE4;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE5 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(5);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE5;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE6 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(6);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE6;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE7 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(7);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE7;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE8 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(8);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE8;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE9 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(9);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE9;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE10 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(10);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE10;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE11 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(11);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE11;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE12 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(12);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE12;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE13 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(13);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE13;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE14 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(14);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE14;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE15 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(15);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE15;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE16 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(16);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE16;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE17 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(17);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE17;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE18 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(18);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE18;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE19 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(19);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE19;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE20 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(20);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE20;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE21 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(21);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE21;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE22 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(22);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE22;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE23 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date.AddHours(23);
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE23;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }
                if (item.HE24 != null)
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["MarketDateTime"] = date;
                    drLmph["ConstraintText"] = Constraint;
                    drLmph["ContingencyText"] = Contingency;
                    drLmph["ShadowPrice"] = item.HE24;
                    drLmph["MaxShadowPrice"] = item.MaxShadowPrice;
                    dtLmph.Rows.Add(drLmph);
                }

            }
            if(dtLmph.Rows.Count>0)
            {

                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                if(isDa)
                {
                    mDeleteNodeHErcotCommand = VayuConnection.CreateCommand();
                    mDeleteNodeHErcotCommand.CommandText = "Truncate table HourlySPConstraintDA_Test";
                    mDeleteNodeHErcotCommand.ExecuteNonQuery();
                    SqlTransaction transaction;
                    transaction = VayuConnection.BeginTransaction();


                    using (SqlBulkCopy bk15Min = new SqlBulkCopy(VayuConnection, SqlBulkCopyOptions.TableLock, transaction))
                    {
                        try
                        {
                            bk15Min.DestinationTableName = "[HourlySPConstraintDA_Test]";
                            bk15Min.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                            bk15Min.ColumnMappings.Add("ConstraintText", "ConstraintText");
                            bk15Min.ColumnMappings.Add("ContingencyText", "ContingencyText");
                            bk15Min.ColumnMappings.Add("ShadowPrice", "ShadowPrice");
                            bk15Min.ColumnMappings.Add("MaxShadowPrice", "MaxShadowPrice");
                            bk15Min.WriteToServer(dtLmph);
                            SqlCommand updateNodeLMP = new SqlCommand("[MergeHourlySPConstraintDA]", VayuConnection, transaction);
                            updateNodeLMP.CommandType = CommandType.StoredProcedure;
                            updateNodeLMP.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            VayuConnection.Close();
                        }
                    }
                }
                else
                {
                    mDeleteNodeHErcotCommand = VayuConnection.CreateCommand();
                    mDeleteNodeHErcotCommand.CommandText = "Truncate table HourlySPConstraintRT_Test";
                    mDeleteNodeHErcotCommand.ExecuteNonQuery();
                    SqlTransaction transaction;
                    transaction = VayuConnection.BeginTransaction();


                    using (SqlBulkCopy bk15Min = new SqlBulkCopy(VayuConnection, SqlBulkCopyOptions.TableLock, transaction))
                    {
                        try
                        {
                            bk15Min.DestinationTableName = "[HourlySPConstraintRT_Test]";
                            bk15Min.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                            bk15Min.ColumnMappings.Add("ConstraintText", "ConstraintText");
                            bk15Min.ColumnMappings.Add("ContingencyText", "ContingencyText");
                            bk15Min.ColumnMappings.Add("ShadowPrice", "ShadowPrice");
                            bk15Min.ColumnMappings.Add("MaxShadowPrice", "MaxShadowPrice");
                            bk15Min.WriteToServer(dtLmph);
                            SqlCommand updateNodeLMP = new SqlCommand("[MergeHourlySPConstraintRT]", VayuConnection, transaction);
                            updateNodeLMP.CommandType = CommandType.StoredProcedure;
                            updateNodeLMP.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            VayuConnection.Close();
                        }
                    }

                }
                

            }

        }

        List<Constraint> CalculateShadowPrices(List<Constraint> tempList, int marketKey, bool isDa)
        {

            foreach (var item in mAvgHash)
            {
                if (item.Key.Contains("CBY_AT3")) //608T608_1/GIDEON-BASTCI/138-138  STEWAR_VERTRE1  CBY_AT3
                {
                }
                try
                {
                    string[] conMon = item.Key.Split(new string[] { "?split?" }, StringSplitOptions.None);
                    var conMonList = tempList.Where(a => a.ConstraintText == conMon[0] && a.ContingencyText == conMon[1]);
                    foreach (DateTime dObj in conMonList.Select(p => p.ConstraintDate.Date).Distinct())
                    {
                        try
                        {
                            List<ConstraintHistoryhelper> lstConstraintHistoryhelpers = new List<ConstraintHistoryhelper>();

                            loadDictHash = new Dictionary<string, double>();
                            var constObjList = conMonList.Where(a => a.ConstraintDate.Date == dObj);

                            if (marketKey == 1)
                            {
                                // if (loadDictHash.Count() == 0)
                                {
                                    List<DateTime> datetimeList = constObjList.Select(x => x.ConstraintDate).ToList<DateTime>();
                                    loadDictHash = GetLoadsData(datetimeList, marketKey);
                                }
                            }
                            if (marketKey == 9)
                            {
                                if (loadDictHash.Count() == 0)
                                {
                                    List<DateTime> datetimeList = constObjList.Select(x => x.ConstraintDate).ToList<DateTime>();
                                    loadDictHash = GetLoadsData(datetimeList, marketKey);
                                }
                            }

                            if (constObjList.Count() == 0)
                                continue;

                            try
                            {
                                Constraint constObj = constObjList.First();
                                string dateCounter = string.Empty;  //DateTime.Now.ToString("yyyy-MM-dd");
                                string dt = string.Empty; //int hrs = 0;
                                if (constObjList.FirstOrDefault() != null)
                                {
                                    dt = constObjList.FirstOrDefault().ConstraintDate.ToString("yyyy-MM-dd");
                                    //  hrs=dt.Hour;
                                    dateCounter = dt; //.Date.AddHours(hrs);
                                }
                                if (!isDa)
                                {
                                    if (marketKey == 9)
                                    {
                                       

                                        Dictionary<int, double> hourlyValues = new Dictionary<int, double>();
                                        Dictionary<int, Dictionary<int, double>> values = new Dictionary<int, Dictionary<int, double>>();
                                        Dictionary<int, List<ErcotConstraintHelper>> hourMinuteDict = new Dictionary<int, List<ErcotConstraintHelper>>();

                                        foreach (var a in item.Value)
                                        {


                                                if (a.Key.Date == dObj)
                                                {
                                                    ErcotConstraintHelper ercotConstraintHelper = new ErcotConstraintHelper();
                                                    ercotConstraintHelper.Minute = a.Key.Minute;
                                                    ercotConstraintHelper.Seconds = a.Key.Second;
                                                    ercotConstraintHelper.ShadowPrice = a.Value;
                                                    ercotConstraintHelper.Hourval = a.Key.Hour;
                                                    ercotConstraintHelper.completedate = new DateTime(a.Key.Date.Year, a.Key.Date.Month, a.Key.Date.Day, a.Key.Hour, a.Key.Minute, a.Key.Second);

                                                    List<ErcotConstraintHelper> lstErcotConstraintHelpers = new List<ErcotConstraintHelper>();
                                                    lstErcotConstraintHelpers.Add(ercotConstraintHelper);

                                                    if (!(hourMinuteDict.ContainsKey(a.Key.Hour)))
                                                    {
                                                        hourMinuteDict.Add(a.Key.Hour, lstErcotConstraintHelpers);
                                                    }
                                                    else
                                                    {
                                                        hourMinuteDict[a.Key.Hour].Add(ercotConstraintHelper);
                                                    }
                                                    //}
                                                }
                                            
                                        }

                                        List<int> hourList = hourMinuteDict.Keys.ToList();


                                        List<string> l1 = new List<string>();
                                        List<string> l2 = new List<string>();
                                        List<string> l3 = new List<string>();
                                        List<string> l4 = new List<string>();

                                        hourList = hourList.OrderByDescending(x => x).ToList();
                                        foreach (var hour in hourList)
                                        {

                                            var hourMinuteCollection = hourMinuteDict[hour];
                                            var hourMinuteCollectionSP = hourMinuteDict[hour];
                                            double interval1, interval2, interval3, interval4;
                                            double spvalue, finalsp;

                                            int currmin, currsec;
                                            string key, allval;

                                            key = allval = "";
                                            spvalue = finalsp = 0;

                                            List<int> minList = new List<int>();
                                            minList.Add(0);
                                            minList.Add(5);
                                            minList.Add(10);
                                            minList.Add(15);
                                            minList.Add(20);
                                            minList.Add(25);
                                            minList.Add(30);
                                            minList.Add(35);
                                            minList.Add(40);
                                            minList.Add(45);
                                            minList.Add(50);
                                            minList.Add(55);
                                            foreach (int a in minList)
                                            {
                                                bool found = false;
                                                for (int i = 0; i < hourMinuteCollectionSP.Count; i++)
                                                {
                                                    if (a == hourMinuteCollectionSP.ElementAt(i).Minute)
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                    {

                                                    }

                                                }
                                                if (!found)
                                                {
                                                    ErcotConstraintHelper obj = new ErcotConstraintHelper();
                                                    obj = new ErcotConstraintHelper();
                                                    obj.Hourval = hour;
                                                    obj.Seconds = 0;
                                                    obj.Minute = a;
                                                    obj.ShadowPrice = 0;
                                                    hourMinuteCollectionSP.Add(obj);
                                                }
                                            }
                                            ErcotConstraintHelper int1Value = new ErcotConstraintHelper();

                                            l1.Clear(); l2.Clear(); l3.Clear(); l4.Clear();
                                            interval1 = interval2 = interval3 = interval4 = 0;


                                            hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.Minute).ToList();
                                            int elementcount = hourMinuteCollectionSP.Count;
                                            int lastmin = hourMinuteCollectionSP.ElementAt(elementcount - 1).Minute;
                                            ErcotConstraintHelper tempobj;

                                            hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.Minute).ToList();

                                            hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.completedate).ToList();
                                            elementcount = hourMinuteCollectionSP.Count;
                                            lastmin = hourMinuteCollectionSP.ElementAt(elementcount - 1).Minute;

                                            while (lastmin < 55)
                                            {
                                                if (lastmin % 5 == 0)
                                                    lastmin = lastmin + 5;
                                                else
                                                {
                                                    do
                                                        lastmin = lastmin + 1;
                                                    while ((lastmin % 5 != 0));
                                                }

                                                tempobj = new ErcotConstraintHelper();
                                                tempobj.Hourval = hour;
                                                tempobj.Seconds = 0;
                                                tempobj.Minute = lastmin;
                                                tempobj.ShadowPrice = 0;
                                                hourMinuteCollectionSP.Add(tempobj);
                                            }

                                            hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.Minute).ToList();
                                            for (int i = 0; i < hourMinuteCollectionSP.Count; i++)
                                            {
                                                int1Value = new ErcotConstraintHelper();
                                                int1Value.Hourval = hourMinuteCollectionSP.ElementAt(i).Hourval;
                                                int1Value.Minute = hourMinuteCollectionSP.ElementAt(i).Minute;
                                                int1Value.Seconds = hourMinuteCollectionSP.ElementAt(i).Seconds;
                                                int1Value.ShadowPrice = hourMinuteCollectionSP.ElementAt(i).ShadowPrice;

                                                currmin = int1Value.Minute;
                                                currsec = int1Value.Seconds;

                                                key = allval = "";
                                                finalsp = 0;


                                                if (i == 0)
                                                {
                                                    key = constObj.ConstraintText + "?split?" + constObj.ContingencyText;
                                                    spvalue = getPreviousMinSP(hour, currmin, currsec, hourMinuteDict, key, dateCounter);

                                                    finalsp = (currmin * 60 + currsec) * spvalue;
                                                    allval = hour + "#" + currmin + "#" + currsec + "?" + (currmin * 60 + currsec) + "#" + spvalue + "#" + finalsp;
                                                    l1.Add(allval);
                                                    interval1 = interval1 + finalsp;
                                                }

                                                int totalsec1, totalsec2, totaltime;
                                                if (i > 0)
                                                {
                                                    if (currmin >= 0 && currmin <= 15)
                                                    {
                                                        if (currmin == 15)
                                                        {
                                                            totalsec1 = currmin * 60;
                                                            totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                            totaltime = totalsec1 - totalsec2;

                                                        }
                                                        else
                                                        {
                                                            totalsec1 = currmin * 60 + currsec;
                                                            totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                            totaltime = totalsec1 - totalsec2;
                                                        }

                                                        spvalue = hourMinuteCollectionSP.ElementAt(i - 1).ShadowPrice;
                                                        finalsp = totaltime * spvalue;
                                                        allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                                        l1.Add(allval);
                                                        interval1 = interval1 + finalsp;

                                                    }
                                                    if (currmin >= 15 && currmin <= 30)
                                                    {
                                                        if (currmin == 30)
                                                        {
                                                            totalsec1 = currmin * 60;
                                                            totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                            totaltime = totalsec1 - totalsec2;

                                                        }
                                                        else
                                                        {
                                                            if (currmin == 15)
                                                            {

                                                                totalsec2 = totalsec1 = 0;
                                                                totaltime = currsec;
                                                            }
                                                            else
                                                            {
                                                                totalsec1 = currmin * 60 + currsec;
                                                                totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                                totaltime = totalsec1 - totalsec2;
                                                            }
                                                        }

                                                        spvalue = hourMinuteCollectionSP.ElementAt(i - 1).ShadowPrice;
                                                        finalsp = totaltime * spvalue;
                                                        allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                                        l2.Add(allval);
                                                        interval2 = interval2 + finalsp;

                                                    }
                                                    if (currmin >= 30 && currmin <= 45)
                                                    {
                                                        if (currmin == 45)
                                                        {
                                                            totalsec1 = currmin * 60;
                                                            totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                            totaltime = totalsec1 - totalsec2;

                                                        }
                                                        else
                                                        {
                                                            if (currmin == 30)
                                                            {
                                                                totalsec2 = totalsec1 = 0;
                                                                totaltime = currsec;
                                                            }
                                                            else
                                                            {
                                                                totalsec1 = currmin * 60 + currsec;
                                                                totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;

                                                                if (totalsec1 > totalsec2)
                                                                    totaltime = totalsec1 - totalsec2;
                                                                else
                                                                    totaltime = totalsec2 - totalsec1;
                                                            }
                                                        }

                                                        spvalue = hourMinuteCollectionSP.ElementAt(i - 1).ShadowPrice;
                                                        finalsp = totaltime * spvalue;
                                                        allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                                        l3.Add(allval);
                                                        interval3 = interval3 + finalsp;

                                                    }


                                                    if (currmin >= 45 && currmin <= 59)
                                                    {
                                                        if (i == hourMinuteCollectionSP.Count - 1)
                                                        {

                                                            totalsec1 = currmin * 60 + currsec;
                                                            totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                            totaltime = totalsec1 - totalsec2;

                                                        }
                                                        else
                                                        {
                                                            if (currmin == 45)
                                                            {

                                                                totalsec2 = totalsec1 = 0;
                                                                totaltime = currsec;
                                                            }
                                                            else
                                                            {
                                                                totalsec1 = currmin * 60 + currsec;
                                                                totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                                totaltime = totalsec1 - totalsec2;
                                                            }
                                                        }

                                                        spvalue = hourMinuteCollectionSP.ElementAt(i - 1).ShadowPrice;
                                                        finalsp = totaltime * spvalue;
                                                        allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                                        l4.Add(allval);
                                                        interval4 = interval4 + finalsp;

                                                        if (i == hourMinuteCollectionSP.Count - 1)
                                                        {

                                                            totalsec1 = currmin * 60 + currsec;
                                                            totalsec2 = 60 * 60;
                                                            totaltime = totalsec2 - totalsec1;

                                                            spvalue = hourMinuteCollectionSP.ElementAt(i).ShadowPrice;
                                                            finalsp = totaltime * spvalue;
                                                            allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                                            l4.Add(allval);
                                                            interval4 = interval4 + finalsp;

                                                        }

                                                    }
                                                }

                                            }

                                            interval1 = interval1 / 900;
                                            interval2 = interval2 / 900;
                                            interval3 = interval3 / 900;
                                            interval4 = interval4 / 900;
                                            spvalue = (interval1 + interval2 + interval3 + interval4) / 4;

                                            ConstraintHistoryhelper objConstraintHistoryhelper = new ConstraintHistoryhelper();
                                            objConstraintHistoryhelper.ConstraintName = constObj.ConstraintText;
                                            objConstraintHistoryhelper.ContingencyName = constObj.ContingencyText;
                                            objConstraintHistoryhelper.date = constObj.ConstraintDate;
                                            objConstraintHistoryhelper.hour = hour;
                                            objConstraintHistoryhelper.shadowprice = spvalue;

                                            lstConstraintHistoryhelpers.Add(objConstraintHistoryhelper);
                                        }

                                        foreach (var annItem in lstConstraintHistoryhelpers)
                                        {
                                            try
                                            {
                                                constObj.GetType().GetProperty("HE" + (annItem.hour == 0 ? "1" : (annItem.hour + 1).ToString())).SetValue(constObj, annItem.shadowprice == 0 ? 0 : annItem.shadowprice);
                                            }
                                            catch
                                            {
                                            }
                                        }

                                        constObj.Price = lstConstraintHistoryhelpers.Sum(a => a.shadowprice) / 24;
                                    }
                                    else
                                    {
                                        var data = from a in item.Value
                                                   where isDa ? (a.Key >= dObj && a.Key < dObj.AddDays(1)) : a.Key.Date == dObj
                                                   group a by a.Key.Hour into g
                                                   select new { time = g.Key, val = g.Sum(o => o.Value) / (isDa ? 1 : 12) }; // change the count here

                                        foreach (var annItem in data)
                                        {
                                            try
                                            {
                                                constObj.GetType().GetProperty("HE" + (annItem.time == 0 ? "1" : (annItem.time + 1).ToString())).SetValue(constObj, annItem.val == 0 ? (double?)null : annItem.val);
                                            }
                                            catch
                                            {
                                            }
                                        }

                                        constObj.Price = data.Sum(a => a.val) / 24;
                                    }

                                }
                                else
                                {
                                    {
                                        Dictionary<DateTime, double> shadowPriceDict = item.Value;
                                        foreach (var item1 in shadowPriceDict)
                                        {
                                            if (dObj == item1.Key.Date)
                                                constObj.GetType().GetProperty("HE" + (item1.Key.Hour + 1).ToString()).SetValue(constObj, item1.Value == 0 ? (double?)null : item1.Value);
                                        }
                                    }
                                    constObj.Price = (item.Value.Sum(a => a.Value)) / 24;
                                }
                                if (loadDictHash != null)
                                {
                                    if (loadDictHash.ContainsKey(dateCounter))
                                        constObj.MaxLoad = loadDictHash[dateCounter];
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                catch
                {

                }
            }
            List<Constraint> tempdatalist = tempList.Distinct().ToList();

            return tempdatalist;
        }

        public List<Constraint> FillAvgHashByService(int marketKey, bool isDa, DateTime fromDate, DateTime? throDate)
        {

            List<Constraint> tempList = new List<Constraint>();
            ConstraintHelper helper = new ConstraintHelper();
            IConstraintInfoProvider chelper = helper.GetInstance();
            List<LatestConstraint> constraintList = null;
            
            throDate = (throDate.HasValue ? throDate.Value.AddDays(1) : fromDate.AddDays(1));
            if (!isDa)
                //dictMaxprice = GetMaxPrice(marketKey, fromDate, throDate);

                lstConstraintDetails = GetMaxPrice(marketKey);
            if (isDa)
                fromDate = fromDate.AddHours(1);

            if (isDa)
                constraintList = chelper.GetConstraintDA(marketKey, Configuration.GetAbsoluteDate(fromDate), Configuration.GetAbsoluteDate(throDate.Value));
            else
                constraintList = chelper.GetConstraintRT(marketKey, Configuration.GetAbsoluteDate(fromDate), Configuration.GetAbsoluteDate(throDate.Value), false);

            mAvgHash = new Dictionary<string, Dictionary<DateTime, double>>();
            foreach (var item in constraintList)
            {
                try
                {

                    string SourceZonename = string.Empty, SinkZonename = string.Empty;
                    string constraint = item.ConstraintText;
                    string contingency = item.ContigencyText;
                    
                    double? maxShadowPrice = 0;
                    double? maxRTShadowPrice = 0;

                    DateTime mktDate;
                    if (isDa)
                        mktDate = item.MarketDate.AddHours(-1);
                    else
                        mktDate = item.MarketDate;
                    double prc = item.ShadowPrice;
                    string keyText = constraint + "?split?" + contingency;
                    int sourceNodekey = item.SourceNodeKey;
                    int sinkNodekey = item.SinkNodeKey;
                    string constName = constraint + "?" + contingency;

                        if (isDa)
                        {
                            maxShadowPrice = 0;
                            maxRTShadowPrice = 0;
                        }
                        else
                        {


                            if (lstConstraintDetails != null)
                            {
                                if (lstConstraintDetails.Count != 0)
                                {
                                    maxShadowPrice = lstConstraintDetails.Find(x => x.ConstraintText == constraint && x.ContingencyText == contingency).MaxShadowPrice;
                                    maxRTShadowPrice = lstConstraintDetails.Find(x => x.ConstraintText == constraint && x.ContingencyText == contingency).MaxRTShadowPrice;
                                }
                            }
                        }
                    
                    Constraint constraintItem = tempList.Where(a => a.ConstraintText == constraint && a.ContingencyText == contingency && a.ConstraintDate.Date == mktDate.Date).FirstOrDefault();
                    if (constraintItem == null)
                    {
                        constraintItem = new Constraint
                        {
                            ConstraintDate = mktDate,
                            ContingencyText = contingency,
                            ConstraintText = constraint,
                            SourceNodekey = sourceNodekey,
                            SinkNodekey = sinkNodekey,
                            SourceZone = SourceZonename,
                            SinkZone = SinkZonename,
                            MaxShadowPrice = maxShadowPrice,
                            MaxRTShadowPrice = maxRTShadowPrice
                        };
                    }
                    if (mAvgHash.ContainsKey(keyText))
                    {
                        Dictionary<DateTime, double> hourValues = mAvgHash[keyText];
                        if (hourValues.ContainsKey(mktDate))
                        {
                            if (!isDa)
                            {
                                if (!double.IsNaN(item.ShadowPriceNaN))
                                    hourValues[mktDate] += hourValues[mktDate] + Math.Abs(item.ShadowPrice);
                            }
                        }
                        else
                        {
                            hourValues.Add(mktDate, double.IsNaN(item.ShadowPriceNaN) ? 0 : Math.Abs(item.ShadowPrice));
                        }
                    }
                    else
                    {
                        Dictionary<DateTime, double> tempHash = new Dictionary<DateTime, double>();
                        tempHash.Add(mktDate, double.IsNaN(item.ShadowPriceNaN) ? 0 : Math.Abs(item.ShadowPrice));
                        mAvgHash.Add(keyText, tempHash);
                    }
                    if (tempList.Contains(constraintItem))
                        tempList.Remove(constraintItem);
                    tempList.Add(constraintItem);
                }
                catch
                {
                    continue;
                }
            }
            return tempList;
        }

        internal Dictionary<string, double> GetLoadsData(List<DateTime> datetimeList, int Marketkey)
        {
            try
            {
                Dictionary<string, double> loadDictHash = new Dictionary<string, double>();
                using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    //List<DateTime> temiList = datetimeList.GroupBy(x => x.ToString("dd-MM-yyyy"));
                    foreach (DateTime date in datetimeList)
                    {
                        double load = 0; string mdate; string sdate = string.Empty;
                        int hr = 12;
                        if (!loadDictHash.ContainsKey(date.AddDays(-1).Date.ToString("dd-MM-yyyy")))
                        {
                            if (Marketkey == 1)
                            {
                                mSelectloadDailyCommand = new SqlCommand();
                                if (VayuConnection.State == ConnectionState.Closed)
                                {
                                    VayuConnection.Open();
                                }
                                mSelectloadDailyCommand.CommandText = "select CAST(MarketDateTime as date) date , max(mw) from NewTrading.. LoadRT where loadskey=25 and   MarketDateTime >= @Startdate and MarketDateTime < @endDate " +
                                                                    " group by CAST(MarketDateTime as date) order by date";
                                mSelectloadDailyCommand.Parameters.AddWithValue("@Startdate", date.Date);
                                mSelectloadDailyCommand.Parameters.AddWithValue("@endDate", date.Date.AddDays(1));
                                mSelectloadDailyCommand.Connection = VayuConnection;
                                SqlDataReader reader = mSelectloadDailyCommand.ExecuteReader();
                                while (reader.Read())
                                {
                                    load = reader.IsDBNull(1) ? 0.0 : Convert.ToDouble(reader.GetValue(1));
                                    // mdate = Convert.ToString(reader.GetValue(0));
                                    //loadDictHash.Add(mdate, load);
                                }
                                reader.Close();
                                VayuConnection.Close();
                                #region Load Zero
                                if (load == 0)
                                {
                                    mSelectloadHourlyCommand = new SqlCommand();
                                    mSelectloadHourlyCommand.CommandText = "select  max(MW) from LoadRTH where LoadsKey = 25 and MarketDateTime >=  @StartDate and MarketDateTime < @EndDate";
                                    mSelectloadHourlyCommand.Parameters.AddWithValue("@StartDate", date.Date.AddDays(-1));
                                    mSelectloadHourlyCommand.Parameters.AddWithValue("@EndDate", date.Date);
                                    mSelectloadHourlyCommand.Connection = VayuConnection;
                                    SqlDataReader reader1 = mSelectloadHourlyCommand.ExecuteReader();
                                    while (reader1.Read())
                                    {
                                        load = reader1.IsDBNull(0) ? 0.0 : Convert.ToDouble(reader1.GetValue(0));
                                    }
                                    reader1.Close();
                                }
                                #endregion Load Zero
                            }
                            else //For Ercot
                            {
                                mSelectloadDailyCommand = new SqlCommand();
                                if (VayuConnection.State == ConnectionState.Closed)
                                {
                                    VayuConnection.Open();
                                }
                                mSelectloadDailyCommand.CommandText = "select CAST(MarketDateTime as date) date , max(mw) from  Vayu..LoadRT where loadskey=2213 and   MarketDateTime >= @Startdate and MarketDateTime < @endDate " +
                                                                      " group by CAST(MarketDateTime as date) order by date";
                                mSelectloadDailyCommand.Parameters.AddWithValue("@Startdate", date.Date);
                                mSelectloadDailyCommand.Parameters.AddWithValue("@endDate", date.Date.AddDays(1));
                                mSelectloadDailyCommand.Connection = VayuConnection;
                                SqlDataReader reader = mSelectloadDailyCommand.ExecuteReader();
                                while (reader.Read())
                                {
                                    load = reader.IsDBNull(1) ? 0.0 : Convert.ToDouble(reader.GetValue(1));
                                    // mdate = reader.GetDateTime(0).ToString("yyyy-MM-dd");
                                    // loadDictHash.Add(mdate, load);
                                }
                                reader.Close();
                                VayuConnection.Close();
                                #region Load Zero
                                if (load == 0)
                                {
                                    mSelectloadHourlyCommand = new SqlCommand();
                                    mSelectloadHourlyCommand.CommandText = "select  max(MW) from  Vayu..LoadRT where LoadsKey = 2213 and MarketDateTime >=  @StartDate and MarketDateTime < @EndDate";//LoadRTH 
                                    mSelectloadHourlyCommand.Parameters.AddWithValue("@StartDate", date.Date.AddDays(-1));
                                    mSelectloadHourlyCommand.Parameters.AddWithValue("@EndDate", date.Date);
                                    mSelectloadHourlyCommand.Connection = VayuConnection;
                                    SqlDataReader reader1 = mSelectloadHourlyCommand.ExecuteReader();
                                    while (reader1.Read())
                                    {
                                        load = reader1.IsDBNull(0) ? 0.0 : Convert.ToDouble(reader1.GetValue(0));
                                    }
                                    reader1.Close();
                                }
                                #endregion Load Zero
                            }
                            loadDictHash.Add(date.ToString("yyyy-MM-dd"), load);
                        }
                    }
                }

                return loadDictHash;





            }
            catch (Exception)
            {
                return null;
                //throw;
            }
        }

        public List<Constraint> GetMaxPrice(int marketkey)
        {
            List<Constraint> lstConstraints = new List<Constraint>();

            try
            {
                using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    mGetMaxShadowPriceCommand = VayuConnection.CreateCommand();
                    if (VayuConnection.State == ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    if (marketkey == 9)
                    {
                        mGetMaxShadowPriceCommand.CommandText = " select distinct c.ConstraintText, c.ContingencyText, MAX(MaxShadowPrice) MaxShadowPrice, MAX(ShadowPrice) MaxRTShadowPrice "
                            + " from Vayu..[ConstraintRT] c  group by c.ConstraintText, c.ContingencyText ";
                    }

                    SqlDataReader reader = mGetMaxShadowPriceCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        lstConstraints.Add(new Constraint
                        {
                            ConstraintText = Convert.ToString(reader.GetString(0)),
                            ContingencyText = reader.IsDBNull(1) ? null : Convert.ToString(reader.GetString(1)),
                            MaxShadowPrice = reader.IsDBNull(2) ? 0 : Convert.ToDouble(reader.GetDecimal(2)),
                            MaxRTShadowPrice = reader.IsDBNull(2) ? 0 : Convert.ToDouble(reader.GetDecimal(3))
                        });
                    }
                    reader.Close();
                    VayuConnection.Close();
                }
            }
            catch (Exception ex)
            {

            }
            return lstConstraints;
        }

        double getPreviousMinSP(int hour, int min, int sec, Dictionary<int, List<ErcotConstraintHelper>> TotalhourMinuteDict, string key, string dateString)
        {
            double sp = 0;
            List<ErcotConstraintHelper> prevHourMinCollectionSP = null;

            try
            {

                if (hour == 0)
                {
                    var item2 = mAvgHash[key];

                    CultureInfo provider = CultureInfo.InvariantCulture;
                    DateTime dateTimeValue = Convert.ToDateTime(dateString);
                    dateTimeValue = dateTimeValue.AddMinutes(-5);
                    List<ErcotConstraintHelper> valueliest = new List<ErcotConstraintHelper>();
                    ErcotConstraintHelper obj;
                    foreach (var keyitem in item2)
                    {

                        if (keyitem.Key.Date.Equals(dateTimeValue.Date))
                        {
                            if (keyitem.Key.Hour == 23 && keyitem.Key.Minute > 54)
                            {
                                obj = new ErcotConstraintHelper();
                                obj.Hourval = 23;
                                obj.Minute = keyitem.Key.Minute;
                                obj.Seconds = keyitem.Key.Second;
                                obj.ShadowPrice = keyitem.Value;
                                valueliest.Add(obj);
                            }
                        }
                    }


                    valueliest = valueliest.OrderByDescending(x => x.Minute).ToList();

                    if (valueliest.Count > 0)
                        sp = valueliest.ElementAt(0).ShadowPrice;

                }
                if (TotalhourMinuteDict.ContainsKey(hour - 1))
                {
                    prevHourMinCollectionSP = TotalhourMinuteDict[hour - 1];
                    prevHourMinCollectionSP = prevHourMinCollectionSP.OrderBy(x => x.Minute).ToList();
                    int lastindex = prevHourMinCollectionSP.Count - 1;

                    sp = prevHourMinCollectionSP.ElementAt(lastindex).ShadowPrice;

                }




            }
            catch (Exception ae)
            { }


            return sp;
        }

        class ConstraintHistoryhelper
        {
            public string ConstraintName { get; set; }
            public string ContingencyName { get; set; }
            public DateTime date { get; set; }
            public int hour { get; set; }
            public double shadowprice { get; set; }
        }

        internal class ErcotConstraintHelper
        {
            public int Minute { get; set; }
            public int Seconds { get; set; }
            public double ShadowPrice { get; set; }
            public int Hourval { get; set; }
            public DateTime completedate { get; set; }
        }

    }
}
