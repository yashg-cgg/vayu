using System;
using System.Collections;
using System.Data.SqlClient;
using System.Diagnostics;
using Vayu.CommonAccessLibrary;

namespace Vayu.CRRPeriodCongestionLibrary
{
    public class Utility
    {
        public static SqlCommand GetCommand(DB dbToUse)
        {
            SqlCommand command = null;
            switch (dbToUse)
            {
                case DB.TradingData:
                    SqlConnection trading = new VayuDBConnection().GetInstance().GetSqlConnection();
                    command = trading.CreateCommand();
                    break;
                case DB.RiskData:
                    SqlConnection risk = new VayuDBConnection().GetInstance().GetSqlConnection();
                    command = risk.CreateCommand();
                    break;
                default:
                    break;
            }

            return command;
        }

        public static double GetDouble(object objData)
        {
            string dStr = (objData ?? "").ToString();
            double dValue = 0;
            double.TryParse(dStr, out dValue);
            return dValue;
        }

        public static double? GetNullDouble(object objData)
        {
            string dStr = (objData ?? "").ToString();
            double dValue = 0;
            if (!double.TryParse(dStr, out dValue))
                return null;

            return dValue;
        }

        public static int GetInt(object objData)
        {
            string dStr = (objData ?? "").ToString();
            int dValue = 0;
            int.TryParse(dStr, out dValue);
            return dValue;
        }

        public static DateTime GetDateTime(object objData)
        {
            string dStr = (objData ?? "").ToString();
            DateTime dValue;
            DateTime.TryParse(dStr, out dValue);
            return dValue;
        }

        private static Hashtable timerHash;

        public static void StartTimer(string key)
        {
            if (timerHash == null)
                timerHash = new Hashtable();

            Stopwatch watch = new Stopwatch();
            timerHash[key] = watch;
            watch.Start();
        }

        public static void StopTimer(string key)
        {
            if (timerHash == null || !timerHash.ContainsKey(key))
                return;

            Stopwatch watch = timerHash[key] as Stopwatch;
            watch.Stop();
            string elapsed = key + ": " + watch.ElapsedTicks.ToString();
            Debug.WriteLine(elapsed);
        }
    }

    public enum DB
    {
        TradingData,

        RiskData
    }
}
