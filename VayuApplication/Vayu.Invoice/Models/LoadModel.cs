using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.Invoice.Models
{
    public class LoadModel
    {
        private SqlConnection VayuDbConn;

        #region SQL Commands


        #endregion

        #region Public Methods

        public void loadDBCommands()
        {
            //VayuDbConn = new SqlConnection("Data Source = ; Initial Catalog = VayuAccounting; Persist Security Info = True; User Id = VayuAcc; password = Vayu@2023!; Connect Timeout = 100000; MultipleActiveResultSets = True");
            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
        }

        public void GetInvoiceData(Action<List<InvoiceSettlement>, Exception> callback, string desc)
        {
            List<InvoiceSettlement> ccdata = new List<InvoiceSettlement>();
            SqlCommand cmd = new SqlCommand();
            try
            {
                //if (marketKey == 9)
                {
                    cmd.CommandText = "select InvoiceDate, InvoiceTotal, StatementType, StatementId, OperatingDate, StatementAmount,InvoiceId from SettlementInvoice where InvoiceId=@InvoiceId";
                }
                cmd.Parameters.AddWithValue("@InvoiceId", "InvoiceId");
                cmd.Parameters["@InvoiceId"].Value = desc;
                cmd.Connection = VayuDbConn;

                SqlDataReader reader;
                if (VayuDbConn.State != ConnectionState.Open)
                    VayuDbConn.Open();

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    InvoiceSettlement data = new InvoiceSettlement();
                    data.InvoiceDate = Convert.ToDateTime(reader[0]);
                    data.InvoiceTotal = -1 * Math.Round(double.Parse(reader[1].ToString()), 2);
                    data.StatementType = reader[2].ToString();
                    data.StatementId = reader[3].ToString();
                    data.OperatingDate = Convert.ToDateTime(reader[4]);
                    data.StatementAmount = -1 * double.Parse(reader[5].ToString());
                    data.InvoiceId = reader[6].ToString();
                    ccdata.Add(data);
                }
                reader.Close();
                if (VayuDbConn.State != ConnectionState.Closed)
                    VayuDbConn.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                callback(ccdata, null);
            }
        }
        #endregion
    }
}
