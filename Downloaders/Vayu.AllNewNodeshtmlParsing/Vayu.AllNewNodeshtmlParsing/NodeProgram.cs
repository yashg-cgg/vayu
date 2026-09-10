using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vayu.CommonAccessLibrary;

namespace Vayu.AllNewNodeshtmlParsing
{
    class NodeProgram
    {
        SqlConnection VayuDbConnection;
        DataTable dtNode = new DataTable(); 
        public NodeProgram()
        {
            ParseHTML();

        }
        
        public void ParseHTML()
        {
            VayuDbConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            StreamReader sr = new StreamReader(@"D:\ISOFiles\AllNewNodeshtmlParsing\ErcotPaths.html");

            dtNode = new DataTable();
            dtNode.Columns.Add("NodeName");
            dtNode.Columns.Add("MarketDateTime");
            while(sr.Peek() >= 0)
            {
                string line = sr.ReadLine();
                if(line.Contains("AEEC") || line.Contains("DC_N") || line.Contains("HB_HOUSTON"))
                {
                    string data = line;
                    string[] stringSeparators = new string[] { "<option value" };
                    string[] result = line.Split(stringSeparators, StringSplitOptions.None);
                     for(int i = 1; i < result.Count(); i++)
                    {
                        DataRow dr = dtNode.NewRow();
                        if (result[i].Contains(">"))
                        {
                            data = result[i].Substring(result[i].IndexOf("=") + 1, result[i].IndexOf(">") - 1);
                            dr["NodeName"] = data.Replace("\"", "");
                            dr["MarketDateTime"] = DateTime.Today;
                            dtNode.Rows.Add(dr);
                            int b = dtNode.Rows.Count;

                        }
                    }
                }
            }

            if (dtNode.Rows.Count > 0)
            {
                if (VayuDbConnection.State == ConnectionState.Closed)
                {
                    VayuDbConnection.Open();
                }
                SqlTransaction sqlTransaction = VayuDbConnection.BeginTransaction();
                using(SqlBulkCopy bkBids= new SqlBulkCopy(VayuDbConnection, SqlBulkCopyOptions.TableLock, sqlTransaction))
                {
                    try
                    {
                        int b = dtNode.Rows.Count;
                        bkBids.DestinationTableName = "Vayu..ErcotValidPTPNodes";
                        bkBids.BulkCopyTimeout = 30000;
                        bkBids.ColumnMappings.Add("NodeName", "NodeName");
                        bkBids.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                        bkBids.WriteToServer(dtNode);
                        sqlTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception occurs", ex);
                        sqlTransaction.Rollback();
                      
                    }
                }
            }
        }

    }
}
