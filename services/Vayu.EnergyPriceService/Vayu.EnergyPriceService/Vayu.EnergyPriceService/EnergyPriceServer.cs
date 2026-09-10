
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using Vayu.NodePriceLibrary;
using Vayu.CommonAccessLibrary;

namespace Vayu.EnergyPriceService
{

    /// <summary>
    /// Energy Price Service
    /// </summary>
  [ServiceBehavior(MaxItemsInObjectGraph = int.MaxValue, ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall)]
    class EnergyPriceServer: ILMPEnergyPrice
    {
        private SqlConnection SigmaErcotDBConnection;
        private SqlCommand cmdSelectRTEnergyPrices;
        private SqlCommand cmdSelectDAEnergyPrices;
        private static Dictionary<long, List<NodeChange>> dictOldNodeHash = new Dictionary<long, List<NodeChange>>();
        Dictionary<string, List<ErcotEnergyPriceHelper>> hourMinListDic= new Dictionary<string, List<ErcotEnergyPriceHelper>>();
        Dictionary<int, List<ErcotEnergyPriceHelper>> EPHourwiseDic = new Dictionary<int, List<ErcotEnergyPriceHelper>>();


        private void LoadDB()
        {
            SigmaErcotDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            cmdSelectRTEnergyPrices = new SqlCommand();
            cmdSelectRTEnergyPrices.CommandText="select  DeliveryDate, DAY(DeliveryDate),FORMAT(DeliveryDate,'HH'),FORMAT(DeliveryDate,'mm'),FORMAT(DeliveryDate,'ss'),SystemLambda from Vayu..RTSystemLambda where  " +
                                                 "DeliveryDate>=@StartDate and DeliveryDate <=@EndDate order by DeliveryDate desc";

            cmdSelectRTEnergyPrices.Parameters.Add("StartDate", SqlDbType.DateTime);
            cmdSelectRTEnergyPrices.Parameters.Add("EndDate",SqlDbType.DateTime);
            cmdSelectRTEnergyPrices.Connection = SigmaErcotDBConnection;



            cmdSelectDAEnergyPrices = new SqlCommand();
            cmdSelectDAEnergyPrices.CommandText = "select  DeliveryDate, DAY(DeliveryDate),FORMAT(DeliveryDate,'HH'),FORMAT(DeliveryDate,'mm'),FORMAT(DeliveryDate,'ss'),SystemLambda from Vayu..DAMSystemLambda where  " +
                                                 "DeliveryDate>=@StartDate and DeliveryDate <=@EndDate order by DeliveryDate desc";

            cmdSelectDAEnergyPrices.Parameters.Add("StartDate", SqlDbType.DateTime);
            cmdSelectDAEnergyPrices.Parameters.Add("EndDate", SqlDbType.DateTime);
            cmdSelectDAEnergyPrices.Connection = SigmaErcotDBConnection;


        }

