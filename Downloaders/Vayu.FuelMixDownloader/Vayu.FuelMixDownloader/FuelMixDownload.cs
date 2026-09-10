using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Vayu.CommonAccessLibrary;
using Newtonsoft.Json;
using System.Dynamic;
using System.Timers;

namespace Vayu.FuelMixDownloader
{
    class FuelMixDownload
    {
        private readonly X509Certificate2 mCert = new X509Certificate2();
        CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        private SqlCommand mDeleteFuelMixTestCommand;
        private SqlConnection VayuDbConn;
        DataTable mERCOTFuelMixDT;
        DataRow mERCOTFuelMixDTRow;
        readonly System.Timers.Timer sTimer = new System.Timers.Timer();

        public FuelMixDownload()
        {
            InitDB();
            InitCert();
            sTimer = new System.Timers.Timer();
            OnTimedEvent(null, null);
            sTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            sTimer.Interval = 60000;//60sec - 1min
            sTimer.Start();
            while (true)
            {
                Thread.Sleep(1000 * 10); // 10 sec
            }
        }

        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            sTimer.Enabled = false;
            try
            {
                Console.WriteLine("Executing for - " + DateTime.Now);
                GetResponse();
            }
            catch (Exception ex)
            {
            }
            sTimer.Enabled = true;
        }
        public void InitDB()
        {
            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
            userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientCert");
            mDeleteFuelMixTestCommand = new SqlCommand();
            mDeleteFuelMixTestCommand.CommandText = "Truncate table Fuelmix_Test";
            mDeleteFuelMixTestCommand.Connection = VayuDbConn;
        }
        public void InitCert()
        {
            mCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, X509KeyStorageFlags.MachineKeySet);
        }
        public void GetResponse()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://www.ercot.com/api/1/services/read/dashboards/fuel-mix.json");
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ServicePoint.ConnectionLimit = 1;
                request.CookieContainer = new CookieContainer();
                request.Method = "GET";
                request.ClientCertificates.Add(mCert);
                request.Timeout = 100000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var res = response;
                StreamReader sr = default(StreamReader);
                sr = new StreamReader(response.GetResponseStream());
                string jsonData = sr.ReadToEnd();
                string modifiedJsonData = jsonData.Replace("Coal and Lignite", "CoalAndLignite");
                modifiedJsonData = modifiedJsonData.Replace("Natural Gas", "NaturalGas");
                modifiedJsonData = modifiedJsonData.Replace("Power Storage", "PowerStorage");

                dynamic jsond = JsonConvert.DeserializeObject(jsonData, typeof(object));
                dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsond.ToString());

                IDictionary<string, Object> valueDictoner = (IDictionary<string, object>)dyn;
                DateTime recentdatetime = DateTime.MinValue;

                mERCOTFuelMixDT = new DataTable();
                mERCOTFuelMixDT.Columns.Add("DateTime", typeof(DateTime));
                mERCOTFuelMixDT.Columns.Add("Solar", typeof(decimal));
                mERCOTFuelMixDT.Columns.Add("Wind", typeof(decimal));
                mERCOTFuelMixDT.Columns.Add("Hydro", typeof(decimal));
                mERCOTFuelMixDT.Columns.Add("PowerStorage", typeof(decimal));
                mERCOTFuelMixDT.Columns.Add("Other", typeof(decimal));
                mERCOTFuelMixDT.Columns.Add("NaturalGas", typeof(decimal));
                mERCOTFuelMixDT.Columns.Add("CoalandLignite", typeof(decimal));
                mERCOTFuelMixDT.Columns.Add("Nuclear", typeof(decimal));
                foreach (var item in valueDictoner)
                {
                    string s = item.Key;
                    if (s.ToLower() == "lastupdated")
                    {
                        recentdatetime = Convert.ToDateTime(item.Value);
                    }
                    if (s.ToLower() == "data")
                    {
                        IDictionary<string, Object> vd = (IDictionary<string, object>)item.Value;
                        foreach (var item1 in vd)
                        {
                            var v = item1.Key;
                            IDictionary<string, Object> vd1 = (IDictionary<string, object>)item1.Value;
                            foreach (var item2 in vd1)
                            {
                                DateTime timestamp = Convert.ToDateTime(item2.Key);
                                IDictionary<string, Object> vd2 = (IDictionary<string, object>)item2.Value;
                                dynamic coal = vd2["Coal and Lignite"];
                                double coalmw1 = coal.gen;
                                //double coalhsl = coal.hsl;
                                //double coalseasonalCapacity = coal.seasonalCapacity;

                                dynamic Hydro = vd2["Hydro"];
                                double Hydrogen = Hydro.gen;
                                //double Hydrohsl = Hydro.hsl;
                                //double HydroseasonalCapacity = Hydro.seasonalCapacity;

                                dynamic Nuclear = vd2["Nuclear"];
                                double Nucleargen = Nuclear.gen;
                                //double Nuclearhsl = Nuclear.hsl;
                                //double NuclearseasonalCapacity = Nuclear.seasonalCapacity;

                                dynamic Solar = vd2["Solar"];
                                double Solargen = Solar.gen;
                                //double Solarhsl = Solar.hsl;
                                //double SolarseasonalCapacity = Solar.seasonalCapacity;

                                dynamic Wind = vd2["Wind"];
                                double Windgen = Wind.gen;
                                //double Windhsl = Wind.hsl;
                                //double WindseasonalCapacity = Wind.seasonalCapacity;

                                dynamic NaturalGas = vd2["Natural Gas"];
                                double NaturalGasgen = NaturalGas.gen;
                                //double NaturalGashsl = NaturalGas.hsl;
                                //double NaturalGasseasonalCapacity = NaturalGas.seasonalCapacity;

                                dynamic Other = vd2["Other"];
                                double Othergen = Other.gen;
                               // double Otherhsl = Other.hsl;
                                //double OtherseasonalCapacity = Other.seasonalCapacity;

                                dynamic PowerStorage = vd2["Power Storage"];
                                double PowerStoragegen = PowerStorage.gen;
                               // double PowerStoragehsl = PowerStorage.hsl;
                               // double PowerStorageseasonalCapacity = PowerStorage.seasonalCapacity;

                                mERCOTFuelMixDTRow = mERCOTFuelMixDT.NewRow();
                                mERCOTFuelMixDTRow["DateTime"] = timestamp;
                                mERCOTFuelMixDTRow["Solar"] = Solargen;
                                mERCOTFuelMixDTRow["Wind"] = Windgen;
                                mERCOTFuelMixDTRow["Hydro"] = Hydrogen;
                                mERCOTFuelMixDTRow["PowerStorage"] = PowerStoragegen;
                                mERCOTFuelMixDTRow["Other"] = Othergen;
                                mERCOTFuelMixDTRow["NaturalGas"] = NaturalGasgen;
                                mERCOTFuelMixDTRow["CoalandLignite"] = coalmw1;
                                mERCOTFuelMixDTRow["Nuclear"] = Nucleargen;
                                mERCOTFuelMixDT.Rows.Add(mERCOTFuelMixDTRow);
                            }
                        }
                    }
                }

                if (VayuDbConn.State == ConnectionState.Open)
                {
                    VayuDbConn.Close();
                }
                VayuDbConn.Open();
                mDeleteFuelMixTestCommand.ExecuteNonQuery();
                SqlTransaction transaction = VayuDbConn.BeginTransaction();
                using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
                {
                    try
                    {
                        bkLmpH.DestinationTableName = "FuelMix_Test";
                        bkLmpH.ColumnMappings.Add("DateTime", "DateTime");
                        bkLmpH.ColumnMappings.Add("Solar", "Solar");
                        bkLmpH.ColumnMappings.Add("Wind", "Wind");
                        bkLmpH.ColumnMappings.Add("Hydro", "Hydro");
                        bkLmpH.ColumnMappings.Add("PowerStorage", "PowerStorage");
                        bkLmpH.ColumnMappings.Add("Other", "Other");
                        bkLmpH.ColumnMappings.Add("NaturalGas", "NaturalGas");
                        bkLmpH.ColumnMappings.Add("CoalandLignite", "CoalandLignite");
                        bkLmpH.ColumnMappings.Add("Nuclear", "Nuclear");
                        bkLmpH.WriteToServer(mERCOTFuelMixDT);

                        SqlCommand updateRTFuelMix = new SqlCommand("[UpMergeRTFuelMix]", VayuDbConn, transaction);
                        updateRTFuelMix.CommandType = CommandType.StoredProcedure;
                        updateRTFuelMix.CommandTimeout = 300000;
                        updateRTFuelMix.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        transaction.Rollback();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }    
}
