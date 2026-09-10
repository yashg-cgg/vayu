using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.CRRPeriods.Model
{
    public class DataService : IDataService
    {
        public void GetData(Action<DataItem, Exception> callback)
        {
            var item = new DataItem("Welcome to MVVM Light");
            callback(item, null);
        }

        /// <summary>
        /// Gets the period data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void GetPeriodData(Action<System.Collections.Generic.List<FtrPeriod>, Exception> callback, string market)
        {
            List<FtrPeriod> periodList = new List<FtrPeriod>();
            try
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        if (market.ToUpper() == "ERCOT")
                        {
                            cmd.CommandText = "select PeriodKey,MarketKey,PeriodName,PeriodType,StartDate,EndDate,PeakHrs,OffPeakHrs,[24Hrs],[PeakWE] from period order by StartDate desc,EndDate";
                        }
                        System.Data.IDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            try
                            {
                                if (market == "ERCOT")
                                {
                                    FtrPeriod mFtrPeriod = new FtrPeriod();
                                    mFtrPeriod.PeriodKey = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                                    mFtrPeriod.Market = GetMarket(reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1)));
                                    mFtrPeriod.PeriodName = reader.IsDBNull(2) ? "" : reader.GetValue(2).ToString();
                                    mFtrPeriod.PeriodType = reader.IsDBNull(3) ? "" : reader.GetValue(3).ToString();
                                    if (mFtrPeriod.PeriodType == "Long Term" || mFtrPeriod.PeriodType == "Seasonal" || mFtrPeriod.PeriodType == "Annual")
                                    {
                                        mFtrPeriod.Hours24 = 24;

                                    }
                                    else
                                    {
                                        mFtrPeriod.Hours24 = reader.IsDBNull(8) ? 0 : Convert.ToInt32(reader.GetValue(8));
                                    }
                                    mFtrPeriod.StartDate = reader.IsDBNull(4) ? new DateTime() : Convert.ToDateTime(reader.GetValue(4));
                                    mFtrPeriod.EndDate = reader.IsDBNull(5) ? new DateTime() : Convert.ToDateTime(reader.GetValue(5));
                                    mFtrPeriod.PeakHours = reader.IsDBNull(6) ? 0 : Convert.ToInt32(reader.GetValue(6));
                                    mFtrPeriod.OffPeakHrs = reader.IsDBNull(7) ? 0 : Convert.ToInt32(reader.GetValue(7));
                                    //mFtrPeriod.Hours24 = reader.IsDBNull(8) ? 0 : Convert.ToInt32(reader.GetValue(8));
                                    mFtrPeriod.PeakWE = reader.IsDBNull(9) ? 0 : Convert.ToInt32(reader.GetValue(9));
                                    periodList.Add(mFtrPeriod);
                                    //periodList.Add(new FtrPeriod
                                    //{

                                    //});

                                }
                            }
                            catch
                            {

                            }
                        }
                        reader.Close();
                        cmd.Connection.Close();
                    }
                }
                callback(periodList, null);
            }
            catch (Exception ex)
            {
                callback(null, ex);
            }

        }

        /// <summary>
        /// Gets the market.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        private string GetMarket(int p)
        {
            switch (p)
            {
                case 9: return "ERCOT";
                default: return "ERCOT";
            }
        }
    }
}