        /// <summary>
        /// 
        /// </summary>
        public void Connect()
        {
           using (ServiceHost host = new ServiceHost(typeof(EnergyPriceServer), new Uri("net.tcp://localhost:8005")))
            //using (ServiceHost host = new ServiceHost(typeof(EnergyPriceServer), new Uri("net.tcp://localhost:7000")))
            {
                NetTcpBinding myBinding = new NetTcpBinding();
                myBinding.Security.Mode = SecurityMode.None;
                myBinding.OpenTimeout = new TimeSpan(0, 30, 0);
                myBinding.SendTimeout = new TimeSpan(0, 12, 0);
                myBinding.ReceiveTimeout = new TimeSpan(0, 12, 0);
                myBinding.CloseTimeout = new TimeSpan(0, 12, 0);
                myBinding.MaxBufferSize = int.MaxValue;
                myBinding.TransactionFlow = false;
                myBinding.MaxReceivedMessageSize = int.MaxValue;
                myBinding.MaxBufferPoolSize = int.MaxValue;
                myBinding.MaxBufferSize = int.MaxValue;
                myBinding.TransferMode = TransferMode.Buffered;
                myBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
                var behavior = new ServiceThrottlingBehavior()
                {
                    MaxConcurrentCalls = 10000,
                    MaxConcurrentInstances = 10000,
                    MaxConcurrentSessions = 1000
                };
                host.Description.Behaviors.Add(behavior);
                host.AddServiceEndpoint(typeof(ILMPEnergyPrice), myBinding, "ISubscribe");
                try
                {
                    host.Open();
                    Console.WriteLine("Energy Price server successfully opened port 8005.");
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
       
        
        

        public Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> RTEnergyPrices(DateTime startDate, DateTime endDate)
        {
            SqlDataReader reader;
            Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> DateHourWiseDic = new Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>>();
            List<string> DateList= new List<string>();
            Dictionary<int, Dictionary<double, Dictionary<int, double>>> EPAllhourlist;
            List<int> hourList;
            Dictionary<int, double> EPhourlist, EPMinWiselist; ;
            Dictionary<double, Dictionary<int, double>> EPHourWiselist;
            List<string> DatehourList;
            string mdate, day, key;
            int hourval,min,sec;
            double energyprice;
            DateTime Todate;
            hourMinListDic = new Dictionary<string, List<ErcotEnergyPriceHelper>>();
            if ((DateTime.Now - endDate).TotalDays < 1)
            {
                Todate = endDate.AddDays(1);
            }
            else
            {
                Todate = endDate.AddDays(1);
                Todate = Todate.AddMinutes(-1);
             }
            LoadDB();

            try 
            {

                if(SigmaErcotDBConnection.State==ConnectionState.Closed)
                  SigmaErcotDBConnection.Open();
                cmdSelectRTEnergyPrices.Parameters["StartDate"].Value = startDate.Date;
                cmdSelectRTEnergyPrices.Parameters["EndDate"].Value = Todate;
                reader = cmdSelectRTEnergyPrices.ExecuteReader();
                while (reader.Read())
                {
                     mdate   = reader.GetDateTime(0).ToString("yyyy-MM-dd");
                     day     = Convert.ToInt32(reader.GetValue(1)).ToString();
                     hourval = Convert.ToInt32(reader.GetValue(2));
                     min     = Convert.ToInt32(reader.GetValue(3));
                     sec     = Convert.ToInt32(reader.GetValue(4));

                    key = mdate + "?" + hourval;
                    energyprice = reader.IsDBNull(5) ? 0.0 : Convert.ToDouble(reader.GetValue(5));
                    ErcotEnergyPriceHelper helper = new ErcotEnergyPriceHelper();
                    helper.hour = hourval;
                    helper.Minute = min;
                    helper.Seconds = sec;
                    helper.EnergyPrice = energyprice;

                    List<ErcotEnergyPriceHelper> lstErcotEnergyPricetHelper = new List<ErcotEnergyPriceHelper>();
                    lstErcotEnergyPricetHelper.Add(helper);

                    if (!hourMinListDic.ContainsKey(key))
                    {
                        hourMinListDic.Add(key, lstErcotEnergyPricetHelper);
                    }
                    else
                        hourMinListDic[key].Add(helper);
                }
                reader.Close();


                DatehourList = hourMinListDic.Keys.ToList();
                EPAllhourlist = new Dictionary<int, Dictionary<double, Dictionary<int, double>>>();

                foreach (string val in DatehourList)
                {
                    string[] valarrya = val.Split('?');
                    int hr = Convert.ToInt32(valarrya[1]);
                    string datevar = valarrya[0];

                    if (!DateList.Contains(datevar))
                        DateList.Add(datevar);

                }

                foreach (var Datevalue in DateList)
                {
                    hourList = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };
                    EPhourlist = new Dictionary<int, double>();
                    EPMinWiselist = new Dictionary<int, double>();
                    EPHourWiselist = new Dictionary<double, Dictionary<int, double>>();
                    EPAllhourlist = new Dictionary<int, Dictionary<double, Dictionary<int, double>>>();

                    foreach (var hour in hourList)
                    {
                        double FinalEnergyPrice = 0;
                        string keyval = Datevalue + "?" + hour;

                        if (hourMinListDic.ContainsKey(keyval))
                        {
                            var hourMinuteCollection = hourMinListDic[keyval];
                            Dictionary<int, double> AllMinDictonary = new Dictionary<int, double>();
                            double SumallEP = 0;
                            EPhourlist = new Dictionary<int, double>();
                            EPMinWiselist = new Dictionary<int, double>();
                            EPHourWiselist = new Dictionary<double, Dictionary<int, double>>();
                            for (int j = 0; j <= 59; j++)
                            {
                                AllMinDictonary[j] = CalculateRTEnergyPrice(j, hourMinuteCollection, AllMinDictonary);
                                SumallEP = SumallEP + AllMinDictonary[j];
                                EPMinWiselist.Add(j, AllMinDictonary[j]);

                            }

                            DateTime loopDate = DateTime.Parse(Datevalue);
                            if ((DateTime.Now - loopDate).TotalDays < 1)
                            {
                                if (DateTime.Now.Hour == hour)
                                {
                                    FinalEnergyPrice = SumallEP / (DateTime.Now.Minute + 1);
                                }
                                else
                                    FinalEnergyPrice = SumallEP / 60;
                            }
                            else
                                FinalEnergyPrice = SumallEP / 60;

                            EPHourWiselist.Add(FinalEnergyPrice, EPMinWiselist);
                            EPAllhourlist.Add(hour, EPHourWiselist);

                        }

                    }//hour loop

                    if (!DateHourWiseDic.ContainsKey(Datevalue))
                        DateHourWiseDic.Add(Datevalue, EPAllhourlist);
                }//date loop

              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new FaultException(ex.Message);
            }
            

            return DateHourWiseDic;
            
        }

        double CalculateRTEnergyPrice(int min, List<ErcotEnergyPriceHelper> hourMinuteCollection, Dictionary<int, double> AllMinDictonary)
        {
            List<int> Minutelist = new List<int>();
            List<int> DoneMinutelist = new List<int>();
            double SPfinal = 0;

            Minutelist.Clear();
            DoneMinutelist.Clear();
            for (int i = 0; i < hourMinuteCollection.Count; i++)
            {
                Minutelist.Add(hourMinuteCollection.ElementAt(i).Minute);
            }

            int length = hourMinuteCollection.Count;
            int currentMin = min;
            int prevmin;

            int count = Minutelist.ToArray().Count(x => x == currentMin);

            if (Minutelist.Contains(currentMin))
            {
                if (count > 1)
                {
                    for (int j = 0; j < hourMinuteCollection.Count; j++)
                    {
                        if (hourMinuteCollection.ElementAt(j).Minute == currentMin)
                        {
                            SPfinal += hourMinuteCollection.ElementAt(j).EnergyPrice;
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
                            SPfinal = hourMinuteCollection.ElementAt(j).EnergyPrice;
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

            return SPfinal;
        }


        public Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> DAEnergyPrices(DateTime startDate, DateTime endDate)
        {
            SqlDataReader reader;
            Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> DateHourWiseDic = new Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>>();
            List<string> DateList = new List<string>();
            Dictionary<int, Dictionary<double, Dictionary<int, double>>> EFAllhourlist;
            List<int> hourList;
            Dictionary<int, double> EPhourlist, EPMinWiselist; ;
            Dictionary<double, Dictionary<int, double>> EPHourWiselist;
            List<string> DatehourList;
            string mdate, day, key;
            int hourval, min, sec;
            double energyprice;
            DateTime Todate;
            hourMinListDic = new Dictionary<string, List<ErcotEnergyPriceHelper>>();
            if ((DateTime.Now - endDate).TotalDays < 1)
            {
                Todate = endDate.AddDays(1);
            }
            else
            {
                Todate = endDate.AddDays(1);
                Todate = Todate.AddMinutes(-1);
            }

            LoadDB();
            if(SigmaErcotDBConnection.State==ConnectionState.Closed)
            SigmaErcotDBConnection.Open();
            cmdSelectDAEnergyPrices.Parameters["StartDate"].Value = startDate.Date;
            cmdSelectDAEnergyPrices.Parameters["EndDate"].Value = Todate;
            reader = cmdSelectDAEnergyPrices.ExecuteReader();
            while (reader.Read())
            {
                 mdate = reader.GetDateTime(0).ToString("yyyy-MM-dd");

                 day = Convert.ToInt32(reader.GetValue(1)).ToString();
                 hourval = Convert.ToInt32(reader.GetValue(2));
                 min = Convert.ToInt32(reader.GetValue(3));
                 sec = Convert.ToInt32(reader.GetValue(4));

                key = mdate + "?" + hourval;
                energyprice = reader.IsDBNull(5) ? 0.0 : Convert.ToDouble(reader.GetValue(5));
                ErcotEnergyPriceHelper helper = new ErcotEnergyPriceHelper();
                helper.hour = hourval;
                helper.Minute = min;
                helper.Seconds = sec;
                helper.EnergyPrice = energyprice;

                List<ErcotEnergyPriceHelper> lstErcotEnergyPricetHelper = new List<ErcotEnergyPriceHelper>();
                lstErcotEnergyPricetHelper.Add(helper);

                if (!hourMinListDic.ContainsKey(key))
                {
                    hourMinListDic.Add(key, lstErcotEnergyPricetHelper);
                }
                else
                    hourMinListDic[key].Add(helper);
            }
            reader.Close();


            DatehourList = hourMinListDic.Keys.ToList();
            DateHourWiseDic = new Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>>();
            DateList = new List<string>();
            foreach (string val in DatehourList)
            {
                string[] valarrya = val.Split('?');
                int hr = Convert.ToInt32(valarrya[1]);
                string datevar = valarrya[0];
                if (!DateList.Contains(datevar))
                    DateList.Add(datevar);

            }

            foreach (var Datevalue in DateList)
            {
                hourList = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };
                EPhourlist = new Dictionary<int, double>();

                EPMinWiselist = new Dictionary<int, double>();
                EPHourWiselist = new Dictionary<double, Dictionary<int, double>>();
                EFAllhourlist = new Dictionary<int, Dictionary<double, Dictionary<int, double>>>();
                foreach (var hour in hourList)
                {
                     double FinalEnergyPrice = 0;
                     key = Datevalue + "?" + hour;

                    if (hourMinListDic.ContainsKey(key))
                    {
                        var hourMinuteCollection = hourMinListDic[key];
                        Dictionary<int, double> AllMinDictonary = new Dictionary<int, double>();
                        double SumallEP = 0;
                        EPhourlist = new Dictionary<int, double>();
                        EPMinWiselist = new Dictionary<int, double>();
                        EPHourWiselist = new Dictionary<double, Dictionary<int, double>>();
                        for (int j = 0; j <= 59; j++)
                        {
                            AllMinDictonary[j] = hourMinuteCollection[0].EnergyPrice;
                            SumallEP = SumallEP + AllMinDictonary[j];
                            EPMinWiselist.Add(j, AllMinDictonary[j]);
                        }

                        //DateTime loopDate = DateTime.Parse(Datevalue);
                        //if ((DateTime.Now - loopDate).TotalDays < 1)
                        //{
                        //    if (DateTime.Now.Hour == hour)
                        //    {
                        //        FinalEnergyPrice = SumallEP / (DateTime.Now.Minute + 1);
                        //    }
                        //    else
                        //        FinalEnergyPrice = SumallEP / 60;
                        //}
                        //else
                        //    FinalEnergyPrice = SumallEP / 60;

                        FinalEnergyPrice = SumallEP / 60;

                        EPHourWiselist.Add(FinalEnergyPrice, EPMinWiselist);
                        EFAllhourlist.Add(hour, EPHourWiselist);
                    }

                }//hour loop

                   if (!DateHourWiseDic.ContainsKey(Datevalue))
                      DateHourWiseDic.Add(Datevalue, EFAllhourlist);
            }//date loop
            return DateHourWiseDic;
        }


        public Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> DARTEnergyPrices(DateTime startDate, DateTime endDate)
        {
            Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> DAEnergyPriceDic = new Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>>();
            Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> RTEnergyPriceDic = new Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>>();
            Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> DateHourWiseDic;
            Dictionary<int, Dictionary<double, Dictionary<int, double>>> DA_DaywiseEnergyPrice;
            Dictionary<int, Dictionary<double, Dictionary<int, double>>> RT_DaywiseEnergyPrice;//dd
            Dictionary<double, Dictionary<int, double>> DA_HourwiseEnergyPrice;
            Dictionary<double, Dictionary<int, double>> RT_HourwiseEnergyPrice;

            RTEnergyPriceDic = RTEnergyPrices(startDate, endDate);
            DAEnergyPriceDic = DAEnergyPrices(startDate, endDate);


            List<string> DateList = new List<string>();
            Dictionary<int, Dictionary<double, Dictionary<int, double>>> EFAllhourlist;
            List<int> hourList;
            Dictionary<int, double> EPhourlist, EPMinWiselist; ;
            Dictionary<double, Dictionary<int, double>> EPHourWiselist;
            List<string> DatehourList;
            string mdate,key;
            int hourval;
            double energyprice;
            hourMinListDic = new Dictionary<string, List<ErcotEnergyPriceHelper>>();

            double RT_EPValue, DA_EPValue, DART_EPValue;
            RT_EPValue = DA_EPValue = DART_EPValue = 0;
            List<string> RTkeylist = RTEnergyPriceDic.Keys.ToList();
           foreach (var item in RTkeylist)
            {
                RT_DaywiseEnergyPrice = RTEnergyPriceDic[item];
                for (int i = 0; i <= 23; i++)
                {
                    if (RT_DaywiseEnergyPrice.ContainsKey(i))
                    {
                        RT_HourwiseEnergyPrice = RT_DaywiseEnergyPrice[i];
                        foreach (var dicItem in RT_HourwiseEnergyPrice)
                            RT_EPValue = dicItem.Key;

                        if (DAEnergyPriceDic.ContainsKey(item))
                        {
                            DA_DaywiseEnergyPrice = DAEnergyPriceDic[item];
                            if (DA_DaywiseEnergyPrice.ContainsKey(i))
                            {
                                DA_HourwiseEnergyPrice = DA_DaywiseEnergyPrice[i];

                                foreach (var dicItem in DA_HourwiseEnergyPrice)
                                    DA_EPValue = dicItem.Key;

                                DART_EPValue = DA_EPValue - RT_EPValue;

                                mdate = item;// reader.GetDateTime(0).ToString("yyyy-MM-dd");
                                hourval = i;
                                key = mdate + "?" + hourval;
                                energyprice = DART_EPValue;
                                ErcotEnergyPriceHelper helper = new ErcotEnergyPriceHelper();
                                helper.hour = hourval;
                                helper.Minute = 0;
                                helper.Seconds = 0;
                                helper.EnergyPrice = energyprice;

                                List<ErcotEnergyPriceHelper> lstErcotEnergyPricetHelper = new List<ErcotEnergyPriceHelper>();
                                lstErcotEnergyPricetHelper.Add(helper);

                                if (!hourMinListDic.ContainsKey(key))
                                {
                                    hourMinListDic.Add(key, lstErcotEnergyPricetHelper);
                                }
                                else
                                    hourMinListDic[key].Add(helper);

                            }
                        }
                    }
                }
            }

        

          

            DatehourList = hourMinListDic.Keys.ToList();
            DateHourWiseDic = new Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>>();
            DateList = new List<string>();
            foreach (string val in DatehourList)
            {
                string[] valarrya = val.Split('?');
                int hr = Convert.ToInt32(valarrya[1]);
                string datevar = valarrya[0];


                if (!DateList.Contains(datevar))
                    DateList.Add(datevar);

            }

            foreach (var Datevalue in DateList)
            {
                hourList = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };
                EPhourlist = new Dictionary<int, double>();

                EPMinWiselist = new Dictionary<int, double>();
                EPHourWiselist = new Dictionary<double, Dictionary<int, double>>();
                EFAllhourlist = new Dictionary<int, Dictionary<double, Dictionary<int, double>>>();


                foreach (var hour in hourList)
                {
                    double FinalEnergyPrice = 0;
                    key = Datevalue + "?" + hour;

                    if (hourMinListDic.ContainsKey(key))
                    {
                        var hourMinuteCollection = hourMinListDic[key];

                        Dictionary<int, double> AllMinDictonary = new Dictionary<int, double>();
                        double SumallEP = 0;
                        EPhourlist = new Dictionary<int, double>();
                        EPMinWiselist = new Dictionary<int, double>();
                        EPHourWiselist = new Dictionary<double, Dictionary<int, double>>();
                        for (int j = 0; j <= 59; j++)
                        {
                            AllMinDictonary[j] = hourMinuteCollection[0].EnergyPrice;
                            SumallEP = SumallEP + AllMinDictonary[j];
                            EPMinWiselist.Add(j, AllMinDictonary[j]);
                        }

                        DateTime loopDate = DateTime.Parse(Datevalue);
                        if ((DateTime.Now - loopDate).TotalDays < 1)
                        {
                            if (DateTime.Now.Hour == hour)
                            {
                                FinalEnergyPrice = SumallEP / (DateTime.Now.Minute + 1);
                            }
                            else
                                FinalEnergyPrice = SumallEP / 60;
                        }
                        else
                            FinalEnergyPrice = SumallEP / 60;

                        EPHourWiselist.Add(FinalEnergyPrice, EPMinWiselist);
                        EFAllhourlist.Add(hour, EPHourWiselist);

                    }

                }//hour loop

                if (!DateHourWiseDic.ContainsKey(Datevalue))
                    DateHourWiseDic.Add(Datevalue, EFAllhourlist);
            }//date loop

            return DateHourWiseDic;
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

        public EnergyPriceServer()
        {
        }
    }
    
}
