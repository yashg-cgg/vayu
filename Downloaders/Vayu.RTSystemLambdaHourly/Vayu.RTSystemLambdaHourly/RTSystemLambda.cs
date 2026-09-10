using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Xml;
using Vayu.CommonAccessLibrary;

namespace Vayu.RTSystemLambdaHourly
{
    class RTSystemLambda
    {
        SqlConnection VayuDbConn;
        private X509Certificate2 mCert = new X509Certificate2();
        CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        private SqlCommand mDeleteNodeHErcotCommand;
        DataTable mERCOTRTSystemLambdaDT;
        DataRow mERCOTRTSystemLambdaDT1;

        Dictionary<string, List<ErcotEnergyPriceHelper>> hourMinListDic;
        private Dictionary<string, List<ErcotEnergyPriceHelper>> AllhourMinListDicEP = new Dictionary<string, List<ErcotEnergyPriceHelper>>();
        Dictionary<string, List<ErcotEnergyPriceHelper>> PrevDayhourMinListDic;
        

        private SqlCommand mSelectloadDailyCommand;
        private SqlCommand mSelectloadHourlyCommand;

        [Obsolete]
        public RTSystemLambda()
        {
            InitDB();

            GetEneryPrice(DateTime.Today.AddDays(-4), DateTime.Today, false);
        }


        public void InitDB()
        {
             VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();

            mDeleteNodeHErcotCommand = new SqlCommand();
            // mDeleteNodeHErcotCommand.CommandText = "Truncate table NodeDALMPHTemp";
            mDeleteNodeHErcotCommand.CommandText = "Truncate table Vayu..RTSystemLambdaHourlyTest";
            mDeleteNodeHErcotCommand.Connection = VayuDbConn;


        }
        public void InitCert()
        {
            mCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, X509KeyStorageFlags.MachineKeySet);
        }

