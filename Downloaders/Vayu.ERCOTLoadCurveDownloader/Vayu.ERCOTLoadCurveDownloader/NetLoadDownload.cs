using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Vayu.ERCOTLoadCurveDownloader
{
    class NetLoadDownload
    {
        private SqlConnection VayuDbConn;
        SqlCommand mInsertNetLoad;
        SqlCommand mUpdateNetLoad;
        SqlCommand mSelectNetLoad;
        DataTable dtNetLoad = new DataTable();
        DataRow drNetLoad;
        System.Timers.Timer mTimer = new System.Timers.Timer();
        public NetLoadDownload()
        {
            InitDB();
            mTimer = new System.Timers.Timer();
            mTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            mTimer.Interval = 5 * 60 * 1000;
            OnTimerEvent(null, null);
            mTimer.Start();
            Console.WriteLine("Wait for 5 min ...");
            while (true)
            {
                Thread.Sleep(10 * 1000);
            }
        }
        public void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            Console.WriteLine("Accessing DownloadNetLoad Function");            
            DownloadNetLoad();
            mTimer.Enabled = true;
        }
        public void InitDB()
        {
            VayuDbConn = new SqlConnection(Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
            mUpdateNetLoad = new SqlCommand();
            mUpdateNetLoad.CommandText = "Update Vayu..NetLoad_Test set MW=@MW where MarketDateTime=@MarketDateTime ";           
            mUpdateNetLoad.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mUpdateNetLoad.Parameters.AddWithValue("@MW", "MW");
            mUpdateNetLoad.Connection = VayuDbConn;
            //
            mInsertNetLoad = new SqlCommand();
            mInsertNetLoad.CommandText = "insert into Vayu..NetLoad_Test(MarketDateTime, MW) Values (@MarketDateTime, @MW)";           
            mInsertNetLoad.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mInsertNetLoad.Parameters.AddWithValue("@MW", "MW");
            mInsertNetLoad.Connection = VayuDbConn;
            //
            mSelectNetLoad = new SqlCommand();
            mSelectNetLoad.CommandText = @"SELECT 
    marketdatetime,net_load
FROM (
    SELECT 
        lr.marketdatetime,
        lr.mw - COALESCE(wp.Realtimevalue, 0) - COALESCE(sp.Realtimevalue, 0) AS net_load
    FROM 
        LoadRT lr
    LEFT JOIN 
        WindPowerGenerationValue wp ON 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lr.marketdatetime, 120), 8) = '00:00:00' THEN CAST(DATEADD(DAY, -1, CAST(lr.marketdatetime AS DATE)) AS DATE)  ELSE CAST(lr.marketdatetime AS DATE) 
            END = CAST(wp.Marketdatetime AS DATE) 
        AND 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lr.marketdatetime, 120), 8) = '00:00:00' THEN '24'  ELSE SUBSTRING(CONVERT(VARCHAR, lr.marketdatetime, 120), 12, 2) 
            END = wp.Hour
    LEFT JOIN 
        SolarPowerGenerationValue sp ON 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lr.marketdatetime, 120), 8) = '00:00:00' THEN CAST(DATEADD(DAY, -1, CAST(lr.marketdatetime AS DATE)) AS DATE) ELSE CAST(lr.marketdatetime AS DATE) 
            END = CAST(sp.Marketdatetime AS DATE) 
        AND 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lr.marketdatetime, 120), 8) = '00:00:00' THEN '24'  ELSE SUBSTRING(CONVERT(VARCHAR, lr.marketdatetime, 120), 12, 2) 
            END = sp.Hour           
UNION ALL
    SELECT 
        lf.marketdatetime,
        lf.MW - COALESCE(wp.forecastvalue, 0) - COALESCE(sp.forecastvalue, 0) AS net_load
    FROM 
        loadforecasts lf
    LEFT JOIN 
        WindPowerGenerationValue wp ON 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lf.marketdatetime, 120), 8) = '00:00:00' THEN CAST(DATEADD(DAY, -1, CAST(lf.marketdatetime AS DATE)) AS DATE) ELSE CAST(lf.marketdatetime AS DATE) 
            END = CAST(CONVERT(VARCHAR, wp.Marketdatetime, 120) AS DATE) 
        AND 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lf.marketdatetime, 120), 8) = '00:00:00' THEN '24' ELSE SUBSTRING(CONVERT(VARCHAR, lf.marketdatetime, 120), 12, 2) 
            END = wp.Hour
    LEFT JOIN 
        SolarPowerGenerationValue sp ON 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lf.marketdatetime, 120), 8) = '00:00:00' THEN CAST(DATEADD(DAY, -1, CAST(lf.marketdatetime AS DATE)) AS DATE)  ELSE CAST(lf.marketdatetime AS DATE) 
            END = CAST(CONVERT(VARCHAR, sp.Marketdatetime, 120) AS DATE) 
        AND 
            CASE 
                WHEN RIGHT(CONVERT(VARCHAR, lf.marketdatetime, 120), 8) = '00:00:00' THEN '24' ELSE SUBSTRING(CONVERT(VARCHAR, lf.marketdatetime, 120), 12, 2) 
            END = sp.Hour
    WHERE 
        lf.loadforecasttypekey = @loadforecasttypekey        
) AS CombinedData;";            
            mSelectNetLoad.Connection = VayuDbConn;
            //
            dtNetLoad.Columns.Add("Date", typeof(DateTime));           
            dtNetLoad.Columns.Add("MW", typeof(decimal));           
        }
        public void DownloadNetLoad()
        {
            List<LoadDataItem> itemList = new List<LoadDataItem>();
            using (SqlConnection con = new SqlConnection(VayuDbConn.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = mSelectNetLoad.CommandText;
                    cmd.CommandTimeout = 3 * 10 * 1000;
                    con.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new LoadDataItem
                        {
                            MarketDateTime = reader.IsDBNull(0) ? new DateTime() : reader.GetDateTime(0),
                            MW = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                        });
                    }
                    if (!reader.IsClosed)
                        reader.Close();
                }
            }
            int count = itemList.Count;
            foreach (var item in itemList)
            {                 
                {
                    drNetLoad = dtNetLoad.NewRow();
                    drNetLoad["MarketDateTime"] = item.MarketDateTime;
                    drNetLoad["MW"] = item.MW;
                    dtNetLoad.Rows.Add(drNetLoad);
                    Console.WriteLine(count);
                }
            }
        }
    }
    public class LoadDataItem
    {
        public DateTime MarketDateTime { get; set; }
        public double MW { get; set; }
    }


}
