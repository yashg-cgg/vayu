using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;
using Vayu.NodePriceLibrary;

namespace Vayu.CRRPeriodCongestionLibrary
{

    public abstract class Strategy
    {
        public abstract string TableName { get; }

        public abstract int MarketID { get; }

        public static Strategy GetStrategy(int mkID)
        {
            Strategy strategy = null;

            switch (mkID)
            {
                case 1:
                    strategy = new PJMStrategy();
                    break;

                case 9:
                    strategy = new ERCOTStrategy();
                    break;

                default:
                    break;
            }

            return strategy;
        }

        /// <summary>
        /// list[0] = dalist
        /// list[1] = rtlist
        /// </summary>
        /// <returns></returns>
        public List<List<Node>> GetNodeList()
        {
            Func<object, object, Node> funcNode = (obj1, obj2) =>
            {
                int key = -1;
                int pkey = -1;

                string str1 = (obj1 ?? "").ToString();
                string str2 = (obj2 ?? "").ToString();

                int.TryParse(str1, out key);
                int.TryParse(str2, out pkey);

                Node node = new Node();
                node.NodeId = key;
                node.PNodeId = pkey;
                node.Market = MarketID;
                return node;
            };

            List<List<Node>> list = new List<List<Node>>();
            List<Node> rtList = new List<Node>();
            List<Node> daList = new List<Node>();
            SqlCommand nodeCmd = Utility.GetCommand(DB.TradingData);
            if (MarketID == 12)
                nodeCmd.CommandText = "select top 17693 NodeKey,ExternalNodeID from Node where MarketKey = 12 ";
            else
                nodeCmd.CommandText = "select NodeKey,ExternalNodeID from Node where MarketKey = " + MarketID;

            if (nodeCmd.Connection.State != System.Data.ConnectionState.Open)
                nodeCmd.Connection.Open();

            SqlDataReader reader = nodeCmd.ExecuteReader();

            while (reader.Read())
            {
                daList.Add(funcNode(reader[0], reader[1]));
                rtList.Add(funcNode(reader[0], reader[1]));
            }

            reader.Close();
            if (nodeCmd.Connection.State != System.Data.ConnectionState.Closed)
                nodeCmd.Connection.Close();

            list.Add(daList);
            list.Add(rtList);
            return list;
        }

        /// <summary>
        /// Clears the previous records.
        /// </summary>
        /// <param name="period">The period.</param>
        public void ClearPreviousRecords(Period period)
        {
            try
            {
                string cmd = "delete " + TableName + " where Periodkey = " + period.PeriodKey;
                SqlCommand command = Utility.GetCommand(DB.TradingData);
                command.CommandText = cmd;

                if (command.Connection.State != System.Data.ConnectionState.Open)
                    command.Connection.Open();

                command.ExecuteScalar();

                if (command.Connection.State != System.Data.ConnectionState.Closed)
                    command.Connection.Close();
            }
            catch { }
        }

        /// <summary>
        /// Inserts the recs.
        /// </summary>
        /// <param name="congessionList">The congession list.</param>
        public void InsertRecs(List<PeriodCongession> congessionList)
        {
            InsertBulkRecs(congessionList);
        }