        public void GetEneryPrice(DateTime fromDate, DateTime toDate, bool isDA)
        {
            hourMinListDic = new Dictionary<string, List<ErcotEnergyPriceHelper>>();
            PrevDayhourMinListDic = new Dictionary<string, List<ErcotEnergyPriceHelper>>();
            DateTime Fromdate = fromDate;
            DateTime Todate = toDate;//.AddDays(1);

            List<DateTime> datetimeList = new List<DateTime>();

            if ((DateTime.Now - toDate).TotalDays < 1)
            {
                Todate = toDate.AddDays(1);

            }
            else
            {
                Todate = Todate.AddDays(1);
                Todate = Todate.AddMinutes(-1);

            }
            List<Constraint> ConstraintHistoryHash = new List<Constraint>();

            SqlCommand sqlCommandEnergyPrice = new SqlCommand();

            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
            if (VayuDbConn.State == ConnectionState.Closed)
            {
                VayuDbConn.Open();
            }

            if (isDA)
            {

                sqlCommandEnergyPrice.CommandText = "select   CASE  when  FORMAT(DeliveryDate,'HH')=0 then DATEADD(day, -1, DeliveryDate) ELSE DeliveryDate END AS DeliveryDate, " +
                        "DAY(DeliveryDate),FORMAT(DeliveryDate,'HH')  ,FORMAT(DeliveryDate,'mm'),FORMAT(DeliveryDate,'ss'),SystemLambda " +
                        "from  Vayu..DAMSystemLambda where DeliveryDate>@Startdate and DeliveryDate <=@endDate order by DeliveryDate desc ";

            }
            else
            {

                sqlCommandEnergyPrice.CommandText = " select  DeliveryDate, DAY(DeliveryDate),FORMAT(DeliveryDate,'HH'),FORMAT(DeliveryDate,'mm'),FORMAT(DeliveryDate,'ss'),SystemLambda from  Vayu..RTSystemLambda where  " +
                                                    "DeliveryDate>=@Startdate and DeliveryDate <=@endDate order by DeliveryDate desc";
            }

            if (isDA)
            {
                sqlCommandEnergyPrice.Parameters.AddWithValue("@Startdate", Fromdate);
            }
            else
            {
                sqlCommandEnergyPrice.Parameters.AddWithValue("@Startdate", Fromdate.AddMinutes(-5));
            }

            sqlCommandEnergyPrice.Parameters.AddWithValue("@endDate", Todate);
            sqlCommandEnergyPrice.Connection = VayuDbConn;
            SqlDataReader reader = sqlCommandEnergyPrice.ExecuteReader();
            while (reader.Read())
            {

                string mdate = reader.GetDateTime(0).ToString("yyyy-MM-dd");
                DateTime currentDateTime = reader.GetDateTime(0);
                string day = Convert.ToInt32(reader.GetValue(1)).ToString();
                int hour = Convert.ToInt32(reader.GetValue(2));
                int min = Convert.ToInt32(reader.GetValue(3));
                int sec = Convert.ToInt32(reader.GetValue(4));

                string key = mdate + "?" + hour;


                double energyprice = reader.IsDBNull(5) ? 0.0 : Convert.ToDouble(reader.GetValue(5));
                ErcotEnergyPriceHelper helper = new ErcotEnergyPriceHelper();
                //  ErcotEnergyPriceHelper prevDayhelper = new ErcotEnergyPriceHelper();
                helper.Hour = hour;
                helper.Minute = min;
                helper.Seconds = sec;
                helper.EnergyPrice = energyprice;
                helper.EPMarketDateTime = reader.GetDateTime(0);

                DateTime Previousday = Fromdate.AddMinutes(-5);
                List<ErcotEnergyPriceHelper> lstErcotEnergyPricetHelper = new List<ErcotEnergyPriceHelper>();
                lstErcotEnergyPricetHelper.Add(helper);

                if (Previousday.Date.Equals(currentDateTime.Date))
                {
                    if (!isDA)//RT
                    {
                        if (!PrevDayhourMinListDic.ContainsKey(key))
                        {
                            PrevDayhourMinListDic.Add(key, lstErcotEnergyPricetHelper);
                        }
                        else
                        {
                            PrevDayhourMinListDic[key].Add(helper);
                        }
                    }
                    else
                    {

                        if (!hourMinListDic.ContainsKey(key))
                        {
                            hourMinListDic.Add(key, lstErcotEnergyPricetHelper);
                        }
                        else
                        {
                            hourMinListDic[key].Add(helper);
                        }
                    }

                }

                else
                {


                    if (!hourMinListDic.ContainsKey(key))
                    {
                        hourMinListDic.Add(key, lstErcotEnergyPricetHelper);
                    }
                    else
                    {
                        hourMinListDic[key].Add(helper);
                    }

                    if (hour == 23 && min >= 54)
                    {
                        if (!isDA)//RT
                        {
                            if (!PrevDayhourMinListDic.ContainsKey(key))
                            {
                                PrevDayhourMinListDic.Add(key, lstErcotEnergyPricetHelper);
                            }
                            else
                            {
                                PrevDayhourMinListDic[key].Add(helper);
                            }
                        }
                    }
                }
            }
            reader.Close();
            AllhourMinListDicEP = new Dictionary<string, List<ErcotEnergyPriceHelper>>();
            AllhourMinListDicEP = hourMinListDic;
            List<string> DatehourList = hourMinListDic.Keys.ToList();
            var dic = new Dictionary<string, List<ErcotEnergyPriceHelper>>();

            Dictionary<string, int> DatehourDic = new Dictionary<string, int>();
            foreach (string val in DatehourList)
            {
                string[] valarrya = val.Split('?');
                if (valarrya[0] == "0")
                { valarrya[0].Insert(0, toDate.ToString()); }

                int hr = Convert.ToInt32(valarrya[1]);

                if (!DatehourDic.ContainsKey(valarrya[0]))
                    DatehourDic.Add(valarrya[0], hr);

            }

            foreach (var Datevalue in DatehourDic)//mERCOTRTSystemLambdaDT
            {
                mERCOTRTSystemLambdaDT = new DataTable();
                mERCOTRTSystemLambdaDT.Columns.Add("DeliveryDate", typeof(DateTime));
                mERCOTRTSystemLambdaDT.Columns.Add("SystemLambda", typeof(decimal));
                List<int> hourList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 0 };
                Dictionary<DateTime, double> EPhourlist = new Dictionary<DateTime, double>();
                
                foreach (var hour in hourList)
                {
                    double FinalEnergyPrice = 0;
                    string key = Datevalue.Key + "?" + hour;

                    if (hourMinListDic.ContainsKey(key))
                    {
                        var hourMinuteCollection = hourMinListDic[key];

                        Dictionary<int, double> AllMinDictonary = new Dictionary<int, double>();
                        double SumallEP = 0;
                        DateTime loopDate = DateTime.Parse(Datevalue.Key);
                        DateTime date = new DateTime(loopDate.Year, loopDate.Month, loopDate.Day,hour,00,00);

                        if (isDA)
                            FinalEnergyPrice = hourMinuteCollection[0].EnergyPrice;
                        else
                            FinalEnergyPrice = CalculateEnergyPrice(loopDate, hourMinuteCollection, PrevDayhourMinListDic);

                        EPhourlist.Add(date, FinalEnergyPrice);
                    }
                   
                    //foreach (DateTime date in EPhourlist.Keys)
                    //{
                    //    row["DeliveryDate"] = date;
                    //    row["SystemLambda"] = EPhourlist[date];
                    //    mERCOTRTSystemLambdaDT.Rows.Add(row);
                    //}

                }
                if(EPhourlist.Count!=0)
                {
                    //DataRow row = mERCOTRTSystemLambdaDT.NewRow();
                    foreach (DateTime date in EPhourlist.Keys)
                    {
                        DataRow row = mERCOTRTSystemLambdaDT.NewRow();
                        row["DeliveryDate"] = date;
                        row["SystemLambda"] = EPhourlist[date];
                        mERCOTRTSystemLambdaDT.Rows.Add(row);
                    }

                }
                if (VayuDbConn.State == ConnectionState.Open)
                {
                    VayuDbConn.Close();
                }
                VayuDbConn.Open();
                mDeleteNodeHErcotCommand.Connection = VayuDbConn;
                mDeleteNodeHErcotCommand.ExecuteNonQuery();
                SqlTransaction transaction = VayuDbConn.BeginTransaction();
                using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
                {
                    try
                    {
                        bkLmpH.DestinationTableName = "[dbo].[RTSystemLambdaHourlyTest]";
                        bkLmpH.ColumnMappings.Add("DeliveryDate", "DeliveryDate");
                        bkLmpH.ColumnMappings.Add("SystemLambda", "SystemLambda");
                        bkLmpH.WriteToServer(mERCOTRTSystemLambdaDT);
                        SqlCommand updateNodeLmpMin = new SqlCommand("[UpMergeRTSystemLambdaHourly]", VayuDbConn, transaction);
                        updateNodeLmpMin.CommandType = CommandType.StoredProcedure;
                        updateNodeLmpMin.CommandTimeout = 300000;
                        updateNodeLmpMin.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                    }
                }
                string loadDicKey = DateTime.Parse(Datevalue.Key).ToString("yyyy-MM-dd");
                datetimeList = new List<DateTime>();
                datetimeList.Add(Convert.ToDateTime(loadDicKey).AddDays(1));
                


            }//adate list


