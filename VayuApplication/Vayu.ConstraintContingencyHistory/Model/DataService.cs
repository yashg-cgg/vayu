using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using Vayu.CommonAccessLibrary;
using Vayu.LatestConstraintsInformationLibrary;
using Vayu.NodePriceLibrary;

namespace Vayu.ConstraintContingencyHistory.Model
{
    public class DataService : IDataService
    {

        SqlConnection VayuConnection;


        #region SQL Commands

        SqlCommand mSelectPJMDistinctConstraintCmd;
        SqlCommand mSelectERCOTDistinctConstraintCmd;
        SqlCommand mSelectERCOTDistinctContingencyCmd;

        SqlCommand mSelectPJMDADistinctConstraintCmd;

        SqlCommand mSelectERCOTDAContingencyCmd;

        SqlCommand mSelectPJMContingencyCmd;

        SqlCommand mSelectERCOTContingencyCmd;

        SqlCommand mSelectPJMDAContingencyCmd;

        SqlCommand mSelectPJMHistoricConstraintData;

        SqlCommand mSelectERCOTHistoricConstraintData;

        SqlCommand mSelectDAPJMHistoricConstraintData;

        SqlCommand mSelectDAERCOTMHistoricConstraintData;
        SqlCommand mSelectDAERCOTMHistoricalContingencyData;
        SqlCommand mSelectErcotDADistinctConstraintCmd;
        SqlCommand mSelectErcotDADistinctContingencyCmd;

        SqlCommand mSelectERCOTConstraintCmd;
        SqlCommand mSelectERCOTDAConstraintCmd;
        private SqlCommand mSelectloadDailyCommand;

        private SqlCommand mSelectloadHourlyCommand;

        private SqlCommand mSelectZoneCommand;

        private SqlCommand mGetMaxShadowPriceCommand;
        #endregion


        private Dictionary<string, Dictionary<DateTime, double>> mAvgHash = null;
        public Dictionary<string, double> loadDictHash = new Dictionary<string, double>();
        Dictionary<string, List<ErcotEnergyPriceHelper>> hourMinListDic;
        private Dictionary<string, List<ErcotEnergyPriceHelper>> AllhourMinListDicEP = new Dictionary<string, List<ErcotEnergyPriceHelper>>();
        Dictionary<string, List<ErcotEnergyPriceHelper>> PrevDayhourMinListDic;
        #region Public Methods

        public void GetConstraintData(Action<System.Collections.Generic.List<Constraint>, Exception> callback, int marketKey, bool isDa, DateTime fromDate, DateTime? throDate = null)
        {
            List<Constraint> tempList = FillAvgHash(marketKey, isDa, fromDate, throDate);

            #endregion

            List<Constraint> tempdatalist = CalculateShadowPrices(tempList, marketKey, isDa);

            callback(tempdatalist.OrderByDescending(a => a.Price).ThenByDescending(a => a.ConstraintDate).ToList(), null);
        }

        public List<Constraint> GetEneryPrice(DateTime fromDate, DateTime toDate, bool isDA)
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

            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            if (isDA)
            {

                sqlCommandEnergyPrice.CommandText = "select   CASE  when  FORMAT(DeliveryDate,'HH')=0 then DATEADD(day, -1, DeliveryDate) ELSE DeliveryDate END AS DeliveryDate, " +
                        "DAY(DeliveryDate),FORMAT(DeliveryDate,'HH')  ,FORMAT(DeliveryDate,'mm'),FORMAT(DeliveryDate,'ss'),SystemLambda " +
                        "from  Vayu..DAMSystemLambda where DeliveryDate>@Startdate and DeliveryDate <=@endDate and DSTFlag='N' order by DeliveryDate desc ";

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
            sqlCommandEnergyPrice.Connection = VayuConnection;
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

            foreach (var Datevalue in DatehourDic)
            {
                List<int> hourList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 0 };
                Dictionary<int, double> EPhourlist = new Dictionary<int, double>();

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

                        if (isDA)
                            FinalEnergyPrice = hourMinuteCollection[0].EnergyPrice;
                        else
                            FinalEnergyPrice = CalculateEnergyPrice(loopDate, hourMinuteCollection, PrevDayhourMinListDic);

                        EPhourlist.Add(hour, FinalEnergyPrice);
                    }

                }
                string loadDicKey = DateTime.Parse(Datevalue.Key).ToString("yyyy-MM-dd");
                datetimeList = new List<DateTime>();
                datetimeList.Add(Convert.ToDateTime(loadDicKey).AddDays(1));
                loadDictHash = GetLoadsData(datetimeList, "9");

                Constraint constraintObj = new Constraint();
                constraintObj.ConstraintText = "";
                constraintObj.ContingencyText = "";
                constraintObj.ConstraintDate = DateTime.Parse(Datevalue.Key);


                loadDicKey = DateTime.Parse(Datevalue.Key).ToString("dd-MM-yyyy");
                constraintObj.MaxLoad = loadDictHash[loadDicKey];