        /// <summary>
        /// Inserts the bulk recs.
        /// </summary>
        /// <param name="congessionList">The congession list.</param>
        public void InsertBulkRecs(List<PeriodCongession> congessionList)
        {
            Action<PeriodCongession, DataTable> ac1 = (cong, table) =>
            {
                DataRow row = table.NewRow();

                row["NodeKey"] = (cong.NodeKey);
                row["PeriodKey"] = (cong.PeriodKey);
                row["PeakDALMP"] = GetDBDouble(cong.PeakDALMP / cong.PeakDACount);
                row["OffPeakDALMP"] = GetDBDouble(cong.OffPeakDALMP / cong.OffPeakDACount);
                row["PeakRTLMP"] = GetDBDouble(cong.PeakRTLMP / cong.PeakRTCount);
                row["OffPeakRTLMP"] = GetDBDouble(cong.OffPeakRTLMP / cong.OffPeakRTCount);
                row["PeakDACongestion"] = GetDBDouble(cong.PeakDACongestion / cong.PeakDACount);
                row["OffPeakDACongestion"] = GetDBDouble(cong.OffPeakDACongestion / cong.OffPeakDACount);
                row["PeakRTCongestion"] = GetDBDouble(cong.PeakRTCongestion / cong.PeakRTCount);
                row["OffPeakRTCongestion"] = GetDBDouble(cong.OffPeakRTCongestion / cong.OffPeakRTCount);
                row["PeakDALoss"] = GetDBDouble(cong.PeakDALoss / cong.PeakDACount);
                row["OffPeakDALoss"] = GetDBDouble(cong.OffPeakDALoss / cong.OffPeakDACount);
                row["PeakRTLoss"] = GetDBDouble(cong.PeakRTLoss / cong.PeakRTCount);
                row["OffPeakRTLoss"] = GetDBDouble(cong.OffPeakRTLoss / cong.OffPeakRTCount);
                table.Rows.Add(row);
            };

            Action<DataTable> insertTab = (tab) =>
            {
                using (SqlConnection dbConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                {

                    using (SqlBulkCopy s = new SqlBulkCopy(dbConnection))
                    {
                        s.DestinationTableName = tab.TableName;

                        foreach (DataColumn column in tab.Columns)
                            s.ColumnMappings.Add(column.ColumnName, column.ColumnName);

                        s.WriteToServer(tab);
                    }
                    dbConnection.Close();
                }
            };

            int count = 0;
            DataTable congessionTable = GetTable();
            foreach (var item in congessionList)
            {
                try { ac1(item, congessionTable); }
                catch { }
                count++;

                if (count > 100)
                {
                    insertTab(congessionTable);
                    congessionTable.Clear();
                    congessionTable = GetTable();
                    count = 0;
                }
            }

            insertTab(congessionTable);
            congessionTable.Clear();
            count = 0;
        }

        /// <summary>
        /// Gets the table.
        /// </summary>
        /// <returns></returns>
        private DataTable GetTable()
        {
            DataTable congessionTable = new DataTable(TableName);
            congessionTable.Columns.Add(new DataColumn("NodeKey", typeof(int)));
            congessionTable.Columns.Add(new DataColumn("PeriodKey", typeof(int)));
            congessionTable.Columns.Add(new DataColumn("PeakDALMP", typeof(double)));
            congessionTable.Columns.Add(new DataColumn("OffPeakDALMP", typeof(double)));
            congessionTable.Columns.Add(new DataColumn("PeakRTLMP", typeof(double)));
            congessionTable.Columns.Add(new DataColumn("OffPeakRTLMP", typeof(double)));
            congessionTable.Columns.Add(new DataColumn("PeakDACongestion", typeof(double)));
            congessionTable.Columns.Add(new DataColumn("OffPeakDACongestion", typeof(double)));
            congessionTable.Columns.Add(new DataColumn("PeakRTCongestion", typeof(double)));
            congessionTable.Columns.Add(new DataColumn("OffPeakRTCongestion", typeof(double)));
            congessionTable.Columns.Add(new DataColumn("PeakDALoss", typeof(double)));
            congessionTable.Columns.Add(new DataColumn("OffPeakDALoss", typeof(double)));
            congessionTable.Columns.Add(new DataColumn("PeakRTLoss", typeof(double)));
            congessionTable.Columns.Add(new DataColumn("OffPeakRTLoss", typeof(double)));
            return congessionTable;
        }

        /// <summary>
        /// Gets the database double.
        /// </summary>
        /// <param name="dValue">The d value.</param>
        /// <returns></returns>
        private object GetDBDouble(double dValue)
        {
            if (double.IsInfinity(dValue) || double.IsNaN(dValue))
                return DBNull.Value;

            return Math.Round(dValue, 4);
        }

        /// <summary>
        /// Gets the database double.
        /// </summary>
        /// <param name="dValue">The d value.</param>
        /// <returns></returns>
        private object GetDBDouble(double? dValue)
        {
            if (!dValue.HasValue)
                return DBNull.Value;

            return GetDBDouble(dValue.Value);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FTRPeriodCongestionLibrary.Strategy" />
    public class MISOStrategy : Strategy
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public override string TableName
        {
            get { return "MISO.PeriodLMP"; }
        }

        /// <summary>
        /// Gets the market identifier.
        /// </summary>
        /// <value>
        /// The market identifier.
        /// </value>
        public override int MarketID
        {
            get { return 2; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FTRPeriodCongestionLibrary.Strategy" />
    public class PJMStrategy : Strategy
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public override string TableName
        {
            get { return "dbo.PeriodLMP"; }
        }

        /// <summary>
        /// Gets the market identifier.
        /// </summary>
        /// <value>
        /// The market identifier.
        /// </value>
        public override int MarketID
        {
            get { return 1; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FTRPeriodCongestionLibrary.Strategy" />
    public class CAISOStrategy : Strategy
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public override string TableName
        {
            get { return "CAISO.PeriodLMP"; }
        }

        /// <summary>
        /// Gets the market identifier.
        /// </summary>
        /// <value>
        /// The market identifier.
        /// </value>
        public override int MarketID
        {
            get { return 7; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FTRPeriodCongestionLibrary.Strategy" />
    public class ERCOTStrategy : Strategy
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public override string TableName
        {
            get { return "ERCOT.PeriodLMP"; }
        }

        /// <summary>
        /// Gets the market identifier.
        /// </summary>
        /// <value>
        /// The market identifier.
        /// </value>
        public override int MarketID
        {
            get { return 9; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FTRPeriodCongestionLibrary.Strategy" />
    public class NYSIOStrategy : Strategy
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public override string TableName
        {
            get { return "NYISO.PeriodLMP"; }
        }

        /// <summary>
        /// Gets the market identifier.
        /// </summary>
        /// <value>
        /// The market identifier.
        /// </value>
        public override int MarketID
        {
            get { return 3; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FTRPeriodCongestionLibrary.Strategy" />
    public class SPPStrategy : Strategy
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public override string TableName
        {
            get { return "SPP.PeriodLMP"; }
        }

        /// <summary>
        /// Gets the market identifier.
        /// </summary>
        /// <value>
        /// The market identifier.
        /// </value>
        public override int MarketID
        {
            get { return 12; }
        }
    }
}