           // return ConstraintHistoryHash;
        }

        double CalculateEnergyPrice(DateTime MarketDateTime, List<ErcotEnergyPriceHelper> hourMinuteCollection, Dictionary<string, List<ErcotEnergyPriceHelper>> PreviousDayCollection)
        {
            double FinalEPVal = 0;
            List<string> l1 = new List<string>();
            List<string> l2 = new List<string>();
            List<string> l3 = new List<string>();
            List<string> l4 = new List<string>();

            List<ErcotEnergyPriceHelper> hourMinuteCollectionSP;
            hourMinuteCollectionSP = hourMinuteCollection;
            ErcotEnergyPriceHelper int1Value = new ErcotEnergyPriceHelper();
            try
            {

                l1.Clear(); l2.Clear(); l3.Clear(); l4.Clear();
                double interval1, interval2, interval3, interval4, spvalue;
                string key = "", allval = ""; ;
                double finalsp = 0;
                interval1 = interval2 = interval3 = interval4 = spvalue = 0;
                hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.Minute).ToList();
                hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.EPMarketDateTime).ToList();

                ErcotEnergyPriceHelper tempobj = new ErcotEnergyPriceHelper();

                int elementcount = hourMinuteCollectionSP.Count;
                int Hourval = hourMinuteCollectionSP.ElementAt(elementcount - 1).Hour;
                int lastmin = hourMinuteCollectionSP.ElementAt(elementcount - 1).Minute;
                //if (lastmin <= 55)
                //{
                //    List<string> MissingPrintList = getMissingPrints(MarketDateTime, Hourval, lastmin);

                //    int cnt = 0;
                //    while (cnt < MissingPrintList.Count)
                //    {
                //        string[] valuearray = MissingPrintList.ElementAt(cnt).Split('#');
                //        tempobj = new ErcotEnergyPriceHelper();
                //       // tempobj.EPMarketDateTime = new DateTime(MarketDateTime.Year, MarketDateTime.Month, MarketDateTime.Day, Hourval, Int32.Parse(valuearray[0]), Int32.Parse(valuearray[1]));
                //        tempobj.Hour = Hourval;
                //        tempobj.Minute = Int32.Parse(valuearray[0]);
                //        tempobj.Seconds = Int32.Parse(valuearray[1]);
                //        tempobj.EnergyPrice = 0;

                //        if (lastmin == 55)
                //            tempobj.Hour = Hourval + 1;
                //        else
                //            tempobj.Hour = Hourval;

                //        tempobj.EPMarketDateTime = new DateTime(MarketDateTime.Year, MarketDateTime.Month, MarketDateTime.Day, tempobj.Hour, Int32.Parse(valuearray[0]), Int32.Parse(valuearray[1]));

                //        hourMinuteCollectionSP.Add(tempobj);
                //        cnt++;
                //    }

                //}
                hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.Minute).ToList();
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

                    tempobj = new ErcotEnergyPriceHelper();
                    tempobj.EPMarketDateTime = new DateTime(MarketDateTime.Year, MarketDateTime.Month, MarketDateTime.Day, Hourval, lastmin, 0);
                    tempobj.Hour = Hourval;
                    tempobj.Seconds = 0;
                    tempobj.Minute = lastmin;
                    tempobj.EnergyPrice = 0;
                    hourMinuteCollectionSP.Add(tempobj);

                }

                hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.Minute).ToList();
                hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.EPMarketDateTime).ToList();
                for (int i = 0; i < hourMinuteCollectionSP.Count; i++)
                {

                    int1Value = new ErcotEnergyPriceHelper();
                    int1Value.Hour = hourMinuteCollectionSP.ElementAt(i).Hour;
                    int1Value.Minute = hourMinuteCollectionSP.ElementAt(i).Minute;
                    int1Value.Seconds = hourMinuteCollectionSP.ElementAt(i).Seconds;
                    int1Value.EnergyPrice = hourMinuteCollectionSP.ElementAt(i).EnergyPrice;

                    int currmin = int1Value.Minute;
                    int currsec = int1Value.Seconds;
                    int currhour = int1Value.Hour;

                    key = allval = ""; ;
                    finalsp = 0;
                    if (i == 0)
                    {
                        spvalue = getPreviousMinEnergyPrice(MarketDateTime, Hourval, currmin, currsec, PreviousDayCollection);

                        finalsp = (currmin * 60 + currsec) * spvalue;
                        allval = currhour + "#" + currmin + "#" + currsec + "?" + (currmin * 60 + currsec) + "#" + spvalue + "#" + finalsp;
                        l1.Add(allval);
                        interval1 = interval1 + finalsp;

                    }

                    int totalsec1, totalsec2, totaltime;
                    if (i > 0)
                    {
                        if (currmin > 0 && currmin <= 15)
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
                            spvalue = hourMinuteCollectionSP.ElementAt(i - 1).EnergyPrice;
                            finalsp = totaltime * spvalue;
                            allval = currhour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;

                            Console.WriteLine("---------------------------");
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

                            spvalue = hourMinuteCollectionSP.ElementAt(i - 1).EnergyPrice;
                            finalsp = totaltime * spvalue;
                            allval = currhour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
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
                                    totaltime = totalsec1 - totalsec2;
                                }
                            }

                            spvalue = hourMinuteCollectionSP.ElementAt(i - 1).EnergyPrice;
                            finalsp = totaltime * spvalue;
                            allval = currhour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                            l3.Add(allval);
                            interval3 = interval3 + finalsp;

                        }


                        if ((currmin >= 45 && currmin <= 59) || (currmin == 0))
                        {
                            if (i == hourMinuteCollectionSP.Count - 1)
                            {

                                if (currhour == hourMinuteCollectionSP.ElementAt(i - 1).Hour + 1)
                                {
                                    totalsec1 = 60 * 60;
                                    totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                    totaltime = totalsec1 - totalsec2;
                                }

                                else
                                {
                                    totalsec1 = currmin * 60 + currsec;
                                    totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                    totaltime = totalsec1 - totalsec2;
                                }

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

                            spvalue = hourMinuteCollectionSP.ElementAt(i - 1).EnergyPrice;
                            finalsp = totaltime * spvalue;
                            allval = currhour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                            l4.Add(allval);
                            interval4 = interval4 + finalsp;

                            if (i == hourMinuteCollectionSP.Count - 1)
                            {

                                totalsec1 = currmin * 60 + currsec;
                                totalsec2 = 60 * 60;
                                totaltime = totalsec2 - totalsec1;

                                spvalue = hourMinuteCollectionSP.ElementAt(i).EnergyPrice;
                                finalsp = totaltime * spvalue;
                                allval = currhour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                l4.Add(allval);
                                interval4 = interval4 + finalsp;

                                Console.WriteLine(currhour + "<br>");
                                Console.WriteLine(l1 + "<br>");
                                Console.WriteLine(l2 + "<br>");
                                Console.WriteLine(l3 + "<br>");
                                Console.WriteLine(l4 + "<br>");



                            }

                        }
                    }

                }
                interval1 = interval1 / 900;
                interval2 = interval2 / 900;
                interval3 = interval3 / 900;
                interval4 = interval4 / 900;

                FinalEPVal = (interval1 + interval2 + interval3 + interval4) / 4;

            }
            catch (Exception e)
            {


            }

            return FinalEPVal;
        }

        double getPreviousMinEnergyPrice(DateTime CurrentDate, int hour, int min, int sec, Dictionary<string, List<ErcotEnergyPriceHelper>> PrevDayCollectionEP)
        {
            double sp = 0;
            List<ErcotEnergyPriceHelper> prevHourMinCollectionSP = null;

            string key = "";
            DateTime dt = CurrentDate;

            if (hour == 0)
            {
                dt = dt.AddMinutes(-5);
                key = dt.ToString("yyyy-MM-dd") + "?23";
                var item2 = PrevDayCollectionEP[key];

                List<ErcotEnergyPriceHelper> valueliest = new List<ErcotEnergyPriceHelper>();
                ErcotEnergyPriceHelper obj;
                foreach (var keyitem in item2)
                {


                    if (keyitem.Hour == 23 && keyitem.Minute > 54)
                    {
                        obj = new ErcotEnergyPriceHelper();
                        obj.Hour = 23;
                        obj.Minute = keyitem.Minute;
                        obj.Seconds = keyitem.Seconds;
                        obj.EnergyPrice = keyitem.EnergyPrice;
                        valueliest.Add(obj);
                    }

                }


                valueliest = valueliest.OrderByDescending(x => x.Minute).ToList();

                if (valueliest.Count > 0)
                    sp = valueliest.ElementAt(0).EnergyPrice;

            }

            else
            {
                dt = CurrentDate;
                key = dt.ToString("yyyy-MM-dd") + "?" + (hour - 1);

                if (AllhourMinListDicEP.ContainsKey(key))
                {
                    prevHourMinCollectionSP = AllhourMinListDicEP[key];
                    prevHourMinCollectionSP = prevHourMinCollectionSP.OrderBy(x => x.Minute).ToList();
                    int lastindex = prevHourMinCollectionSP.Count - 1;
                    sp = prevHourMinCollectionSP.ElementAt(lastindex).EnergyPrice;

                }
            }

            return sp;
        }

        internal Dictionary<string, double> GetLoadsData(List<DateTime> datetimeList, string Market)
        {
            try
            {
                Dictionary<string, double> loadDictHash = new Dictionary<string, double>();
                using (VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    //List<DateTime> temiList = datetimeList.GroupBy(x => x.ToString("dd-MM-yyyy"));
                    foreach (DateTime date in datetimeList)
                    {
                        double load = 0;
                        if (!loadDictHash.ContainsKey(date.AddDays(-1).Date.ToString("dd-MM-yyyy")))
                        {
                                mSelectloadDailyCommand = new SqlCommand();
                                if (VayuDbConn.State == ConnectionState.Closed)
                                {
                                    VayuDbConn.Open();
                                }
                                mSelectloadDailyCommand.CommandText = "select CAST(MarketDateTime as date) date , max(mw) from  Vayu..LoadRT where loadskey=2213 and   MarketDateTime >= @Startdate and MarketDateTime < @endDate"
                                                                      + " group by CAST(MarketDateTime as date) order by date";
                                mSelectloadDailyCommand.Parameters.AddWithValue("@Startdate", date.Date.AddDays(-1));
                                mSelectloadDailyCommand.Parameters.AddWithValue("@endDate", date.Date);
                                mSelectloadDailyCommand.Connection = VayuDbConn;
                                SqlDataReader reader = mSelectloadDailyCommand.ExecuteReader();
                                while (reader.Read())
                                {
                                    load = reader.IsDBNull(1) ? 0.0 : Convert.ToDouble(reader.GetValue(1));
                                }
                                reader.Close();
                                if (load == 0)
                                {
                                    mSelectloadHourlyCommand = new SqlCommand();
                                    mSelectloadHourlyCommand.CommandText = "select  max(MW) from  Vayu..LoadRT where LoadsKey = 2213 and MarketDateTime >=  @StartDate and MarketDateTime < @EndDate";//LoadRTH 
                                    mSelectloadHourlyCommand.Parameters.AddWithValue("@StartDate", date.Date.AddDays(-1));
                                    mSelectloadHourlyCommand.Parameters.AddWithValue("@EndDate", date.Date);
                                    mSelectloadHourlyCommand.Connection = VayuDbConn;
                                    SqlDataReader reader1 = mSelectloadHourlyCommand.ExecuteReader();
                                    while (reader1.Read())
                                    {
                                        load = reader1.IsDBNull(0) ? 0.0 : Convert.ToDouble(reader1.GetValue(0));
                                    }
                                    reader1.Close();
                                }
                            
                            loadDictHash.Add(date.AddDays(-1).Date.ToString("dd-MM-yyyy"), load);
                        }

                    }
                }
                VayuDbConn.Close();
                return loadDictHash;

            }
            catch (Exception)
            {
                return null;
                //throw;
            }
        }

        internal class ErcotEnergyPriceHelper
        {
            public DateTime EPMarketDateTime { get; set; }
            public int Hour { get; set; }
            public int Minute { get; set; }
            public int Seconds { get; set; }
            public double EnergyPrice { get; set; }
            public double MaxLoad { get; set; }
        }


    }
}