using Vayu.NodePriceMonitorLibrary;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Timers;
using System.Data;
using System.Reflection;
using System.Xml;
using System.ServiceModel.Channels;
using Vayu.CommonAccessLibrary;

namespace Vayu.NodePriceMonitorServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class NodePriceServer : INodePrice
    {
        private static readonly object lockObj = new object();
        private static List<INodePriceCallback> sSubscriberList = new List<INodePriceCallback>();
        private static Dictionary<INodePriceCallback, string> sSubscriberHash = new Dictionary<INodePriceCallback, string>();
        private static Dictionary<string, Dictionary<string, double?>> mExanteDispatchCacheHash = new Dictionary<string, Dictionary<string, double?>>();
        private static Dictionary<string, List<HourlyNodePriceDetails>> mRTCacheHash = new Dictionary<string, List<HourlyNodePriceDetails>>();
        private static Dictionary<string, DateTime> mRTCacheLastUpdateHash = new Dictionary<string, DateTime>();
        private static Dictionary<string, List<HourlyNodePriceDetails>> mEDCacheHash = new Dictionary<string, List<HourlyNodePriceDetails>>();
        private static Dictionary<string, DateTime> mEDCacheLastUpdateHash = new Dictionary<string, DateTime>();
        private static Dictionary<INodePriceCallback, DateTime> mEDUserHash = new Dictionary<INodePriceCallback, DateTime>();
        private static int mCountSubscriber = 0;

        private static System.Timers.Timer sTimer = null;

        private SqlConnection VayuDBConnection;
        private SqlCommand mSelectMISOLmpCommand;
        private SqlCommand mSelectCAISOLmpCommand;
        private SqlCommand mSelectDispatchCommand;
        private SqlCommand mSelectExanteCommand;
        private SqlCommand mSelectERCOTminCommand;
        private SqlCommand mSelectERCOTFifteenMinCommand;
        private SqlCommand mSelectWHADLmpCommand;
        private SqlCommand mSelectPJMLmpCommand;
        public void initializeDB()
        {
            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            

            
            //
            mSelectWHADLmpCommand = new SqlCommand();
            mSelectWHADLmpCommand.CommandText = "Select MarketDate, HourPart + 1 HourPart, [0],[5],[10],[15],[20],[25],[30],[35],[40],[45],[50],[55]  " +
                                                "From ( select cast(AD.MarketDateTime as Date) MarketDate, DATEPART(hh,AD.MarketDateTime) HourPart, WH.LMP-AD.LMP Lmp, " +
                                                "DATEPART(MINUTE,AD.MarketDateTime) MinutePart " +
                                                "from pjm.Nodelmp WH Join pjm.Nodelmp AD on WH.MarketDateTime = AD.MarketDateTime " +
                                                "Where WH.NodeKey = 30 And AD.NodeKey = 21 And AD.MarketDateTime Between @start And @end " +
                                                "And WH.MarketDateTime Between @start And @end ) up " +
                                                "Pivot (Avg(Lmp) for MinutePart In ([0],[5],[10],[15],[20],[25],[30],[35],[40],[45],[50],[55])) AS Pvt Order By HourPart";
            mSelectWHADLmpCommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectWHADLmpCommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectWHADLmpCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectWHADLmpCommand.Connection = VayuDBConnection;
            //
            mSelectDispatchCommand = new SqlCommand();
            mSelectDispatchCommand.CommandText = "Select MarketDate, HourPart , [0],[5],[10],[15],[20],[25],[30],[35],[40],[45],[50],[55] " +
                                                 "From (Select cast(MarketDateTime as Date) MarketDate, DATEPART(hh, MarketDateTime) + 2 HourPart, Rate, " +
                                                 "DATEPART(MINUTE,MarketDateTime) MinutePart " +
                                                 "From CalculatedDispatch where marketdatetime between @start And @end) up  " +
                                                 "Pivot (Avg(Rate) for MinutePart In ([0],[5],[10],[15],[20],[25],[30],[35],[40],[45],[50],[55])) AS Pvt  " +
                                                 "Order By HourPart";
            mSelectDispatchCommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectDispatchCommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectDispatchCommand.Connection = VayuDBConnection;
            //
            mSelectExanteCommand = new SqlCommand();
            mSelectExanteCommand.CommandText = "Select MarketDate, HourPart , [0],[5],[10],[15],[20],[25],[30],[35],[40],[45],[50],[55]  " +
                                               "From (Select cast(Interval as Date) MarketDate, DATEPART(hh, DateAdd(Minute,-5,Interval)) HourPart, lmp, " +
                                               "DATEPART(MINUTE,DateAdd(Minute,-5,Interval)) MinutePart  " +
                                               "From exante where NodeKey = @nodekey and Interval >= @start And Interval < @end) up  " +
                                               "Pivot (Avg(Lmp) for MinutePart In ([0],[5],[10],[15],[20],[25],[30],[35],[40],[45],[50],[55])) AS Pvt  " +
                                               "Order By HourPart";
            //"select lmp, updatedatetime, DateAdd(Minute,-5,Interval) Interval from exante where nodekey = @nodekey and interval = @start";
            mSelectExanteCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectExanteCommand.Parameters.AddWithValue("@start", "start");
            mSelectExanteCommand.Parameters.AddWithValue("@end", "end");
            mSelectExanteCommand.Connection = VayuDBConnection;
            //
            mSelectERCOTminCommand = new SqlCommand();
            mSelectERCOTminCommand.CommandText = "select Lmp, MarketDate, MarketHour, MarketMin from Vayu..NodeLmpMin where nodeKey = @nodekey and MarketDate >= @start  and MarketDate < @end "
                                               + "order by MarketDate, MarketHour, MarketMin";
            mSelectERCOTminCommand.Parameters.Add("@nodekey", typeof(Int16));
            mSelectERCOTminCommand.Parameters.Add("@start", typeof(string));
            mSelectERCOTminCommand.Parameters.Add("@end", typeof(string));
            mSelectERCOTminCommand.Connection = VayuDBConnection;
            //
            mSelectERCOTFifteenMinCommand = new SqlCommand();
            mSelectERCOTFifteenMinCommand.CommandText = "select MarketDateTime, lmp,FinalYN from Vayu..NodeLMP where NodeKey = @nodekey and MarketDateTime >= @start and MarketDateTime < @end order by MarketDateTime";
            mSelectERCOTFifteenMinCommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectERCOTFifteenMinCommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectERCOTFifteenMinCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectERCOTFifteenMinCommand.Connection = VayuDBConnection;
            //
            mSelectCAISOLmpCommand = new SqlCommand();
            mSelectCAISOLmpCommand.CommandText = " Select MarketDate, HourPart , [0],[5],[10],[15],[20],[25],[30],[35],[40],[45],[50],[55] " +
                                                 " From (Select cast(MarketDateTime as Date) MarketDate, DATEPART(hh, MarketDateTime) + 1 HourPart, LMP, " +
                                                 " DATEPART(MINUTE,MarketDateTime) MinutePart " +
                                                 " From Caiso.NodeLmp where marketdatetime between @start And @end And NodeKey = @nodekey) up " +
                                                 " Pivot (Avg(Lmp) for MinutePart In ([0],[5],[10],[15],[20],[25],[30],[35],[40],[45],[50],[55])) AS Pvt " +
                                                 " Order By HourPart";
            mSelectCAISOLmpCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectCAISOLmpCommand.Parameters.AddWithValue("@start", "start");
            mSelectCAISOLmpCommand.Parameters.AddWithValue("@end", "end");
            mSelectCAISOLmpCommand.Connection = VayuDBConnection;
        }

        public NodePriceServer()
        {
            initializeDB();
            if (sTimer == null)
            {
                StartTimer();
            }
        }

        private bool CompareValues(List<HourlyNodePriceDetails> cachelist, List<HourlyNodePriceDetails> newList)
        {
            if (cachelist.Count == newList.Count && newList.Zip(cachelist, (c, n) => new { c, n })
                                        .All(x => x.c.Hour == x.n.Hour && x.c.Min0 == x.n.Min0 && x.c.Min10 == x.n.Min10 && x.c.Min15 == x.n.Min15
                                            && x.c.Min20 == x.n.Min20 && x.c.Min25 == x.n.Min25 && x.c.Min30 == x.n.Min30 && x.c.Min35 == x.n.Min35
                                            && x.c.Min40 == x.n.Min40 && x.c.Min45 == x.n.Min45 && x.c.Min50 == x.n.Min50 && x.c.Min55 == x.n.Min55
                                            && x.c.MinuteCount == x.n.MinuteCount)) //&& x.c.DispatchHash == x.n.DispatchHash
            {
                return true;
            }
            else
                return false;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            sTimer.Enabled = false;
            DateTime currentTime = DateTime.Now;
            if (sSubscriberHash != null && sSubscriberHash.Count > 0)
            {
                List<INodePriceCallback> callbackKeys = sSubscriberHash.Keys.ToList<INodePriceCallback>();
                if (sSubscriberHash.Count == 0)
                {
                    sTimer.Enabled = true;
                    return;
                }
                foreach (INodePriceCallback callback in callbackKeys)
                {
                    string item = sSubscriberHash[callback];
                    string[] parameters = item.Split('@');
                    string key = parameters[0] + "@" + parameters[1] + "@" + parameters[2] + "@" + parameters[3];

                    string[] upminute = System.Configuration.ConfigurationManager.AppSettings[parameters[0]].ToString().Split(',');

                    try
                    {
                        List<HourlyNodePriceDetails> updatedList = new List<HourlyNodePriceDetails>();
                        DateTime dblastTime = GetLatestDateTime(int.Parse(parameters[0]));
                        float updateminute = float.Parse(upminute[1]);
                        DateTime lastUpdated = default(DateTime);
                        if (mRTCacheLastUpdateHash.ContainsKey(key))
                            lastUpdated = mRTCacheLastUpdateHash[key];

                        if (lastUpdated != dblastTime)
                        {
                            updatedList = GetHourHash(int.Parse(parameters[0]), int.Parse(parameters[1]), DateTime.Parse(parameters[2]));
                            //if (!CompareValues(mRTCacheHash[key], updatedList))
                            {
                                mRTCacheHash.Remove(key);
                                mRTCacheHash.Add(key, updatedList);
                                mRTCacheLastUpdateHash.Remove(key);
                                mRTCacheLastUpdateHash.Add(key, dblastTime);

                            }
                        }
                        try
                        {
                            if (mRTCacheHash.ContainsKey(key))
                                callback.SendLmpPrint(mRTCacheHash[key]);
                        }
                        catch (Exception)
                        {
                            sSubscriberHash.Remove(callback);
                        }

                        #region comment
                        //if (lastSendTime < lastUpdated)
                        //{
                        //    mRTUserHash.Remove(callback);
                        //    mRTUserHash.Add(callback, currentTime);
                        //    callback.SendLmpPrint(mRTCacheHash[key]);
                        //}
                        //}
                        //else
                        //{
                        //    updatedList = GetHourHash(int.Parse(parameters[0]), int.Parse(parameters[1]), DateTime.Parse(parameters[2]));
                        //    mRTCacheLastUpdateHash.Add(key, dblastTime);
                        //    mRTCacheHash.Add(key, updatedList);
                        //    //mRTUserHash.Remove(callback);
                        //    //mRTUserHash.Add(callback, currentTime);
                        //    callback.SendLmpPrint(updatedList);
                        //}
                        #endregion comment
                        if (mEDUserHash.ContainsKey(callback))
                        {
                            DateTime lastSendTime = mEDUserHash[callback];
                            updateminute = float.Parse(upminute[0]);
                            if (parameters[3] != string.Empty)
                            {
                                if (mEDCacheLastUpdateHash.ContainsKey(key))
                                {
                                    lastUpdated = mEDCacheLastUpdateHash[key];
                                    TimeSpan diff = currentTime - lastUpdated;

                                    if (diff.TotalMinutes > updateminute)
                                    {
                                        updatedList = GetExanteDispatch(int.Parse(parameters[0]), int.Parse(parameters[1]), DateTime.Parse(parameters[2]));
                                        if (!CompareValues(mEDCacheHash[key], updatedList))
                                        {
                                            mEDCacheHash.Remove(key);
                                            mEDCacheHash.Add(key, updatedList);
                                            mEDCacheLastUpdateHash.Remove(key);
                                            mEDCacheLastUpdateHash.Add(key, currentTime);
                                            lastUpdated = currentTime;
                                        }
                                    }

                                    if (lastSendTime < lastUpdated)
                                    {
                                        mEDUserHash.Remove(callback);
                                        mEDUserHash.Add(callback, currentTime);
                                        callback.SendLmpDispatch(mEDCacheHash[key]);
                                    }
                                }
                                else
                                {
                                    updatedList = GetExanteDispatch(int.Parse(parameters[0]), int.Parse(parameters[1]), DateTime.Parse(parameters[2]));
                                    mEDCacheLastUpdateHash.Add(key, currentTime);
                                    mEDCacheHash.Add(key, updatedList);
                                    mEDUserHash.Remove(callback);
                                    mEDUserHash.Add(callback, currentTime);
                                    callback.SendLmpDispatch(updatedList);
                                }
                            }
                        }
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        mEDUserHash.Remove(callback);
                        //mRTUserHash.Remove(callback);
                        sSubscriberHash.Remove(callback);
                        Console.WriteLine("Total Users: {0}", --mCountSubscriber);
                    }
                    catch (Exception)
                    {
                        //sSubscriberHash.Remove(callback);
                        //Console.WriteLine("Total Users: {0}", --mCountSubscriber);
                    }
                }
            }
            sTimer.Enabled = true;
        }

        //private double GetPrice(SqlDataReader reader)
        //{
        //    double price = 0;
        //    int i = 0;
        //    DateTime dateTime = DateTime.Today;
        //    while (reader.Read())
        //    {
        //        string zone = reader.GetString(0);
        //        double rate = reader.GetDouble(1);
        //        dateTime = reader.GetDateTime(2);
        //        if (zone == "PENELEC")
        //        {
        //            if (i == 0)
        //            {
        //                price = 0;
        //            }
        //            price += (0.542069 * rate);
        //            i++;
        //        }
        //        if (zone == "PEP")
        //        {
        //            if (i == 0)
        //            {
        //                price = 0;
        //            }
        //            price += (0.429916 * rate);
        //            i++;
        //        }
        //        if (zone == "DOM")
        //        {
        //            if (i == 0)
        //            {
        //                price = 0;
        //            }
        //            price += (0.009346 * rate);
        //            i++;
        //        }
        //        if (zone == "BC")
        //        {
        //            if (i == 0)
        //            {
        //                price = 0;
        //            }
        //            price += (0.009346 * rate);
        //            i++;
        //        }
        //        if (zone == "METED")
        //        {
        //            if (i == 0)
        //            {
        //                price = 0;
        //            }
        //            price += (0.009346 * rate);
        //            i++;
        //        }
        //        if (i == 5)
        //        {
        //            i = 0;
        //        }
        //    }
        //    //DispatchRate dispatchRate = new DispatchRate();
        //    //dispatchRate.Price = price;
        //    //dispatchRate.DispatchRateTime = dateTime.AddHours(1);
        //    return price;
        //}

        private List<HourlyNodePriceDetails> GetHourHash(int market, int node, DateTime rtDate)
        {
            List<HourlyNodePriceDetails> hourlyLmpList = new List<HourlyNodePriceDetails>();
            SqlDataReader reader = null;

            try
            {
                if (VayuDBConnection.State == ConnectionState.Closed)
                {
                    VayuDBConnection.Open();
                }
                //mSelectMISOLmpCommand.Parameters["@MarketValue"].Value = market;
                //if (market == 2)
                //{
                //    mSelectMISOLmpCommand.Parameters["@start"].Value = rtDate.AddMinutes(1);
                //    mSelectMISOLmpCommand.Parameters["@end"].Value = rtDate.AddDays(1).AddMinutes(1);
                //    mSelectMISOLmpCommand.Parameters["@nodekey"].Value = node;       //miso
                //    reader = mSelectMISOLmpCommand.ExecuteReader();
                //}
                //else if (market == 1)
                //{
                //    if (node == 897557)
                //    {
                //        mSelectWHADLmpCommand.Parameters["@start"].Value = rtDate;
                //        mSelectWHADLmpCommand.Parameters["@end"].Value = rtDate.AddDays(1);
                //        mSelectWHADLmpCommand.Parameters["@nodekey"].Value = node;           //PJM
                //        reader = mSelectWHADLmpCommand.ExecuteReader();
                //    }
                //    else
                //    {
                //        mSelectPJMLmpCommand.Parameters["@start"].Value = rtDate;
                //        mSelectPJMLmpCommand.Parameters["@end"].Value = rtDate.AddDays(1);
                //        mSelectPJMLmpCommand.Parameters["@nodekey"].Value = node;
                //        mSelectPJMLmpCommand.Parameters["@MarketValue"].Value = market;//PJM
                //        reader = mSelectPJMLmpCommand.ExecuteReader();
                //    }
                //}
                //else if (market == 7)
                //{
                //    mSelectCAISOLmpCommand.Parameters["@start"].Value = rtDate;
                //    mSelectCAISOLmpCommand.Parameters["@end"].Value = rtDate.AddDays(1);
                //    mSelectCAISOLmpCommand.Parameters["@nodekey"].Value = node;          //caiso
                //    reader = mSelectCAISOLmpCommand.ExecuteReader();
                //}
                //else
                if (market == 9)
                {
                    hourlyLmpList = GetErcotHourHash(market, node, rtDate);
                }

                double? nullValue = null;

                if (market != 9)
                {
                    while (reader.Read())
                    {
                        HourlyNodePriceDetails lmphourlyprice = new HourlyNodePriceDetails();
                        DateTime pricedate = DateTime.Parse(reader[0].ToString());
                        lmphourlyprice.Hour = int.Parse(reader[1].ToString());
                        double total = 0;
                        int avgcount = 0;
                        for (int i = 0; i < 12; i++)
                        {
                            double? price = reader.IsDBNull(i + 2) ? nullValue : double.Parse(reader[i + 2].ToString());
                            string propertyName = "Min" + (i * 5);
                            if (price != null)
                            {
                                lmphourlyprice.GetType().GetProperty(propertyName).SetValue(lmphourlyprice, double.Parse(reader[i + 2].ToString()), null);
                                total += double.Parse(price.ToString());
                                avgcount++;
                            }
                        }
                        lmphourlyprice.RT = total / (double)avgcount;
                        lmphourlyprice.RTSum = total;
                        lmphourlyprice.MinuteCount = avgcount;
                        hourlyLmpList.Add(lmphourlyprice);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                VayuDBConnection.Close();
            }
            return hourlyLmpList;
        }

        private List<HourlyNodePriceDetails> GetExanteDispatch(int market, int node, DateTime rtDate)
        {
            double? nullValue = null;
            List<HourlyNodePriceDetails> hourlyLmpList = new List<HourlyNodePriceDetails>();
            if (market == 1)
            {
                mSelectDispatchCommand.Parameters["@start"].Value = rtDate;
                mSelectDispatchCommand.Parameters["@end"].Value = rtDate.AddDays(1);
                if (VayuDBConnection.State == ConnectionState.Closed)
                {
                    VayuDBConnection.Open();
                }
                SqlDataReader dispatchreader = mSelectDispatchCommand.ExecuteReader();
                while (dispatchreader.Read())
                {
                    HourlyNodePriceDetails lmphourlyprice = new HourlyNodePriceDetails();
                    DateTime pricedate = DateTime.Parse(dispatchreader[0].ToString());
                    lmphourlyprice.Hour = int.Parse(dispatchreader[1].ToString());
                    for (int i = 0; i < 12; i++)
                    {
                        double? price = dispatchreader.IsDBNull(i + 2) ? nullValue : double.Parse(dispatchreader[i + 2].ToString());
                        string propertyName = "Min" + (i * 5);
                        if (price != null)
                        {
                            lmphourlyprice.GetType().GetProperty(propertyName).SetValue(lmphourlyprice, double.Parse(dispatchreader[i + 2].ToString()), null);
                            lmphourlyprice.MinuteCount++;
                        }
                    }
                    hourlyLmpList.Add(lmphourlyprice);
                }
                dispatchreader.Close();
                return hourlyLmpList;
            }
            else if (market == 2)
            {
                node = 10788;
                mSelectExanteCommand.Parameters["@nodekey"].Value = node;
                mSelectExanteCommand.Parameters["@start"].Value = rtDate.AddHours(1).AddMinutes(1);
                mSelectExanteCommand.Parameters["@end"].Value = rtDate.AddDays(1).AddHours(1);
                if (VayuDBConnection.State == ConnectionState.Closed)
                {
                    VayuDBConnection.Open();
                }
                SqlDataReader exantereader = mSelectExanteCommand.ExecuteReader();


                while (exantereader.Read())
                {
                    HourlyNodePriceDetails lmphourlyprice = new HourlyNodePriceDetails();
                    DateTime pricedate = DateTime.Parse(exantereader[0].ToString());
                    lmphourlyprice.Hour = int.Parse(exantereader[1].ToString());
                    for (int i = 0; i < 12; i++)
                    {
                        double? price = exantereader.IsDBNull(i + 2) ? nullValue : double.Parse(exantereader[i + 2].ToString());
                        string propertyName = "Min" + (i * 5);
                        if (price != null)
                        {
                            lmphourlyprice.GetType().GetProperty(propertyName).SetValue(lmphourlyprice, double.Parse(exantereader[i + 2].ToString()), null);
                            lmphourlyprice.MinuteCount++;
                        }
                    }
                    hourlyLmpList.Add(lmphourlyprice);
                }
                exantereader.Close();
                return hourlyLmpList;
            }
            else
                return null;
        }

        public void StartTimer()
        {
            try
            {
                sTimer = new System.Timers.Timer(20000) { Enabled = true };
                sTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " " + ex.Message);
            }
        }

        public void Connect()
        {
            using (ServiceHost host = new ServiceHost(this, new Uri("net.tcp://localhost:8012")))
            {
                NetTcpBinding myBinding = new NetTcpBinding();
                myBinding.OpenTimeout = new TimeSpan(0, 30, 0);
                myBinding.SendTimeout = new TimeSpan(0, 12, 0);
                myBinding.ReceiveTimeout = new TimeSpan(0, 12, 0);
                myBinding.CloseTimeout = new TimeSpan(0, 12, 0);
                myBinding.TransactionFlow = false;
                myBinding.MaxReceivedMessageSize = int.MaxValue;
                myBinding.MaxBufferPoolSize = int.MaxValue;
                myBinding.MaxBufferSize = int.MaxValue;
                myBinding.TransferMode = TransferMode.Buffered;
                myBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
                myBinding.Security.Mode = SecurityMode.None;
                host.AddServiceEndpoint(typeof(INodePrice), myBinding, "ISubscribe");
                try
                {
                    host.Open();
                    Console.WriteLine(" Successfully opened port 8012 ");
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        public bool SubscribeLMPServer(int market, int hub, DateTime lmpDate, string tempnode)
        {
            lock (lockObj)
            {
                try
                {
                    INodePriceCallback callback = OperationContext.Current.GetCallbackChannel<INodePriceCallback>();
                    if (!sSubscriberHash.ContainsKey(callback))
                    {
                        sSubscriberHash.Add(callback, market.ToString() + "@" + hub.ToString() + "@" + lmpDate.ToString() + "@" + tempnode);
                        mEDUserHash.Add(callback, lmpDate);
                        Console.WriteLine("Total Users: {0}", ++mCountSubscriber);
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool HeartBeat()
        {
            Console.WriteLine(DateTime.Now + " HeartBeat " + GetSubscriberIpAddress(OperationContext.Current));
            return true;
        }
        private string GetSubscriberIpAddress(OperationContext operationContext)
        {
            return ((operationContext.IncomingMessageProperties as MessageProperties)[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address;
        }
        public List<HourlyNodePriceDetails> GetLmpPrint(int market, int node, DateTime lmpDate)
        {
            return GetHourHash(market, node, lmpDate);
        }

        private List<HourlyNodePriceDetails> GetErcotHourHash(int market, int node, DateTime date)
        {

            List<HourlyNodePriceDetails> hourlyLmpList = new List<HourlyNodePriceDetails>();

            DateTime marketDateTime = DateTime.Today;
            Dictionary<int, Dictionary<int, double>> hourHash = new Dictionary<int, Dictionary<int, double>>();
            try
            {
                mSelectERCOTFifteenMinCommand.Parameters["@start"].Value = date;
                mSelectERCOTFifteenMinCommand.Parameters["@end"].Value = date.AddDays(1);
                mSelectERCOTFifteenMinCommand.Parameters["@nodekey"].Value = 57457;

                mSelectERCOTminCommand.Parameters["@start"].Value = date;
                mSelectERCOTminCommand.Parameters["@end"].Value = date.AddDays(1);
                mSelectERCOTminCommand.Parameters["@nodekey"].Value = 57457;

                SqlDataReader ercotreader = mSelectERCOTminCommand.ExecuteReader();
                Dictionary<int, Dictionary<int, double>> temphourHash = new Dictionary<int, Dictionary<int, double>>();
                Dictionary<int, Dictionary<int, double>> tempFifteenHash = new Dictionary<int, Dictionary<int, double>>();

                while (ercotreader.Read())
                {
                    Dictionary<int, double> lmpHash = new Dictionary<int, double>();
                    double lmp = (double)ercotreader.GetDecimal(0);
                    DateTime date1 = ercotreader.GetDateTime(1);
                    int hour = ercotreader.GetInt32(2);
                    int min = ercotreader.GetInt32(3);
                    if (temphourHash.ContainsKey(hour))
                    {
                        lmpHash = temphourHash[hour];
                        temphourHash.Remove(hour);
                    }

                    if (lmpHash.ContainsKey(min))
                    {
                        lmpHash.Remove(min);
                    }
                    lmpHash.Add(min, lmp);
                    temphourHash.Add(hour, lmpHash);
                }
                ercotreader.Close();

                IDataReader ercotFifteenReader = mSelectERCOTFifteenMinCommand.ExecuteReader();
                while (ercotFifteenReader.Read())
                {
                    Dictionary<int, double> lmpHash = new Dictionary<int, double>();
                    double lmp = ercotFifteenReader.IsDBNull(1) ? 0.0 : Convert.ToDouble(ercotFifteenReader.GetValue(1));
                    DateTime date1 = ercotFifteenReader.IsDBNull(0) ? DateTime.Now : ercotFifteenReader.GetDateTime(0);
                    int hour = date1.Hour;
                    int min = date1.Minute;
                    if (tempFifteenHash.ContainsKey(hour))
                    {
                        lmpHash = tempFifteenHash[hour];
                        tempFifteenHash.Remove(hour);
                    }

                    if (lmpHash.ContainsKey(min))
                    {
                        lmpHash.Remove(min);
                    }
                    lmpHash.Add(min, lmp);
                    tempFifteenHash.Add(hour, lmpHash);
                }
                ercotFifteenReader.Close();
                VayuDBConnection.Close();
                double fiveminsum = 0;
                double count = 0;

                //calculate five minute average
                foreach (KeyValuePair<int, Dictionary<int, double>> key in temphourHash)
                {
                    int i = 0;
                    int j = 0;
                    double lmp = 0;
                    Dictionary<int, double> minHash = new Dictionary<int, double>();
                    while (i < 65)
                    {
                        if (date >= DateTime.Today.Date)
                        {
                            if (key.Key >= DateTime.Now.Hour && i > DateTime.Now.Minute)
                            {
                                break;
                            }
                        }

                        if (key.Value.ContainsKey(i))
                        {
                            lmp = key.Value[i];
                        }
                        if (key.Key == 0 && i == 0) j = 1;
                        if (i % 5 == 0 && (key.Key != 0 || i != 0))
                        {
                            int k = j * 5;
                            minHash.Add(k, lmp);
                            if (lmp == 0)
                            {
                                minHash.Remove(k);
                            }
                            fiveminsum = 0;
                            count = 0;
                            j++;

                        }
                        //ignore zero price
                        if (lmp != 0)
                        {
                            fiveminsum += lmp;
                            count++;
                        }
                        i++;
                    }
                    hourHash.Add(key.Key, minHash);
                }

                foreach (var item in hourHash.Keys)
                {
                    var itemHash = hourHash[item];
                    itemHash.Keys.ToList().ForEach(m =>
                    {
                        if (tempFifteenHash.ContainsKey(item))
                        {
                            var minHash = tempFifteenHash[item];
                            try
                            {
                                minHash.Keys.ToList().ForEach(a =>
                                                   {
                                                       if (a == 0)
                                                       {
                                                           if (hourHash.ContainsKey(item - 1))
                                                               if (hourHash[item - 1].ContainsKey(60))
                                                                   hourHash[item - 1][60] = Math.Round(minHash[a], 2);

                                                       }
                                                       else if (a == m)
                                                       {
                                                           hourHash[item][m] = Math.Round(minHash[a], 2);
                                                       }
                                                   });
                            }
                            catch
                            {
                            }
                        }
                    });
                }
                foreach (KeyValuePair<int, Dictionary<int, double>> key in hourHash)
                {
                    Dictionary<int, double?> updatedValue = new Dictionary<int, double?>();
                    key.Value.Keys.ToList().ForEach(m => updatedValue.Add(m - 5, double.IsNaN(key.Value[m]) ? null : (double?)key.Value[m]));
                    HourlyNodePriceDetails lmphourlyprice = new HourlyNodePriceDetails();
                    foreach (PropertyInfo item in lmphourlyprice.GetType().GetProperties())
                    {
                        var name = item.Name.Split('n');
                        int minNo;
                        if (int.TryParse(name[name.Length - 1], out minNo))
                        {

                            if (updatedValue.ContainsKey(minNo))
                            {
                                switch (minNo)
                                {
                                    case 0:
                                        lmphourlyprice.Min0 = updatedValue[minNo];
                                        break;
                                    case 5:
                                        lmphourlyprice.Min5 = updatedValue[minNo];
                                        break;
                                    case 10:
                                        lmphourlyprice.Min10 = updatedValue[minNo];
                                        break;
                                    case 15:
                                        lmphourlyprice.Min15 = updatedValue[minNo];
                                        if (tempFifteenHash.ContainsKey(key.Key))
                                        {
                                            if (!tempFifteenHash[key.Key].ContainsKey(minNo))
                                            {
                                                if (hourlyLmpList.Count > 0)
                                                {
                                                    hourlyLmpList[hourlyLmpList.Count - 1].Min15 = null;
                                                }
                                            }
                                            //else
                                            //{
                                            //    double? temptotoal = updatedValue.ContainsKey(minNo - 5) ? updatedValue[minNo - 5] : 0.0;
                                            //    if (temptotoal != 0.0 && !double.IsNaN(temptotoal.Value))
                                            //    {
                                            //        total = total + (double)temptotoal;
                                            //        avgcount++;
                                            //    }
                                            //}
                                        }
                                        break;
                                    case 20:
                                        lmphourlyprice.Min20 = updatedValue[minNo];
                                        break;
                                    case 25:
                                        lmphourlyprice.Min25 = updatedValue[minNo];
                                        break;
                                    case 30:
                                        lmphourlyprice.Min30 = updatedValue[minNo];
                                        if (tempFifteenHash.ContainsKey(key.Key))
                                        {
                                            if (!tempFifteenHash[key.Key].ContainsKey(minNo))
                                            {
                                                if (hourlyLmpList.Count > 0)
                                                {
                                                    hourlyLmpList[hourlyLmpList.Count - 1].Min30 = null;
                                                }
                                            }
                                        }
                                        break;
                                    case 35:
                                        lmphourlyprice.Min35 = updatedValue[minNo];
                                        break;
                                    case 40:
                                        lmphourlyprice.Min40 = updatedValue[minNo];
                                        break;
                                    case 45:
                                        lmphourlyprice.Min45 = updatedValue[minNo];
                                        if (tempFifteenHash.ContainsKey(key.Key))
                                        {
                                            if (!tempFifteenHash[key.Key].ContainsKey(minNo))
                                            {
                                                if (hourlyLmpList.Count > 0)
                                                {
                                                    hourlyLmpList[hourlyLmpList.Count - 1].Min45 = null;
                                                }
                                            }
                                        }
                                        break;
                                    case 50:
                                        lmphourlyprice.Min50 = updatedValue[minNo];
                                        break;
                                    case 55:
                                        lmphourlyprice.Min55 = updatedValue[minNo];
                                        if (tempFifteenHash.ContainsKey(key.Key))
                                        {
                                            if (!tempFifteenHash[key.Key].ContainsKey(0))
                                            {
                                                if (hourlyLmpList.Count > 0)
                                                {
                                                    hourlyLmpList[hourlyLmpList.Count - 1].Min55 = null;
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                                lmphourlyprice.GetType().GetProperty(item.Name).SetValue(lmphourlyprice, null);
                        }
                    }
                    hourlyLmpList.Add(lmphourlyprice);
                }
                hourlyLmpList.ForEach(a =>
                {
                    //a.RT = GetRtPrice(a.Min10, a.Min25, a.Min40, a.Min55);
                    //a.RTSum = GetTotal(a.Min10, a.Min25, a.Min40, a.Min55);
                    //a.MinuteCount = GetAverageCount(a.Min10, a.Min25, a.Min40, a.Min55);
                    if (!a.Min10.HasValue)
                        a.Min10 = Get15MinValues(a.Min0, a.Min5);
                    if (!a.Min25.HasValue)
                        a.Min25 = Get15MinValues(a.Min15, a.Min20);
                    if (!a.Min40.HasValue)
                        a.Min40 = Get15MinValues(a.Min30, a.Min35);
                    if (!a.Min55.HasValue)
                        a.Min55 = Get15MinValues(a.Min45, a.Min50);
                });
                hourlyLmpList = hourlyLmpList.ToList();
                hourlyLmpList.ForEach(a =>
                {
                    a.RT = GetRtPrice(a.Min10, a.Min25, a.Min40, a.Min55);
                    a.RTSum = GetTotal(a.Min10, a.Min25, a.Min40, a.Min55);
                    a.MinuteCount = GetAverageCount(a.Min10, a.Min25, a.Min40, a.Min55);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return hourlyLmpList.ToList();
        }

        private double? Get15MinValues(double? nullable1, double? nullable2)
        {
            if (nullable1 == null && nullable2 != null)
                return nullable2;
            else if (nullable1 != null && nullable2 == null)
                return nullable1;
            else if (nullable1.HasValue && nullable2.HasValue)
                return (nullable1 + nullable2) / 2;
            else return null;
        }

        private int GetAverageCount(double? nullable1, double? nullable2, double? nullable3, double? nullable4)
        {
            int count = 0;
            if (nullable1 != null)
                count += 1;
            if (nullable2 != null)
                count += 1;
            if (nullable3 != null)
                count += 1;
            if (nullable4 != null)
                count += 1;
            return count;
        }

        private double? GetTotal(double? nullable1, double? nullable2, double? nullable3, double? nullable4)
        {
            int count = 0;
            double? total = 0;
            if (nullable1 != null)
            {
                count += 1;
                total += nullable1;
            }
            if (nullable2 != null)
            {
                count += 1;
                total += nullable2;
            }
            if (nullable3 != null)
            {
                count += 1;
                total += nullable3;
            }
            if (nullable4 != null)
            {
                count += 1;
                total += nullable4;
            }
            return total;
        }

        private double? GetRtPrice(double? nullable1, double? nullable2, double? nullable3, double? nullable4)
        {
            int count = 0;
            double? total = 0;
            if (nullable1 != null)
            {
                count += 1;
                total += nullable1;
            }
            if (nullable2 != null)
            {
                count += 1;
                total += nullable2;
            }
            if (nullable3 != null)
            {
                count += 1;
                total += nullable3;
            }
            if (nullable4 != null)
            {
                count += 1;
                total += nullable4;
            }
            return count == 0 ? null : ((double?)(total / count));
        }

        public bool Subscribe(bool isAlgo)
        {
            return false;
        }

        public bool Unsubscribe(bool isAlgo)
        {
            lock (lockObj)
            {
                try
                {
                    INodePriceCallback callback = OperationContext.Current.GetCallbackChannel<INodePriceCallback>();
                    if (sSubscriberHash.ContainsKey(callback))
                    {
                        mEDUserHash.Remove(callback);
                        //mRTUserHash.Remove(callback);
                        sSubscriberHash.Remove(callback);

                        Console.WriteLine("Total Users: {0}", --mCountSubscriber);
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private DateTime GetLatestDateTime(int market)
        {
            try
            {
                SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection(); 
                SqlCommand cmd = con.CreateCommand();
                //cmd.CommandText = "select max(MarketDateTime) from PJM.Nodelmp nolock where nodekey = 30";
                cmd.CommandText = "select max(MarketDateTime) from Nodelmp nolock where nodekey = 57457";
                if(cmd.Connection.State==ConnectionState.Closed)
                  cmd.Connection.Open();
                DateTime? dt = cmd.ExecuteScalar() as DateTime?;
                cmd.Connection.Close();
                return dt.GetValueOrDefault();
            }
            catch { return default(DateTime); }
        }
    }
}
