using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vayu.CommonAccessLibrary;

namespace Vayu.BlockAlgorithmNamespace
{
    class CalculateMinMaxRTAll
    {
        private SqlConnection SigmaDBConnection;
        private SqlCommand mDeleteMinMaxRTAllCommand;
        private DataTable mDTMinMaxAll;
        public CalculateMinMaxRTAll()
        {
            IninDB();
            GetData();
        }

        private void GetData()
        {
            mDTMinMaxAll.Clear();
            if (SigmaDBConnection.State == ConnectionState.Open)
            {
                SigmaDBConnection.Close();
            }
            SigmaDBConnection.Open();
            SqlDataAdapter da = new SqlDataAdapter("select a.SourceName,a.SinkName,MinValue as RTMinAll,MaxValue as RTMaxAll from BlockAlgoMinMaxFall a inner join BlockAlgoMinMaxSpring b on a.SourceName=b.SourceName and a.SinkName=b.SinkName   inner join BlockAlgoMinMaxSummer c on c.SourceName=a.SourceName and c.SinkName=a.SinkName   inner join BlockAlgoMinMaxWinter d on d.SourceName=a.SourceName and d.SinkName=a.SinkName   CROSS APPLY (SELECT MIN(d) MinValue FROM (VALUES (a.RTMinFall), (a.RTMaxFall), (b.RTMinSpring),(b.RTMaxSpring),(c.RTMinSummer),(c.RTMaxSummer),   (d.RTMin),(d.RTMax)) AS MI(d)) MI   CROSS APPLY (SELECT MAX(e) MaxValue FROM (VALUES (a.RTMinFall), (a.RTMaxFall), (b.RTMinSpring),(b.RTMaxSpring),(c.RTMinSummer),(c.RTMaxSummer),   (d.RTMin),(d.RTMax)) AS MI(e)) MX", SigmaDBConnection);
            da.Fill(mDTMinMaxAll);
            Insert(mDTMinMaxAll);
        }
        private void Insert(DataTable mDTMinMaxAll)
        {
            if (SigmaDBConnection.State == ConnectionState.Open)
            {
                SigmaDBConnection.Close();
            }
            SigmaDBConnection.Open();
            mDeleteMinMaxRTAllCommand.ExecuteNonQuery();
            SqlTransaction transaction = SigmaDBConnection.BeginTransaction();
            using (SqlBulkCopy bkLmpH = new SqlBulkCopy(SigmaDBConnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    bkLmpH.DestinationTableName = "BlockAlgoMinMaxAllTest ";
                    bkLmpH.BatchSize = 15000;
                    bkLmpH.BulkCopyTimeout = 30000;
                    bkLmpH.ColumnMappings.Add("SourceName", "SourceName");
                    bkLmpH.ColumnMappings.Add("SinkName", "SinkName");
                    bkLmpH.ColumnMappings.Add("RTMinAll", "RTMinAll");
                    bkLmpH.ColumnMappings.Add("RTMaxAll", "RTMaxAll");
                    bkLmpH.WriteToServer(mDTMinMaxAll);
                    SqlCommand cmdUpdatenodelmph = new SqlCommand("[dbo].[UpMergeRTMaxAll]", SigmaDBConnection, transaction);
                    cmdUpdatenodelmph.CommandType = CommandType.StoredProcedure;
                    cmdUpdatenodelmph.CommandTimeout = 30000;
                    cmdUpdatenodelmph.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
        }
        private void IninDB()
        {
            //SigmaDBConnection = new SqlConnection(Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
            SigmaDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mDeleteMinMaxRTAllCommand = new SqlCommand();
            mDeleteMinMaxRTAllCommand.CommandText = "truncate table BlockAlgoMinMaxAllTest";
            mDeleteMinMaxRTAllCommand.Connection = SigmaDBConnection;

            mDTMinMaxAll = new DataTable();

            mDTMinMaxAll.Columns.Add("SourceName", typeof(string));
            mDTMinMaxAll.Columns.Add("SinkName", typeof(string));
            mDTMinMaxAll.Columns.Add("RTMinAll", typeof(double));
            mDTMinMaxAll.Columns.Add("RTMaxAll", typeof(double));

        }

    }
}