                foreach (var annItem in EPhourlist)
                {
                    try
                    {
                        if (isDA)
                        {
                            if (annItem.Key == 0)
                            {
                                constraintObj.GetType().GetProperty("HE" + (annItem.Key == 0 ? "24" : (annItem.Key).ToString())).SetValue(constraintObj, annItem.Value == 1 ? 0 : annItem.Value);
                            }
                            else { constraintObj.GetType().GetProperty("HE" + (annItem.Key == 1 ? "1" : (annItem.Key).ToString())).SetValue(constraintObj, annItem.Value == 1 ? 0 : annItem.Value); }
                        }
                        else
                        {
                            constraintObj.GetType().GetProperty("HE" + (annItem.Key == 0 ? "1" : (annItem.Key + 1).ToString())).SetValue(constraintObj, annItem.Value == 0 ? 0 : annItem.Value);
                        }

                    }
                    catch
                    {
                    }
                }
                ConstraintHistoryHash.Add(constraintObj);

            }//adate list


            return ConstraintHistoryHash;
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
                                        #region Shadow Price Calculation

                                        Dictionary<int, double> hourlyValues = new Dictionary<int, double>();
                                        Dictionary<int, Dictionary<int, double>> values = new Dictionary<int, Dictionary<int, double>>();
                                        Dictionary<int, List<ErcotConstraintHelper>> hourMinuteDict = new Dictionary<int, List<ErcotConstraintHelper>>();

                                        foreach (var a in item.Value)
                                        {

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
                                //constObj.MaxShadowPrice = item.Value;
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


        List<string> getMissingPrints(DateTime DateVal, int Hourval, int MinVal)
        {
            List<string> MinSecList = new List<string>();
            string MinuteSecond = "";
            try
            {
                SqlCommand mSelectSCEDPrintCommond = new SqlCommand();
                VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }

                if (MinVal == 55)
                    mSelectSCEDPrintCommond.CommandText = "Select distinct top 1 MarketMin, Second from  Vayu..NodeLMPMin where MarketDate = @DateVal and  Markethour=@Hourval+1 and MarketMin =0 order by MarketMin,Second";  //Markethour=@Hourval-1 and MarketMin >=55
                else
                    mSelectSCEDPrintCommond.CommandText = "Select distinct MarketMin, Second from  Vayu..NodeLMPMin where MarketDate = @DateVal and  Markethour=@Hourval and MarketMin > @MinVal";

                mSelectSCEDPrintCommond.Parameters.AddWithValue("@DateVal", DateVal.Date);
                mSelectSCEDPrintCommond.Parameters.AddWithValue("@Hourval", Hourval);
                mSelectSCEDPrintCommond.Parameters.AddWithValue("@MinVal", MinVal);
                mSelectSCEDPrintCommond.Connection = VayuConnection;
                SqlDataReader reader = mSelectSCEDPrintCommond.ExecuteReader();
                while (reader.Read())
                {
                    MinuteSecond = reader.GetInt32(0) + "#" + reader.GetInt32(1);
                    MinSecList.Add(MinuteSecond);
                }
                reader.Close();

            }
            catch (Exception ae)
            { }
            VayuConnection.Close();
            return MinSecList;

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

        double GetShadowPrice(int min, List<ErcotConstraintHelper> hourMinuteCollection, Dictionary<int, double> AllMinDictonary)
        {


            List<int> Minutelist = new List<int>();

            double SPfinal = 0;

            Minutelist.Clear();

            for (int i = 0; i < hourMinuteCollection.Count; i++)
            {
                Minutelist.Add(hourMinuteCollection.ElementAt(i).Minute);
            }

            int length = hourMinuteCollection.Count;
            double numdemo = 0;
            int currentMin = min;
            int prevmin;

            int count = Minutelist.ToArray().Count(x => x == currentMin);

            try
            {

                if (Minutelist.Contains(currentMin))
                {
                    if (count > 1)
                    {
                        for (int j = 0; j < hourMinuteCollection.Count; j++)
                        {
                            if (hourMinuteCollection.ElementAt(j).Minute == currentMin)
                            {
                                SPfinal += hourMinuteCollection.ElementAt(j).ShadowPrice;
                            }

                        }

                        SPfinal = SPfinal / count;

                    }

                    else
                    {
                        for (int j = 0; j < hourMinuteCollection.Count; j++)
                        {
                            if (hourMinuteCollection.ElementAt(j).Minute == currentMin)
                            {
                                SPfinal = hourMinuteCollection.ElementAt(j).ShadowPrice;
                            }

                        }
                    }
                }

                else
                {
                    if (currentMin == 0)
                        SPfinal = 0;
                    else
                    {
                        prevmin = currentMin - 1;
                        if (GetInterval(prevmin) == GetInterval(currentMin))
                        {
                            SPfinal = AllMinDictonary[prevmin];
                        }
                        else
                            SPfinal = 0;
                    }

                }

            }
            catch (Exception ae)
            { }


            return SPfinal;
        }

        int GetInterval(int MinuteValue)
        {
            int interval = 0;

            int currentMin = MinuteValue;
            if (currentMin >= 0 && currentMin < 5)
                interval = 1;
            if (currentMin >= 5 && currentMin < 10)
                interval = 2;
            if (currentMin >= 10 && currentMin < 15)
                interval = 3;
            if (currentMin >= 15 && currentMin < 20)
                interval = 4;
            if (currentMin >= 20 && currentMin < 25)
                interval = 5;
            if (currentMin >= 25 && currentMin < 30)
                interval = 6;
            if (currentMin >= 30 && currentMin < 35)
                interval = 7;
            if (currentMin >= 35 && currentMin < 40)
                interval = 8;
            if (currentMin >= 40 && currentMin < 45)
                interval = 9;
            if (currentMin >= 45 && currentMin < 50)
                interval = 10;
            if (currentMin >= 50 && currentMin < 55)
                interval = 11;
            if (currentMin >= 55 && currentMin < 60)
                interval = 12;

            return interval;


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
                using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    //List<DateTime> temiList = datetimeList.GroupBy(x => x.ToString("dd-MM-yyyy"));
                    foreach (DateTime date in datetimeList)
                    {
                        double load = 0;
                        if (!loadDictHash.ContainsKey(date.AddDays(-1).Date.ToString("dd-MM-yyyy")))
                        {
                            if (Market == "PJM")
                            {
                                mSelectloadDailyCommand = new SqlCommand();
                                if (VayuConnection.State == ConnectionState.Closed)
                                {
                                    VayuConnection.Open();
                                }
                                mSelectloadDailyCommand.CommandText = "select CAST(MarketDateTime as date) date , max(mw) from LoadRT where loadskey=25 and   MarketDateTime >= @Startdate and MarketDateTime < @endDate"
                                                                      + " group by CAST(MarketDateTime as date) order by date";
                                mSelectloadDailyCommand.Parameters.AddWithValue("@Startdate", date.Date.AddDays(-1));
                                mSelectloadDailyCommand.Parameters.AddWithValue("@endDate", date.Date);
                                mSelectloadDailyCommand.Connection = VayuConnection;
                                SqlDataReader reader = mSelectloadDailyCommand.ExecuteReader();
                                while (reader.Read())
                                {
                                    load = reader.IsDBNull(1) ? 0.0 : Convert.ToDouble(reader.GetValue(1));
                                }
                                reader.Close();
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
                            }
                            else //For Ercot
                            {
                                mSelectloadDailyCommand = new SqlCommand();
                                if (VayuConnection.State == ConnectionState.Closed)
                                {
                                    VayuConnection.Open();
                                }
                                mSelectloadDailyCommand.CommandText = "select CAST(MarketDateTime as date) date , max(mw) from  Vayu..LoadRT where loadskey=2213 and   MarketDateTime >= @Startdate and MarketDateTime < @endDate"
                                                                      + " group by CAST(MarketDateTime as date) order by date";
                                mSelectloadDailyCommand.Parameters.AddWithValue("@Startdate", date.Date.AddDays(-1));
                                mSelectloadDailyCommand.Parameters.AddWithValue("@endDate", date.Date);
                                mSelectloadDailyCommand.Connection = VayuConnection;
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
                                    mSelectloadHourlyCommand.Connection = VayuConnection;
                                    SqlDataReader reader1 = mSelectloadHourlyCommand.ExecuteReader();
                                    while (reader1.Read())
                                    {
                                        load = reader1.IsDBNull(0) ? 0.0 : Convert.ToDouble(reader1.GetValue(0));
                                    }
                                    reader1.Close();
                                }
                            }
                            loadDictHash.Add(date.AddDays(-1).Date.ToString("dd-MM-yyyy"), load);
                        }

                    }
                }
                VayuConnection.Close();
                return loadDictHash;

            }
            catch (Exception)
            {
                return null;
                //throw;
            }
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
        internal List<ViewModels.SensitivityHelper> GetErcotSensitivities(string constraintName, string contingencyName, bool isDA)
        {
            List<ViewModels.SensitivityHelper> senSitivityList = new List<ViewModels.SensitivityHelper>();
            try
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.Connection = con;
                        if (isDA)
                        {
                            cmd.CommandText = "select a.ConstraintRTNum , b.MonitoredText , b.ContingencyText , c.NodeName , a.Sensitivity , a.source , a.nodekey , c.Zone  " +
                                             "from DAMasterVector a join DAMasterConstraint b on a.ConstraintRTNum = b.ConstraintRTNum join Node c on a.NodeKey = c.NodeKey " +
                                             "where b.MonitoredText = '" + constraintName + "' and b.ContingencyText = '" + contingencyName + "'";
                        }
                        else
                        { //changing here for testing the data for the constraint 10-04-2026 rtmastervector_new to rtmastervector
                            cmd.CommandText = "select a.ConstraintRTNum , b.MonitoredText , b.ContingencyText , c.NodeName , a.Sensitivity , a.source , a.nodekey , c.Zone " +
                                " from RTMasterVector a join RTMasterConstraint b on a.ConstraintRTNum = b.ConstraintRTNum " +
                                " join Node c on a.NodeKey = c.NodeKey where b.MonitoredText = '" + constraintName + "' and b.ContingencyText = '" + contingencyName + "'";
                        }
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            ViewModels.SensitivityHelper helper = new ViewModels.SensitivityHelper();
                            helper.ConstraintId = rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr.GetValue(0));
                            helper.Constraint = rdr.IsDBNull(1) ? "" : rdr.GetValue(1).ToString();
                            helper.Contingency = rdr.IsDBNull(2) ? "" : rdr.GetValue(2).ToString();
                            helper.NodeName = rdr.IsDBNull(3) ? "" : rdr.GetValue(3).ToString();
                            helper.Sensitivity = rdr.IsDBNull(4) ? 0 : Convert.ToDouble(rdr.GetValue(4));
                            helper.Source = rdr.IsDBNull(5) ? "" : rdr.GetValue(5).ToString();
                            helper.NodeKey = rdr.IsDBNull(6) ? 0 : Convert.ToInt32(rdr.GetValue(6));
                            helper.Zone = rdr.IsDBNull(7) ? "" : rdr.GetValue(7).ToString();
                            senSitivityList.Add(helper);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return senSitivityList;
        }

        List<Constraint> lstConstraintDetails;

        public List<Constraint> FillAvgHashByService(int marketKey, bool isDa, DateTime fromDate, DateTime? throDate)
        {

            List<Constraint> tempList = new List<Constraint>();
            ConstraintHelper helper = new ConstraintHelper();
            IConstraintInfoProvider chelper = helper.GetInstance();
            List<LatestConstraint> constraintList = null;
            Dictionary<int, string> dictZones = new Dictionary<int, string>();
            //Dictionary<string, double> dictMaxprice = new Dictionary<string, double>();
            dictZones = GetZones(marketKey);
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
                    if (contingency == "DGT_HOC8")
                    {
                        if (constraint == "GG_TAP91_1/GG-GG/138-138")
                        {

                        }
                    }
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

                    {
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
                    }
                    //else
                    //{
                    //    if (isDa)
                    //        maxShadowPrice = 00;
                    //    else
                    //        maxShadowPrice = item.MaxSHadowPrice;
                    //}
                    if (sourceNodekey != 0)
                    {
                        //if (sourceNodekey != null)
                        SourceZonename = dictZones[sourceNodekey].ToString();
                    }
                    else
                    {
                        SourceZonename = " ";
                    }

                    if (sinkNodekey != 0)
                    {
                        //if (sinkNodekey != null)
                        SinkZonename = dictZones[sinkNodekey].ToString();
                    }
                    else
                    {
                        SinkZonename = " ";
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

        public Dictionary<int, string> GetZones(int Marketkey)
        {
            Dictionary<int, string> dictZones = new Dictionary<int, string>();
            using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                mSelectZoneCommand = VayuConnection.CreateCommand();
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }

                mSelectZoneCommand.CommandText = "Select NodeKey,Zone from  Vayu..Node where MarketKey = 9";
                SqlDataReader reader = mSelectZoneCommand.ExecuteReader();
                while (reader.Read())
                {
                    dictZones.Add(Convert.ToInt32(reader.GetValue(0)), Convert.ToString(reader.GetValue(1)));
                }
                reader.Close();
                VayuConnection.Close();
            }
            return dictZones;
        }
        public void GetDetailedConstraintData(Action<List<Constraint>, Exception> callback, string hour, Constraint constraint, int market, string marketName)
        {

            if (marketName == "SystemLambda")
            {
                if (hourMinListDic != null)
                {

                    List<ErcotEnergyPriceHelper> EPMinutewisedata = new List<ErcotEnergyPriceHelper>();
                    hour = (Convert.ToInt32(hour) - 1).ToString();
                    string key = constraint.ConstraintDate.ToString("yyyy-MM-dd") + "?" + hour;
                    DateTime tempDate;
                    try
                    {
                        List<Constraint> tempEPlist = new List<Constraint>();
                        if (hourMinListDic.ContainsKey(key))
                        {
                            EPMinutewisedata = hourMinListDic[key];
                            foreach (var item in EPMinutewisedata)
                            {
                                string dd = constraint.ConstraintDate.ToString("yyyy-MM-dd") + " " + hour + ":" + item.Minute + ":" + item.Seconds;

                                tempDate = Convert.ToDateTime(dd);
                                tempEPlist.Add(new Constraint
                                {
                                    ConstraintText = "",
                                    ContingencyText = "",
                                    ConstraintDate = tempDate,
                                    Price = item.EnergyPrice
                                }); ;
                            }
                        }
                        callback(tempEPlist.OrderByDescending(a => a.ConstraintDate).ToList(), null);

                    }
                    catch
                    {
                        callback(null, new ArgumentException("Unknown error occured, pls contact support team"));
                    }

                }
            }

            else
            {
                if (mAvgHash != null)
                {
                    try
                    {
                        List<Constraint> tempList = new List<Constraint>();
                        string key = constraint.ConstraintText + "?split?" + constraint.ContingencyText;
                        if (mAvgHash.ContainsKey(key))
                        {
                            Dictionary<DateTime, double> valueHash = mAvgHash[key];
                            var data = valueHash.Where(a => a.Key.Date == constraint.ConstraintDate.Date && a.Key.Hour + 1 == Convert.ToInt32(hour));
                            foreach (var item in data)
                            {
                                tempList.Add(new Constraint
                                {
                                    ConstraintText = constraint.ConstraintText,
                                    ContingencyText = constraint.ContingencyText,
                                    ConstraintDate = item.Key,
                                    Price = item.Value
                                });
                            }
                        }
                        callback(tempList.OrderByDescending(a => a.ConstraintDate).ToList(), null);
                    }
                    catch
                    {
                        callback(null, new ArgumentException("Unknown error occured, pls contact support team"));
                    }
                }
            }
        }
        #endregion


        #region Private Methods
        private void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();


            mSelectERCOTDistinctConstraintCmd = new SqlCommand();
            mSelectERCOTDistinctConstraintCmd.CommandText = "select distinct constrainttext from  Vayu..ConstraintRT order by ConstraintText";
            mSelectERCOTDistinctConstraintCmd.Connection = VayuConnection;

            mSelectErcotDADistinctConstraintCmd = new SqlCommand();
            mSelectErcotDADistinctConstraintCmd.CommandText = "select distinct constrainttext from  Vayu..ConstraintDA order by ConstraintText";
            mSelectErcotDADistinctConstraintCmd.Connection = VayuConnection;


            mSelectERCOTDistinctContingencyCmd = new SqlCommand();
            mSelectERCOTDistinctContingencyCmd.CommandText = "select distinct ContingencyText from  Vayu..ConstraintRT order by ContingencyText";
            mSelectERCOTDistinctContingencyCmd.Connection = VayuConnection;

            mSelectErcotDADistinctContingencyCmd = new SqlCommand();
            mSelectErcotDADistinctContingencyCmd.CommandText = "select distinct ContingencyText from  Vayu..ConstraintDA order by ContingencyText";
            mSelectErcotDADistinctContingencyCmd.Connection = VayuConnection;


            mSelectERCOTConstraintCmd = new SqlCommand();
            mSelectERCOTConstraintCmd.CommandText = "select distinct constrainttext from [Vayu].[dbo].[ConstraintRT] where ContingencyText = @ContingencyText order by constrainttext";
            mSelectERCOTConstraintCmd.Connection = VayuConnection;
            //

            mSelectERCOTDAConstraintCmd = new SqlCommand();
            mSelectERCOTDAConstraintCmd.CommandText = "select distinct constrainttext from [Vayu].[dbo].[ConstraintDA] where ContingencyText = @ContingencyText order by constrainttext";
            mSelectERCOTDAConstraintCmd.Connection = VayuConnection;

            mSelectERCOTContingencyCmd = new SqlCommand();
            mSelectERCOTContingencyCmd.CommandText = "select distinct ContingencyText from [Vayu].[dbo].[ConstraintRT] where ConstraintText = @ConstraintText order by ContingencyText";
            mSelectERCOTContingencyCmd.Connection = VayuConnection;
            //

            mSelectERCOTDAContingencyCmd = new SqlCommand();
            mSelectERCOTDAContingencyCmd.CommandText = "select distinct ContingencyText from [Vayu].[dbo].[ConstraintDA] where ConstraintText = @ConstraintText order by ContingencyText";
            mSelectERCOTDAContingencyCmd.Connection = VayuConnection;


            mSelectERCOTHistoricConstraintData = new SqlCommand();

            mSelectERCOTHistoricConstraintData.CommandText = " select distinct constraintText, contingencyText, CAST(MarketDateTime as date) Date, "
                + " DATEPART(HOUR, MarketDateTime) AS Hour, ShadowPrice from [Vayu].[dbo].[ConstraintRT]  where ConstraintText = @ConstraintText "
                + " order by CAST(MarketDateTime as date) ";
            mSelectERCOTHistoricConstraintData.Connection = VayuConnection;

            mSelectDAERCOTMHistoricConstraintData = new SqlCommand();
            mSelectDAERCOTMHistoricConstraintData.CommandText = "select distinct constraintText , contingencyText, CAST(MarketDateTime as date) date , DATEPART(HOUR , MarketDateTime) , abs(ShadowPrice)  from  Vayu..ConstraintDA where ConstraintText = @ConstraintText order by date desc";
            mSelectDAERCOTMHistoricConstraintData.Connection = VayuConnection;

            mSelectDAERCOTMHistoricalContingencyData = new SqlCommand();
            mSelectDAERCOTMHistoricalContingencyData.CommandText = "select distinct contingencyText, constraintText , CAST(MarketDateTime as date) date , DATEPART(HOUR , MarketDateTime) , abs(ShadowPrice)  from  Vayu..ConstraintDA where ContingencyText = @ContingencyText order by date desc";
            mSelectDAERCOTMHistoricalContingencyData.Connection = VayuConnection;
        }

        private List<Constraint> FillAvgHash(int marketKey, bool isDa, DateTime fromDate, DateTime? throDate)
        {
            //return FillAvgHashByService(marketKey, isDa, fromDate, throDate);

            List<Constraint> tempList = new List<Constraint>();
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                try
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        if (marketKey == 1)
                        {
                            cmd.CommandText = isDa
                                ? "select ConstraintName,ContingencyName,MarketDateTime,MarginalValue from MarginalValueDA(nolock)  where MarketKey=1 and MarketDateTime>=@start and MarketDateTime<@end "
                                : "select ConstraintText,ContingencyText,MarketDateTime,ShadowPrice from ConstraintRT(nolock) where MarketKey=1 and MarketDateTime>=@start and MarketDateTime<@end ";
                        }
                        else if (marketKey == 2)
                        {
                            cmd.CommandText = "select ConstraintName,ContingencyName,MarketDateTime,ShadowPrice from " + (isDa ? " MISO.ConstraintDA " : " MISO.ConstraintRT ") + "(nolock) where  MarketDateTime>=@start and MarketDateTime<@end";
                        }
                        else if (marketKey == 12)
                        {
                            cmd.CommandText = "select ConstraintName,ContingentFacility,MarketDateTime,ShadowPrice,MonitoredFacility from " + (isDa ? " SPP.MarginalValueDA " : " SPP.constraintrt ") + "(nolock) where  MarketDateTime>=@start and MarketDateTime<@end ";
                        }
                        else if (marketKey == 7)
                        {
                            cmd.CommandText = isDa ? "select ConstraintName,ContingencyName,MarketDateTime,ShadowPrice from CAISO.ConstraintDA(nolock) where  MarketDateTime>=@start and MarketDateTime<@end"
                                                   : "select ConstraintName,ContingencyName,MarketDateTime,ShadowPrice from CAISO.ConstraintRT(nolock) where  MarketDateTime>=@start and MarketDateTime<@end";
                        }
                        else if (marketKey == 9)
                        {
                            cmd.CommandText = isDa ? "select ConstraintName,ContingencyName,MarketDateTime,MarginalValue from MarginalValueDA(nolock)  where MarketKey=9 and MarketDateTime>=@start and MarketDateTime<@end "
                                                   : "select ConstraintText,ContingencyText,MarketDateTime,ShadowPrice from  Vayu..ConstraintRT(nolock) where MarketDateTime>=@start and MarketDateTime<@end ";
                        }
                        else if (marketKey == 3)
                        {
                            cmd.CommandText = isDa ? "select ConstraintName,ContingencyName,MarketDateTime,MarginalValue from MarginalValueDA(nolock)  where MarketKey=3 and MarketDateTime>=@start and MarketDateTime<@end "
                                                    : "select ConstraintText,ContingencyText,MarketDateTime,ShadowPrice from ConstraintRT(nolock) where MarketKey=3 and MarketDateTime>=@start and MarketDateTime<@end ";
                        }
                        cmd.Parameters.AddWithValue("@start", isDa ? fromDate.AddHours(1) : fromDate);
                        cmd.Parameters.AddWithValue("@end", isDa ? throDate.HasValue ? throDate.Value.AddDays(1) : fromDate.AddDays(1).AddHours(1) : (throDate.HasValue ? throDate.Value.AddDays(1) : fromDate.AddDays(1).AddHours(1)));
                        //cmd.Connection.Open();
                        IDataReader reader = cmd.ExecuteReader();
                        mAvgHash = new Dictionary<string, Dictionary<DateTime, double>>();
                        while (reader.Read())
                        {
                            try
                            {
                                string constraint = reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString();
                                string contingency = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString();
                                DateTime mktDate = reader.IsDBNull(2) ? new DateTime() : Convert.ToDateTime(reader.GetValue(2));
                                if (mktDate.Hour == 0 && isDa)
                                    mktDate = mktDate.AddMinutes(-1);
                                double prc = reader.IsDBNull(3) ? 0 : Convert.ToDouble(reader.GetValue(3));
                                string keyText = constraint + "?split?" + contingency;
                                // string monitoredFacility = marketKey == 12 ? (reader.IsDBNull(4) ? "" : reader.GetValue(4).ToString()) : "";

                                /* if (!constraint.ToLower().StartsWith("osgcanbusdea"))
                                     continue;
                                 if (mktDate.Hour == 24)
                                 {
                                 }*/
                                Constraint constraintItem = tempList.Where(a => a.ConstraintText == constraint && a.ContingencyText == contingency && a.ConstraintDate.Date == mktDate.Date).FirstOrDefault();
                                if (constraintItem == null)
                                {
                                    constraintItem = new Constraint { ConstraintDate = mktDate, ContingencyText = contingency, ConstraintText = constraint };
                                }
                                if (mAvgHash.ContainsKey(keyText))
                                {
                                    Dictionary<DateTime, double> hourValues = mAvgHash[keyText];
                                    if (hourValues.ContainsKey(mktDate))
                                    {
                                        if (!reader.IsDBNull(3))
                                            hourValues[mktDate] += hourValues[mktDate] + Math.Abs((Convert.ToDouble(reader.GetValue(3))));
                                    }
                                    else
                                    {
                                        hourValues.Add(mktDate, reader.IsDBNull(3) ? 0 : Math.Abs(Convert.ToDouble(reader.GetValue(3))));
                                    }
                                }
                                else
                                {
                                    Dictionary<DateTime, double> tempHash = new Dictionary<DateTime, double>();
                                    tempHash.Add(mktDate, reader.IsDBNull(3) ? 0 : Math.Abs(Convert.ToDouble(reader.GetValue(3))));
                                    mAvgHash.Add(keyText, tempHash);
                                }
                                if (tempList.Contains(constraintItem))
                                    tempList.Remove(constraintItem);
                                tempList.Add(constraintItem);
                            }
                            catch
                            {
                            }
                        }
                        reader.Close();
                        cmd.Connection.Close();
                    }
                }
                catch
                {
                }
            }

            return tempList;
        }

        internal string GetConstraintname(int constraintId, int marketKey, bool isDA)
        {
            string constraintdet = string.Empty;
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {

                try
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        if (marketKey == 9)
                        {
                            if (isDA)
                                cmd.CommandText = "select MonitoredText , ContingencyText from  Vayu..DAMasterConstraint where ConstraintRTNum = " + constraintId.ToString();
                            else
                                cmd.CommandText = "select MonitoredText , ContingencyText from  Vayu..RTMasterConstraint where ConstraintRTNum = " + constraintId.ToString();
                        }
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string constraintname = rdr.IsDBNull(0) ? null : rdr.GetString(0);
                            string contingencyName = rdr.IsDBNull(0) ? null : rdr.GetString(1);
                            constraintdet = constraintname + '?' + contingencyName;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error While Fetching Constraint Date");
                }
                return constraintdet;
                con.Close();
            }

        }

        #endregion
        SqlDataReader rdrConstaint;
        #region Internal Methods

        internal List<string> FillConstraintList(bool isRt, int marketId, bool isconstraint)
        {
            loadDBCommands();
            List<string> constraintList = new List<string>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            if (isconstraint)
            {
                //SqlDataReader rdr;
                if (isRt)
                {
                    if (marketId == 9) { rdrConstaint = mSelectERCOTDistinctConstraintCmd.ExecuteReader(); }
                }
                else
                {
                    if (marketId == 9)
                        rdrConstaint = mSelectErcotDADistinctConstraintCmd.ExecuteReader();
                }
            }
            else
            {
                if (isRt)
                {
                    if (marketId == 9) { rdrConstaint = mSelectERCOTDistinctContingencyCmd.ExecuteReader(); }
                }
                else
                {
                    if (marketId == 9)
                        rdrConstaint = mSelectErcotDADistinctContingencyCmd.ExecuteReader();
                }
            }
            while (rdrConstaint.Read())
            {
                string constraint = rdrConstaint.GetValue(0).ToString();
                constraintList.Add(constraint);
            }
            rdrConstaint.Close();
            VayuConnection.Close();
            return constraintList;
        }

        internal List<string> FillContingencyList(bool isRt, int marketId)
        {
            loadDBCommands();
            List<string> constraintList = new List<string>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            //SqlDataReader rdr;
            if (isRt)
            {
                if (marketId == 9) { rdrConstaint = mSelectERCOTDistinctConstraintCmd.ExecuteReader(); }
            }
            else
            {
                if (marketId == 9)
                    rdrConstaint = mSelectErcotDADistinctConstraintCmd.ExecuteReader();
            }

            while (rdrConstaint.Read())
            {
                string constraint = rdrConstaint.GetValue(0).ToString();
                constraintList.Add(constraint);
            }
            rdrConstaint.Close();
            VayuConnection.Close();
            return constraintList;
        }

        SqlDataReader rdrContingencies;
        internal List<string> GetAllContingencies(string constraintName, bool isRt, int marketId)
        {
            loadDBCommands();
            List<string> contingencyList = new List<string>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            if (isRt)
            {
                if (marketId == 9)
                {
                    mSelectERCOTContingencyCmd.Parameters.AddWithValue("@ConstraintText", constraintName);
                    rdrContingencies = mSelectERCOTContingencyCmd.ExecuteReader();
                }
            }
            else
            {
                if (marketId == 9)
                {
                    mSelectERCOTDAContingencyCmd.Parameters.AddWithValue("@ConstraintText", constraintName);
                    rdrContingencies = mSelectERCOTDAContingencyCmd.ExecuteReader();
                }
            }


            while (rdrContingencies.Read())
            {
                string contingency = rdrContingencies.GetValue(0).ToString();
                contingencyList.Add(contingency);
            }
            rdrContingencies.Close();
            VayuConnection.Close();
            return contingencyList;
        }

        internal List<string> GetAllConstraints(string contingencyName, bool isRt, int marketId)
        {
            loadDBCommands();
            List<string> constraintList = new List<string>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            if (isRt)
            {
                if (marketId == 9)
                {
                    mSelectERCOTConstraintCmd.Parameters.AddWithValue("@ContingencyText", contingencyName);
                    rdrContingencies = mSelectERCOTConstraintCmd.ExecuteReader();
                }
            }
            else
            {
                if (marketId == 9)
                {
                    mSelectERCOTDAConstraintCmd.Parameters.AddWithValue("@ContingencyText", contingencyName);
                    rdrContingencies = mSelectERCOTDAConstraintCmd.ExecuteReader();
                }
            }


            while (rdrContingencies.Read())
            {
                string contingency = rdrContingencies.GetValue(0).ToString();
                constraintList.Add(contingency);
            }
            rdrContingencies.Close();
            VayuConnection.Close();
            return constraintList;
        }

        internal List<Constraint> GetAllHistoricalConstraintsData(string constraintname, string ContingencyName, bool isRt, int marketkey)
        {
            loadDBCommands();
            DateTime prevDate = DateTime.MinValue;
            List<Constraint> HistoricalDataList = new List<Constraint>();

            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();

            SqlDataReader rdr = null;
            if (isRt)
            {

                if (constraintname != "")
                {
                    HistoricalDataList = GetHistoricalConstraintsForErcot(constraintname, isRt, marketkey);
                }

                return HistoricalDataList;

                //mSelectERCOTHistoricConstraintData.Parameters.AddWithValue("@ConstraintText", constraintname);
                ////mSelectERCOTHistoricConstraintData.Parameters.AddWithValue("@ContingencyText", ContingencyName);
                //rdr = mSelectERCOTHistoricConstraintData.ExecuteReader();
            }

            else
            {
                if (marketkey == 9)
                {
                    mSelectDAERCOTMHistoricConstraintData.Parameters.AddWithValue("@ConstraintText", constraintname);
                    //mSelectDAERCOTMHistoricConstraintData.Parameters.AddWithValue("@ContingencyText", ContingencyName);
                    rdr = mSelectDAERCOTMHistoricConstraintData.ExecuteReader();
                }
            }

            List<ConstraintHistoryhelper> ConstraintHistoryHash = new List<ConstraintHistoryhelper>();
            while (rdr.Read())
            {
                // string date = Convert.ToDateTime(rdr.GetValue(0)).ToString() + ":" + rdr.GetValue(1);
                List<ConstraintHistoryhelper> constraintTempList;
                ConstraintHistoryhelper constraintObj = new ConstraintHistoryhelper();
                constraintObj.ConstraintName = rdr.GetValue(0).ToString();
                constraintObj.ContingencyName = rdr.GetValue(1).ToString();
                constraintObj.date = Convert.ToDateTime(rdr.GetValue(2));
                constraintObj.hour = Convert.ToInt32(rdr.GetValue(3));
                constraintObj.shadowprice = rdr.IsDBNull(4) ? 00 : Convert.ToDouble(rdr.GetValue(4));

                if (Convert.ToInt32(rdr.GetValue(3)) == 0)//if hour is 0 reset date to previous day date
                    constraintObj.date = Convert.ToDateTime(rdr.GetValue(2)).AddDays(-1);

                ConstraintHistoryHash.Add(constraintObj);
                //}
            }
            rdr.Close();

            var listdate = ConstraintHistoryHash.GroupBy(x => x.date);

            foreach (var item in listdate)
            {
                Constraint constraint = new Constraint();
                //List<ConstraintHistoryhelper> ConstraintTempList = item.Value;
                constraint.ConstraintDate = item.Key;

                foreach (var subitem in item)
                {
                    //var listContingency = ConstraintHistoryHash.GroupBy(x => x.ContingencyName).Distinct();
                    constraint.ConstraintText = subitem.ConstraintName;
                    constraint.ContingencyText = subitem.ContingencyName;
                    int hour = subitem.hour;
                    double shadowPrice = subitem.shadowprice;

                    if (hour == 0)
                        constraint.HE24 = shadowPrice;
                    else if (hour == 1)
                        constraint.HE1 = shadowPrice;
                    else if (hour == 2)
                        constraint.HE2 = shadowPrice;
                    else if (hour == 3)
                        constraint.HE3 = shadowPrice;
                    else if (hour == 4)
                        constraint.HE4 = shadowPrice;
                    else if (hour == 5)
                        constraint.HE5 = shadowPrice;
                    else if (hour == 6)
                        constraint.HE6 = shadowPrice;
                    else if (hour == 7)
                        constraint.HE7 = shadowPrice;
                    else if (hour == 8)
                        constraint.HE8 = shadowPrice;
                    else if (hour == 9)
                        constraint.HE9 = shadowPrice;
                    else if (hour == 10)
                        constraint.HE10 = shadowPrice;
                    else if (hour == 11)
                        constraint.HE11 = shadowPrice;
                    else if (hour == 12)
                        constraint.HE12 = shadowPrice;
                    else if (hour == 13)
                        constraint.HE13 = shadowPrice;
                    else if (hour == 14)
                        constraint.HE14 = shadowPrice;
                    else if (hour == 15)
                        constraint.HE15 = shadowPrice;
                    else if (hour == 16)
                        constraint.HE16 = shadowPrice;
                    else if (hour == 17)
                        constraint.HE17 = shadowPrice;
                    else if (hour == 18)
                        constraint.HE18 = shadowPrice;
                    else if (hour == 19)
                        constraint.HE19 = shadowPrice;
                    else if (hour == 20)
                        constraint.HE20 = shadowPrice;
                    else if (hour == 21)
                        constraint.HE21 = shadowPrice;
                    else if (hour == 22)
                        constraint.HE22 = shadowPrice;
                    else if (hour == 23)
                        constraint.HE23 = shadowPrice;
                }
                HistoricalDataList.Add(constraint);
            }

            return HistoricalDataList;
        }

        internal List<Constraint> GetAllHistoricalContingencyData(string constraintname, string ContingencyName, bool isRt, int marketkey)
        {
            loadDBCommands();
            DateTime prevDate = DateTime.MinValue;
            List<Constraint> HistoricalDataList = new List<Constraint>();

            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();

            SqlDataReader rdr = null;
            if (isRt)
            {

                if (ContingencyName != "")
                {
                    HistoricalDataList = GetHistoricalContingencyForErcot(ContingencyName, isRt, marketkey);
                }

                return HistoricalDataList;

                //mSelectERCOTHistoricConstraintData.Parameters.AddWithValue("@ConstraintText", constraintname);
                ////mSelectERCOTHistoricConstraintData.Parameters.AddWithValue("@ContingencyText", ContingencyName);
                //rdr = mSelectERCOTHistoricConstraintData.ExecuteReader();
            }

            else
            {
                if (marketkey == 9)
                {
                    mSelectDAERCOTMHistoricalContingencyData.Parameters.AddWithValue("@ContingencyText", ContingencyName);
                    //mSelectDAERCOTMHistoricConstraintData.Parameters.AddWithValue("@ContingencyText", ContingencyName);
                    rdr = mSelectDAERCOTMHistoricalContingencyData.ExecuteReader();
                }
            }

            List<ConstraintHistoryhelper> ConstraintHistoryHash = new List<ConstraintHistoryhelper>();
            while (rdr.Read())
            {
                // string date = Convert.ToDateTime(rdr.GetValue(0)).ToString() + ":" + rdr.GetValue(1);
                List<ConstraintHistoryhelper> constraintTempList;
                ConstraintHistoryhelper constraintObj = new ConstraintHistoryhelper();
                constraintObj.ContingencyName = rdr.GetValue(0).ToString();
                constraintObj.ConstraintName = rdr.GetValue(1).ToString();
                constraintObj.date = Convert.ToDateTime(rdr.GetValue(2));
                constraintObj.hour = Convert.ToInt32(rdr.GetValue(3));
                constraintObj.shadowprice = rdr.IsDBNull(4) ? 00 : Convert.ToDouble(rdr.GetValue(4));

                if (Convert.ToInt32(rdr.GetValue(3)) == 0)//if hour is 0 reset date to previous day date
                    constraintObj.date = Convert.ToDateTime(rdr.GetValue(2)).AddDays(-1);

                ConstraintHistoryHash.Add(constraintObj);
                //}
            }
            rdr.Close();

            var listdate = ConstraintHistoryHash.GroupBy(x => x.date);

            foreach (var item in listdate)
            {
                Constraint constraint = new Constraint();
                //List<ConstraintHistoryhelper> ConstraintTempList = item.Value;
                constraint.ConstraintDate = item.Key;

                foreach (var subitem in item)
                {
                    //var listContingency = ConstraintHistoryHash.GroupBy(x => x.ContingencyName).Distinct();
                    constraint.ConstraintText = subitem.ConstraintName;
                    constraint.ContingencyText = subitem.ContingencyName;
                    int hour = subitem.hour;
                    double shadowPrice = subitem.shadowprice;

                    if (hour == 0)
                        constraint.HE24 = shadowPrice;
                    else if (hour == 1)
                        constraint.HE1 = shadowPrice;
                    else if (hour == 2)
                        constraint.HE2 = shadowPrice;
                    else if (hour == 3)
                        constraint.HE3 = shadowPrice;
                    else if (hour == 4)
                        constraint.HE4 = shadowPrice;
                    else if (hour == 5)
                        constraint.HE5 = shadowPrice;
                    else if (hour == 6)
                        constraint.HE6 = shadowPrice;
                    else if (hour == 7)
                        constraint.HE7 = shadowPrice;
                    else if (hour == 8)
                        constraint.HE8 = shadowPrice;
                    else if (hour == 9)
                        constraint.HE9 = shadowPrice;
                    else if (hour == 10)
                        constraint.HE10 = shadowPrice;
                    else if (hour == 11)
                        constraint.HE11 = shadowPrice;
                    else if (hour == 12)
                        constraint.HE12 = shadowPrice;
                    else if (hour == 13)
                        constraint.HE13 = shadowPrice;
                    else if (hour == 14)
                        constraint.HE14 = shadowPrice;
                    else if (hour == 15)
                        constraint.HE15 = shadowPrice;
                    else if (hour == 16)
                        constraint.HE16 = shadowPrice;
                    else if (hour == 17)
                        constraint.HE17 = shadowPrice;
                    else if (hour == 18)
                        constraint.HE18 = shadowPrice;
                    else if (hour == 19)
                        constraint.HE19 = shadowPrice;
                    else if (hour == 20)
                        constraint.HE20 = shadowPrice;
                    else if (hour == 21)
                        constraint.HE21 = shadowPrice;
                    else if (hour == 22)
                        constraint.HE22 = shadowPrice;
                    else if (hour == 23)
                        constraint.HE23 = shadowPrice;
                }
                HistoricalDataList.Add(constraint);
            }

            return HistoricalDataList;
        }

        internal List<Constraint> GetAllHistoricalConstraintsDataforDateRange(DateTime FromDateValue, DateTime ToDateValue, string constraintname, string ContingencyName, bool isRt, int marketkey)
        {
            loadDBCommands();
            DateTime prevDate = DateTime.MinValue;
            List<Constraint> HistoricalDataList = new List<Constraint>();

            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();

            SqlDataReader rdr = null;
            if (isRt)
            {

                if (constraintname != "")
                {
                    HistoricalDataList = GetHistoricalConstraintsForErcotDateRange(FromDateValue, ToDateValue, constraintname, ContingencyName, isRt, marketkey);
                }

                return HistoricalDataList;

                //mSelectERCOTHistoricConstraintData.Parameters.AddWithValue("@ConstraintText", constraintname);
                ////mSelectERCOTHistoricConstraintData.Parameters.AddWithValue("@ContingencyText", ContingencyName);
                //rdr = mSelectERCOTHistoricConstraintData.ExecuteReader();
            }

            else
            {
                if (marketkey == 9)
                {
                    mSelectDAERCOTMHistoricConstraintData.CommandText = "select distinct constraintText , contingencyText, CAST(MarketDateTime as date) date , DATEPART(HOUR , MarketDateTime) , abs(ShadowPrice)  from  Vayu..ConstraintDA where ConstraintText = @ConstraintText and (ContingencyText=@ContingencyText) and MarketDateTime>=@FromDate and MarketDateTime <= @ToDate     order by date desc";
                    mSelectDAERCOTMHistoricConstraintData.Connection = VayuConnection;

                    mSelectDAERCOTMHistoricConstraintData.Parameters.AddWithValue("@ConstraintText", constraintname);
                    mSelectDAERCOTMHistoricConstraintData.Parameters.AddWithValue("@ContingencyText", ContingencyName);
                    mSelectDAERCOTMHistoricConstraintData.Parameters.AddWithValue("@FromDate", FromDateValue);
                    mSelectDAERCOTMHistoricConstraintData.Parameters.AddWithValue("@ToDate", ToDateValue);
                    //mSelectDAERCOTMHistoricConstraintData.Parameters.AddWithValue("@ContingencyText", ContingencyName);
                    rdr = mSelectDAERCOTMHistoricConstraintData.ExecuteReader();
                }
            }

            List<ConstraintHistoryhelper> ConstraintHistoryHash = new List<ConstraintHistoryhelper>();
            while (rdr.Read())
            {
                // string date = Convert.ToDateTime(rdr.GetValue(0)).ToString() + ":" + rdr.GetValue(1);
                List<ConstraintHistoryhelper> constraintTempList;
                ConstraintHistoryhelper constraintObj = new ConstraintHistoryhelper();
                constraintObj.ConstraintName = rdr.GetValue(0).ToString();
                constraintObj.ContingencyName = rdr.GetValue(1).ToString();
                constraintObj.date = Convert.ToDateTime(rdr.GetValue(2));
                constraintObj.hour = Convert.ToInt32(rdr.GetValue(3));
                constraintObj.shadowprice = rdr.IsDBNull(4) ? 00 : Convert.ToDouble(rdr.GetValue(4));

                if (Convert.ToInt32(rdr.GetValue(3)) == 0)//if hour is 0 reset date to previous day date
                    constraintObj.date = Convert.ToDateTime(rdr.GetValue(2)).AddDays(-1);

                ConstraintHistoryHash.Add(constraintObj);
                //}
            }
            rdr.Close();

            var listdate = ConstraintHistoryHash.GroupBy(x => x.date);

            foreach (var item in listdate)
            {
                Constraint constraint = new Constraint();
                //List<ConstraintHistoryhelper> ConstraintTempList = item.Value;
                constraint.ConstraintDate = item.Key;

                foreach (var subitem in item)
                {
                    //var listContingency = ConstraintHistoryHash.GroupBy(x => x.ContingencyName).Distinct();
                    constraint.ConstraintText = subitem.ConstraintName;
                    constraint.ContingencyText = subitem.ContingencyName;
                    int hour = subitem.hour;
                    double shadowPrice = subitem.shadowprice;

                    if (hour == 0)
                        constraint.HE24 = shadowPrice;
                    else if (hour == 1)
                        constraint.HE1 = shadowPrice;
                    else if (hour == 2)
                        constraint.HE2 = shadowPrice;
                    else if (hour == 3)
                        constraint.HE3 = shadowPrice;
                    else if (hour == 4)
                        constraint.HE4 = shadowPrice;
                    else if (hour == 5)
                        constraint.HE5 = shadowPrice;
                    else if (hour == 6)
                        constraint.HE6 = shadowPrice;
                    else if (hour == 7)
                        constraint.HE7 = shadowPrice;
                    else if (hour == 8)
                        constraint.HE8 = shadowPrice;
                    else if (hour == 9)
                        constraint.HE9 = shadowPrice;
                    else if (hour == 10)
                        constraint.HE10 = shadowPrice;
                    else if (hour == 11)
                        constraint.HE11 = shadowPrice;
                    else if (hour == 12)
                        constraint.HE12 = shadowPrice;
                    else if (hour == 13)
                        constraint.HE13 = shadowPrice;
                    else if (hour == 14)
                        constraint.HE14 = shadowPrice;
                    else if (hour == 15)
                        constraint.HE15 = shadowPrice;
                    else if (hour == 16)
                        constraint.HE16 = shadowPrice;
                    else if (hour == 17)
                        constraint.HE17 = shadowPrice;
                    else if (hour == 18)
                        constraint.HE18 = shadowPrice;
                    else if (hour == 19)
                        constraint.HE19 = shadowPrice;
                    else if (hour == 20)
                        constraint.HE20 = shadowPrice;
                    else if (hour == 21)
                        constraint.HE21 = shadowPrice;
                    else if (hour == 22)
                        constraint.HE22 = shadowPrice;
                    else if (hour == 23)
                        constraint.HE23 = shadowPrice;
                }
                HistoricalDataList.Add(constraint);
            }

            return HistoricalDataList;
        }

        internal List<Constraint> GetHistoricalConstraintsForErcot(string constraintName, bool isRT, int marketKey)
        {
            List<Constraint> tempdatalist = null;

            if (constraintName == "")
            {
                return tempdatalist;
            }

            List<Constraint> tempList = new List<Constraint>();

            if (isRT && marketKey == 9)
            {
                loadDBCommands();


                List<LatestConstraint> latestConstraintList = new List<LatestConstraint>();

                Dictionary<string, DateTime> dictConstraintHistory = new Dictionary<string, DateTime>();

                SqlCommand cmdGetConstraintHistory = VayuConnection.CreateCommand();
                cmdGetConstraintHistory.CommandText = " select distinct ConstraintText, ContingencyText, MarketDateTime, ShadowPrice from  Vayu..ConstraintRT "
                    + " where ConstraintText = @ConstraintText order by MarketDateTime desc ";
                cmdGetConstraintHistory.Parameters.AddWithValue("@ConstraintText", "ConstraintText");

                cmdGetConstraintHistory.Parameters["@ConstraintText"].Value = constraintName;

                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();

                SqlDataReader rdr = cmdGetConstraintHistory.ExecuteReader();

                while (rdr.Read())
                {
                    //string ConstraintName = rdr.GetString(0);
                    //string Contingency = rdr.GetString(1);
                    //DateTime MarketDateTime = rdr.GetDateTime(2);
                    //double ShadowPrice = rdr.GetDouble(3);

                    LatestConstraint latestConstraint = new LatestConstraint();
                    latestConstraint.ConstraintText = rdr.GetValue(0).ToString();
                    latestConstraint.ContigencyText = rdr.GetValue(1).ToString();
                    latestConstraint.MarketDate = Convert.ToDateTime(rdr.GetValue(2));
                    //latestConstraint.hour = Convert.ToInt32(rdr.GetValue(3));
                    latestConstraint.ShadowPrice = rdr.IsDBNull(3) ? 00 : Convert.ToDouble(rdr.GetValue(3));

                    latestConstraintList.Add(latestConstraint);
                }

                rdr.Close();
                VayuConnection.Close();

                mAvgHash = new Dictionary<string, Dictionary<DateTime, double>>();
                foreach (var item in latestConstraintList)
                {
                    string constraint = item.ConstraintText;
                    string contingency = item.ContigencyText;

                    DateTime mktDate;
                    //if (isDa)
                    //    mktDate = item.MarketDate.AddHours(-1);
                    //else

                    mktDate = item.MarketDate;
                    double prc = item.ShadowPrice;
                    string keyText = constraint + "?split?" + contingency;
                    //int sourceNodekey = item.SourceNodeKey;
                    //int sinkNodekey = item.SinkNodeKey;
                    string constName = constraint + "?" + contingency;

                    Constraint constraintItem = tempList.Where(a => a.ConstraintText == constraint && a.ContingencyText == contingency && a.ConstraintDate.Date == mktDate.Date).FirstOrDefault();
                    if (constraintItem == null)
                    {
                        constraintItem = new Constraint
                        {
                            ConstraintDate = mktDate,
                            ContingencyText = contingency,
                            ConstraintText = constraint
                        };
                    }
                    if (mAvgHash.ContainsKey(keyText))
                    {
                        Dictionary<DateTime, double> hourValues = mAvgHash[keyText];
                        if (hourValues.ContainsKey(mktDate))
                        {
                            if (isRT)
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
            }

            tempdatalist = CalculateShadowPrices(tempList, marketKey, false);

            return tempdatalist;
        }

        internal List<Constraint> GetHistoricalContingencyForErcot(string contingencyName, bool isRT, int marketKey)
        {
            List<Constraint> tempdatalist = null;

            if (contingencyName == "")
            {
                return tempdatalist;
            }

            List<Constraint> tempList = new List<Constraint>();

            if (isRT && marketKey == 9)
            {
                loadDBCommands();


                List<LatestConstraint> latestConstraintList = new List<LatestConstraint>();

                Dictionary<string, DateTime> dictConstraintHistory = new Dictionary<string, DateTime>();

                SqlCommand cmdGetConstraintHistory = VayuConnection.CreateCommand();
                cmdGetConstraintHistory.CommandText = " select distinct ContingencyText,ConstraintText, MarketDateTime, ShadowPrice from  Vayu..ConstraintRT "
                    + " where ContingencyText = @ContingencyText order by MarketDateTime desc ";
                cmdGetConstraintHistory.Parameters.AddWithValue("@ContingencyText", "ContingencyText");

                cmdGetConstraintHistory.Parameters["@ContingencyText"].Value = contingencyName;

                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();

                SqlDataReader rdr = cmdGetConstraintHistory.ExecuteReader();

                while (rdr.Read())
                {
                    //string ConstraintName = rdr.GetString(0);
                    //string Contingency = rdr.GetString(1);
                    //DateTime MarketDateTime = rdr.GetDateTime(2);
                    //double ShadowPrice = rdr.GetDouble(3);

                    LatestConstraint latestConstraint = new LatestConstraint();
                    latestConstraint.ContigencyText = rdr.GetValue(0).ToString();
                    latestConstraint.ConstraintText = rdr.GetValue(1).ToString();
                    latestConstraint.MarketDate = Convert.ToDateTime(rdr.GetValue(2));
                    //latestConstraint.hour = Convert.ToInt32(rdr.GetValue(3));
                    latestConstraint.ShadowPrice = rdr.IsDBNull(3) ? 00 : Convert.ToDouble(rdr.GetValue(3));

                    latestConstraintList.Add(latestConstraint);
                }

                rdr.Close();
                VayuConnection.Close();

                mAvgHash = new Dictionary<string, Dictionary<DateTime, double>>();
                foreach (var item in latestConstraintList)
                {
                    string constraint = item.ConstraintText;
                    string contingency = item.ContigencyText;

                    DateTime mktDate;
                    //if (isDa)
                    //    mktDate = item.MarketDate.AddHours(-1);
                    //else

                    mktDate = item.MarketDate;
                    double prc = item.ShadowPrice;
                    string keyText = constraint + "?split?" + contingency;
                    //int sourceNodekey = item.SourceNodeKey;
                    //int sinkNodekey = item.SinkNodeKey;
                    string constName = constraint + "?" + contingency;

                    Constraint constraintItem = tempList.Where(a => a.ConstraintText == constraint && a.ContingencyText == contingency && a.ConstraintDate.Date == mktDate.Date).FirstOrDefault();
                    if (constraintItem == null)
                    {
                        constraintItem = new Constraint
                        {
                            ConstraintDate = mktDate,
                            ContingencyText = contingency,
                            ConstraintText = constraint
                        };
                    }
                    if (mAvgHash.ContainsKey(keyText))
                    {
                        Dictionary<DateTime, double> hourValues = mAvgHash[keyText];
                        if (hourValues.ContainsKey(mktDate))
                        {
                            if (isRT)
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
            }

            tempdatalist = CalculateShadowPrices(tempList, marketKey, false);

            return tempdatalist;
        }



        internal List<Constraint> GetHistoricalConstraintsForErcotDateRange(DateTime FromDateValue, DateTime ToDateValue, string constraintName, string contigencyName, bool isRT, int marketKey)
        {
            List<Constraint> tempdatalist = null;

            if (constraintName == "")
            {
                return tempdatalist;
            }

            List<Constraint> tempList = new List<Constraint>();

            if (isRT && marketKey == 9)
            {
                loadDBCommands();


                List<LatestConstraint> latestConstraintList = new List<LatestConstraint>();

                Dictionary<string, DateTime> dictConstraintHistory = new Dictionary<string, DateTime>();

                SqlCommand cmdGetConstraintHistory = VayuConnection.CreateCommand();
                cmdGetConstraintHistory.CommandText = " select distinct ConstraintText, ContingencyText, MarketDateTime, ShadowPrice from  Vayu..ConstraintRT "
                    + " where (ConstraintText = @ConstraintText) and (ContingencyText=@ContingencyText) and  (MarketDateTime>=@FromdDate and MarketDateTime<=@ToDate) order by MarketDateTime desc ";
                cmdGetConstraintHistory.Parameters.AddWithValue("@ConstraintText", "ConstraintText");
                cmdGetConstraintHistory.Parameters.AddWithValue("@ContingencyText", "ContingencyText");
                cmdGetConstraintHistory.Parameters.AddWithValue("@FromdDate", "FromdDate");
                cmdGetConstraintHistory.Parameters.AddWithValue("@ToDate", "ToDate");

                cmdGetConstraintHistory.Parameters["@ConstraintText"].Value = constraintName;
                cmdGetConstraintHistory.Parameters["@ContingencyText"].Value = contigencyName;
                cmdGetConstraintHistory.Parameters["@ToDate"].Value = ToDateValue;
                cmdGetConstraintHistory.Parameters["@FromdDate"].Value = FromDateValue;

                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();

                SqlDataReader rdr = cmdGetConstraintHistory.ExecuteReader();

                while (rdr.Read())
                {
                    //string ConstraintName = rdr.GetString(0);
                    //string Contingency = rdr.GetString(1);
                    //DateTime MarketDateTime = rdr.GetDateTime(2);
                    //double ShadowPrice = rdr.GetDouble(3);

                    LatestConstraint latestConstraint = new LatestConstraint();
                    latestConstraint.ConstraintText = rdr.GetValue(0).ToString();
                    latestConstraint.ContigencyText = rdr.GetValue(1).ToString();
                    latestConstraint.MarketDate = Convert.ToDateTime(rdr.GetValue(2));
                    //latestConstraint.hour = Convert.ToInt32(rdr.GetValue(3));
                    latestConstraint.ShadowPrice = rdr.IsDBNull(3) ? 00 : Convert.ToDouble(rdr.GetValue(3));

                    latestConstraintList.Add(latestConstraint);
                }

                rdr.Close();
                VayuConnection.Close();

                mAvgHash = new Dictionary<string, Dictionary<DateTime, double>>();
                foreach (var item in latestConstraintList)
                {
                    string constraint = item.ConstraintText;
                    string contingency = item.ContigencyText;

                    DateTime mktDate;
                    //if (isDa)
                    //    mktDate = item.MarketDate.AddHours(-1);
                    //else

                    mktDate = item.MarketDate;
                    double prc = item.ShadowPrice;
                    string keyText = constraint + "?split?" + contingency;
                    //int sourceNodekey = item.SourceNodeKey;
                    //int sinkNodekey = item.SinkNodeKey;
                    string constName = constraint + "?" + contingency;

                    Constraint constraintItem = tempList.Where(a => a.ConstraintText == constraint && a.ContingencyText == contingency && a.ConstraintDate.Date == mktDate.Date).FirstOrDefault();
                    if (constraintItem == null)
                    {
                        constraintItem = new Constraint
                        {
                            ConstraintDate = mktDate,
                            ContingencyText = contingency,
                            ConstraintText = constraint
                        };
                    }
                    if (mAvgHash.ContainsKey(keyText))
                    {
                        Dictionary<DateTime, double> hourValues = mAvgHash[keyText];
                        if (hourValues.ContainsKey(mktDate))
                        {
                            if (isRT)
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
            }

            tempdatalist = CalculateShadowPrices(tempList, marketKey, false);

            return tempdatalist;
        }
        internal List<Constraint> GetHistoricalConstraintsForErcotMap(string constraintName, bool isRT, int marketKey)
        {
            List<Constraint> tempdatalist = null;

            if (constraintName == "")
            {
                return tempdatalist;
            }

            List<Constraint> tempList = new List<Constraint>();

            if (isRT && marketKey == 9)
            {
                loadDBCommands();


                List<LatestConstraint> latestConstraintList = new List<LatestConstraint>();

                Dictionary<string, DateTime> dictConstraintHistory = new Dictionary<string, DateTime>();

                SqlCommand cmdGetConstraintHistory = VayuConnection.CreateCommand();
                cmdGetConstraintHistory.CommandText = " select distinct ConstraintText, ContingencyText, MarketDateTime, ShadowPrice from  Vayu..ConstraintRT "
                    + " where ConstraintText like'%" + constraintName + "%'order by MarketDateTime desc ";
                //cmdGetConstraintHistory.Parameters.AddWithValue("@ConstraintText", "ConstraintText");

                // cmdGetConstraintHistory.Parameters["@ConstraintText"].Value = constraintName;

                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();

                SqlDataReader rdr = cmdGetConstraintHistory.ExecuteReader();

                while (rdr.Read())
                {
                    LatestConstraint latestConstraint = new LatestConstraint();
                    latestConstraint.ConstraintText = rdr.GetValue(0).ToString();
                    latestConstraint.ContigencyText = rdr.GetValue(1).ToString();
                    latestConstraint.MarketDate = Convert.ToDateTime(rdr.GetValue(2));
                    //latestConstraint.hour = Convert.ToInt32(rdr.GetValue(3));
                    latestConstraint.ShadowPrice = rdr.IsDBNull(3) ? 00 : Convert.ToDouble(rdr.GetValue(3));
                    latestConstraintList.Add(latestConstraint);
                }
                rdr.Close();
                VayuConnection.Close();
                mAvgHash = new Dictionary<string, Dictionary<DateTime, double>>();
                foreach (var item in latestConstraintList)
                {
                    string constraint = item.ConstraintText;
                    string contingency = item.ContigencyText;

                    DateTime mktDate;
                    mktDate = item.MarketDate;
                    double prc = item.ShadowPrice;
                    string keyText = constraint + "?split?" + contingency;
                    //int sourceNodekey = item.SourceNodeKey;
                    //int sinkNodekey = item.SinkNodeKey;
                    string constName = constraint + "?" + contingency;

                    Constraint constraintItem = tempList.Where(a => a.ConstraintText == constraint && a.ContingencyText == contingency && a.ConstraintDate.Date == mktDate.Date).FirstOrDefault();
                    if (constraintItem == null)
                    {
                        constraintItem = new Constraint
                        {
                            ConstraintDate = mktDate,
                            ContingencyText = contingency,
                            ConstraintText = constraint
                        };
                    }
                    if (mAvgHash.ContainsKey(keyText))
                    {
                        Dictionary<DateTime, double> hourValues = mAvgHash[keyText];
                        if (hourValues.ContainsKey(mktDate))
                        {
                            if (isRT)
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
            }
            tempdatalist = CalculateShadowPrices(tempList, marketKey, false);
            return tempdatalist;
        }

        internal Node[] RunLMP(int marketkey, DateTime StartDate, DateTime EndDate, string type)
        {
            string DateTimeKey = StartDate.ToString() + EndDate.ToString();
            DateTime tempdate = Convert.ToDateTime("2017-09-20");
            Node[] priceNodes = null;
            try
            {
                NetTcpBinding binding = new NetTcpBinding();
                binding.Security.Mode = SecurityMode.None;
                binding.OpenTimeout = new TimeSpan(0, 30, 0);
                binding.SendTimeout = new TimeSpan(0, 12, 0);
                binding.ReceiveTimeout = new TimeSpan(0, 12, 0);
                binding.CloseTimeout = new TimeSpan(0, 12, 0);
                binding.TransactionFlow = false;
                binding.MaxReceivedMessageSize = int.MaxValue;
                binding.MaxBufferPoolSize = int.MaxValue;
                binding.MaxBufferSize = int.MaxValue;
                binding.TransferMode = TransferMode.Buffered;
                binding.ReaderQuotas.MaxArrayLength = int.MaxValue;
                ChannelFactory<ILMP> pipeFactory = new ChannelFactory<ILMP>(binding, ServiceConnections.GetLMPServiceAddress());
                ILMP nodeProxy = pipeFactory.CreateChannel();
                Console.WriteLine(DateTime.Now + " Getting Prices " + StartDate);
                if (marketkey == 1)
                {
                    if (type == "FTR")
                    {
                        if (StartDate <= tempdate)
                        {
                            priceNodes = nodeProxy.GetAllPrice(1, false, StartDate.AddMinutes(-StartDate.Minute), EndDate.AddHours(1).AddMinutes(-EndDate.Minute), false);
                        }
                        else
                        {
                            priceNodes = nodeProxy.GetAllFiveMinPrice(1, StartDate, EndDate, false);
                        }
                    }
                    else
                    {
                        priceNodes = nodeProxy.GetAllFiveMinPrice(1, StartDate, EndDate, false);
                    }
                }
                else if (marketkey == 9)
                    priceNodes = nodeProxy.GetAllFiveMinPrice(9, StartDate, EndDate, false);

                Console.WriteLine(DateTime.Now + " Got prices " + priceNodes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return priceNodes;
        }

        public List<NodePriceHelper> GetFiveMinsPRicesForUptos(DateTime fromdate, DateTime toDate, string hours, Constraint selectedConstraint, int marketKey)
        {

            List<NodePriceHelper> NodePriceList = new List<NodePriceHelper>();
            List<string> nodeNameList = new List<string>();
            int hour = Convert.ToInt32(hours);
            DateTime constraintDate = DateTime.MinValue;
            string constrint = selectedConstraint.ConstraintText;
            string contingency = selectedConstraint.ContingencyText;
            Dictionary<int, string> dictZones = new Dictionary<int, string>();
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {

                try
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        DateTime startDate = fromdate.AddHours(hour - 1);
                        DateTime endDate = fromdate.AddHours(hour);
                        if (marketKey == 9)
                        {
                            cmd.CommandText = "select top 1 MarketDateTime  from  Vayu..ConstraintRT where  ConstraintText = @ConstraintText " +
                                           "and ContingencyText = @ContingencyText and MarketDateTime >= @FromDate and MarketDateTime <= @ToDate order by ShadowPrice desc  ";

                        }
                        cmd.Parameters.AddWithValue("@ConstraintText", constrint);
                        cmd.Parameters.AddWithValue("@ContingencyText", contingency);
                        cmd.Parameters.AddWithValue("@FromDate", startDate);
                        cmd.Parameters.AddWithValue("@ToDate", endDate);
                        cmd.Connection = con;
                        constraintDate = Convert.ToDateTime(cmd.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error While Fetching Constraint Date");
                }
                con.Close();
            }
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {

                try
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        if (marketKey == 1)
                            cmd.CommandText = "select distinct SourceName from EESPathList ";
                        else if (marketKey == 9)
                            cmd.CommandText = "select distinct SourceName from  Vayu..EESPathList ";

                        cmd.Connection = con;
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string nodeName = rdr.GetValue(0).ToString();
                            nodeNameList.Add(nodeName);
                        }
                        rdr.Close();
                        if (marketKey == 1)
                            cmd.CommandText = "select distinct SinkName from EESPathList ";
                        else if (marketKey == 9)
                            cmd.CommandText = "select distinct SinkName from  Vayu..EESPathList ";

                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string nodeName = rdr.GetValue(0).ToString();
                            if (!nodeNameList.Contains(nodeName))
                            {
                                nodeNameList.Add(nodeName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error While Fetching Constraint Date");
                }
                con.Close();
            }
            dictZones = Zones(marketKey);
            List<Node> PriceList = RunLMP(marketKey, constraintDate, constraintDate.AddMinutes(5), "Uptos").ToList();
            foreach (Node nodePrice in PriceList)
            {
                if (!nodeNameList.Contains(nodePrice.NodeName))
                    continue;
                NodePriceHelper helper = new NodePriceHelper();
                helper.MktDateTime = nodePrice.LmpTimePriceList.First(a => a.MarketTime == constraintDate.AddSeconds(-constraintDate.Second)).MarketTime;
                helper.NodeName = nodePrice.NodeName;
                helper.Zone = dictZones[nodePrice.NodeId];
                helper.LMP = Math.Round(nodePrice.LmpTimePriceList.First(a => a.MarketTime == constraintDate.AddSeconds(-constraintDate.Second)).Lmp.Price, 2);
                helper.Congestion = Math.Round(nodePrice.LmpTimePriceList.First(a => a.MarketTime == constraintDate.AddSeconds(-constraintDate.Second)).Lmp.Congestion, 2);
                helper.Loss = Math.Round(nodePrice.LmpTimePriceList.First(a => a.MarketTime == constraintDate.AddSeconds(-constraintDate.Second)).Lmp.Loss, 2);
                NodePriceList.Add(helper);
                NodePriceList = NodePriceList.GroupBy(x => x.NodeName).Select(x => x.First()).ToList();
            }
            return NodePriceList;
        }

        public Dictionary<int, string> Zones(int Marketkey)
        {
            Dictionary<int, string> dictZones = new Dictionary<int, string>();
            try
            {
                using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                {

                    mSelectZoneCommand = new SqlCommand();
                    if (VayuConnection.State == ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    if (Marketkey == 1)
                        mSelectZoneCommand.CommandText = "Select distinct NodeKey,Zone from NewTrading..Node where MarketKey=1";
                    else
                        mSelectZoneCommand.CommandText = "Select distinct NodeKey,Zone from  Vayu..Node where MarketKey=9";
                    mSelectZoneCommand.Connection = VayuConnection;
                    SqlDataReader reader = mSelectZoneCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        if (!dictZones.ContainsKey(Convert.ToInt32(reader.GetValue(0))))
                            dictZones.Add(Convert.ToInt32(reader.GetValue(0)), Convert.ToString(reader.GetValue(1)));
                    }
                    reader.Close();
                    VayuConnection.Close();
                }
            }
            catch
            {

            }
            return dictZones;
        }
        #endregion

        public List<NodePriceHelper> GetFiveMinsPRicesForFTR(DateTime fromdate, DateTime toDate, string hours, Constraint selectedConstraint, int marketKey)
        {
            List<NodePriceHelper> NodePriceList = new List<NodePriceHelper>();
            List<string> nodeNameList = new List<string>();
            int hour = Convert.ToInt32(hours);
            DateTime constraintDate = DateTime.MinValue;
            string constrint = selectedConstraint.ConstraintText;
            string contingency = selectedConstraint.ContingencyText;
            Dictionary<int, string> dictZones = new Dictionary<int, string>();
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                con.Open();
                try
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        DateTime startDate = fromdate.AddHours(hour - 1);
                        DateTime endDate = fromdate.AddHours(hour);
                        if (marketKey == 1)
                        {
                            cmd.CommandText = "select top 1 MarketDateTime  from pjm.ConstraintRT where  ConstraintText = @ConstraintText " +
                                     "and ContingencyText = @ContingencyText and MarketDateTime >= @FromDate and MarketDateTime <= @ToDate order by ShadowPrice desc  ";

                        }
                        else if (marketKey == 9)
                        {
                            cmd.CommandText = "select top 1 MarketDateTime  from  Vayu..ConstraintRT where  ConstraintText = @ConstraintText " +
                                           "and ContingencyText = @ContingencyText and MarketDateTime >= @FromDate and MarketDateTime <= @ToDate order by ShadowPrice desc  ";

                        }
                        cmd.Parameters.AddWithValue("@ConstraintText", constrint);
                        cmd.Parameters.AddWithValue("@ContingencyText", contingency);
                        cmd.Parameters.AddWithValue("@FromDate", startDate);
                        cmd.Parameters.AddWithValue("@ToDate", endDate);
                        cmd.Connection = con;
                        constraintDate = Convert.ToDateTime(cmd.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error While Fetching Constraint Date");
                }
                con.Close();
            }
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {

                try
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = " select distinct n.nodename from PJM.FTRAuctionNodePrice f,Node n where f.NodeKey=n.NodeKey";
                        cmd.Connection = con;
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string nodeName = rdr.GetValue(0).ToString();
                            nodeNameList.Add(nodeName);
                        }
                        rdr.Close();

                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error While Fetching Constraint Date");
                }
                con.Close();
            }
            dictZones = Zones(marketKey);
            List<Node> PriceList = RunLMP(marketKey, constraintDate, constraintDate.AddMinutes(5), "FTR").ToList();
            foreach (Node nodePrice in PriceList)
            {
                if (!nodeNameList.Contains(nodePrice.NodeName))
                    continue;
                NodePriceHelper helper = new NodePriceHelper();

                helper.MktDateTime = nodePrice.LmpTimePriceList.FirstOrDefault().MarketTime;// nodePrice.LmpTimePriceList.First(a => a.MarketTime == constraintDate).MarketTime;
                helper.NodeName = nodePrice.NodeName;
                helper.Zone = dictZones[nodePrice.NodeId];
                helper.LMP = Math.Round(nodePrice.LmpTimePriceList.FirstOrDefault().Lmp.Price, 2); //Math.Round(nodePrice.LmpTimePriceList.First(a => a.MarketTime == constraintDate).Lmp.Price, 2);
                helper.Congestion = Math.Round(nodePrice.LmpTimePriceList.FirstOrDefault().Lmp.Congestion, 2); //Math.Round(nodePrice.LmpTimePriceList.First(a => a.MarketTime == constraintDate).Lmp.Congestion, 2);
                helper.Loss = Math.Round(nodePrice.LmpTimePriceList.FirstOrDefault().Lmp.Loss, 2);//Math.Round(nodePrice.LmpTimePriceList.First(a => a.MarketTime == constraintDate).Lmp.Loss, 2);
                NodePriceList.Add(helper);
                NodePriceList = NodePriceList.GroupBy(x => x.NodeName).Select(x => x.First()).ToList();
            }
            return NodePriceList;
        }

        public List<NodePriceHelper> GetFiveMinsPRicesForALL(DateTime fromdate, DateTime toDate, string hours, Constraint selectedConstraint, int marketKey)
        {

            List<NodePriceHelper> NodePriceList = new List<NodePriceHelper>();
            List<string> nodeNameList = new List<string>();
            int hour = Convert.ToInt32(hours);
            DateTime constraintDate = DateTime.MinValue;
            string constrint = selectedConstraint.ConstraintText;
            string contingency = selectedConstraint.ContingencyText;
            Dictionary<int, string> dictZones = new Dictionary<int, string>();
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {

                try
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        DateTime startDate = fromdate.AddHours(hour - 1);
                        DateTime endDate = fromdate.AddHours(hour);
                        if (marketKey == 1)
                        {
                            cmd.CommandText = "select top 1 MarketDateTime  from pjm.ConstraintRT where  ConstraintText = @ConstraintText " +
                                     "and ContingencyText = @ContingencyText and MarketDateTime >= @FromDate and MarketDateTime <= @ToDate order by ShadowPrice desc  ";

                        }
                        else if (marketKey == 9)
                        {
                            cmd.CommandText = "select top 1 MarketDateTime  from  Vayu..ConstraintRT where  ConstraintText = @ConstraintText " +
                                           "and ContingencyText = @ContingencyText and MarketDateTime >= @FromDate and MarketDateTime <= @ToDate order by ShadowPrice desc  ";

                        }
                        cmd.Parameters.AddWithValue("@ConstraintText", constrint);
                        cmd.Parameters.AddWithValue("@ContingencyText", contingency);
                        cmd.Parameters.AddWithValue("@FromDate", startDate);
                        cmd.Parameters.AddWithValue("@ToDate", endDate);
                        cmd.Connection = con;
                        constraintDate = Convert.ToDateTime(cmd.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error While Fetching Constraint Date");
                }
                con.Close();
            }
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {

                try
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "select distinct nodename from pjm.VirtualValidNodes ";
                        cmd.Connection = con;
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string nodeName = rdr.GetValue(0).ToString();
                            nodeNameList.Add(nodeName);
                        }
                        rdr.Close();

                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error While Fetching Constraint Date");
                }
                con.Close();
            }
            dictZones = Zones(marketKey);
            List<Node> PriceList = RunLMP(marketKey, constraintDate, constraintDate.AddMinutes(5), "All").ToList();
            foreach (Node nodePrice in PriceList)
            {
                if (!nodeNameList.Contains(nodePrice.NodeName))
                    continue;
                NodePriceHelper helper = new NodePriceHelper();
                helper.MktDateTime = nodePrice.LmpTimePriceList.First(a => a.MarketTime == constraintDate).MarketTime;
                helper.NodeName = nodePrice.NodeName;
                helper.Zone = dictZones[nodePrice.NodeId];
                helper.LMP = Math.Round(nodePrice.LmpTimePriceList.First(a => a.MarketTime == constraintDate).Lmp.Price, 2);
                helper.Congestion = Math.Round(nodePrice.LmpTimePriceList.First(a => a.MarketTime == constraintDate).Lmp.Congestion, 2);
                helper.Loss = Math.Round(nodePrice.LmpTimePriceList.First(a => a.MarketTime == constraintDate).Lmp.Loss, 2);
                NodePriceList.Add(helper);
                NodePriceList = NodePriceList.GroupBy(x => x.NodeName).Select(x => x.First()).ToList();
            }
            return NodePriceList;
        }

        class ConstraintHistoryhelper
        {
            public string ConstraintName { get; set; }
            public string ContingencyName { get; set; }
            public DateTime date { get; set; }
            public int hour { get; set; }
            public double shadowprice { get; set; }
        }
    }
    internal class ErcotConstraintHelper
    {
        public int Minute { get; set; }
        public int Seconds { get; set; }
        public double ShadowPrice { get; set; }
        public int Hourval { get; set; }
        public DateTime completedate { get; set; }
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
