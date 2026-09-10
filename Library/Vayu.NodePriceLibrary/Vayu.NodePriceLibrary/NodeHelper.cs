using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;

namespace Vayu.NodePriceLibrary
{
    public class NodeHelper
    {
        private SqlCommand cmdSelectSourceSinkNodeCommand;
        private SqlConnection VayuConnection;
        private static NodeHelper nodeHelper;

        public static NodeHelper Instance
        {
            get
            {
                if (nodeHelper == null)
                {
                    nodeHelper = new NodeHelper();
                }

                return nodeHelper;
            }
        }

        public NodeHelper()
        { }

        public void LoadDB()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            cmdSelectSourceSinkNodeCommand = VayuConnection.CreateCommand();
            cmdSelectSourceSinkNodeCommand.CommandText = "SELECT src.SourceNodeName, sink.SinkNodeName FROM EESPathList src "
                                                           + "INNER JOIN Node n ON n.NodeKey = src.SourceNodeKey "
                                                           + "INNER JOIN EESPathList sink ON sink.SinkNodeKey = src.SinkNodeKey "
                                                           + "AND sink.EESPathListKey = src.EESPathListKey "
                                                           + "INNER JOIN Node n2 ON n2.NodeKey = sink.SinkNodeKey "
                                                           + "WHERE n.MarketKey = @MarketKey AND n2.MarketKey = @MarketKey "
                                                           + "AND src.MarketKey = @MarketKey AND sink.MarketKey = @MarketKey "
                                                           + "ORDER BY n.NodeName";
            cmdSelectSourceSinkNodeCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
        }

        public List<int> GetErcotUpToNodeKeys(string Market, bool isErcotVirtual = true)
        {
            LoadDB();
            Hashtable hash = new Hashtable();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlCommand cmdTemp = VayuConnection.CreateCommand();
            if (Market == "ERCOT")
            {
                if (isErcotVirtual)
                    cmdTemp.CommandText = "select distinct SourceNodeKey from Vayu..EESPathList where SourceNodeKey is not null";
                else
                    cmdTemp.CommandText = "Select * from Vayu..NodeDALMPH where MarketDateTime='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
            }
            else
            {
                cmdTemp.CommandText = "select distinct SourceNodeKey  from Vayu..EESPathList  where SourceNodeKey is not null";
            }
            SqlDataReader dr = cmdTemp.ExecuteReader();

            Action<SqlDataReader> rdr = (sqlReader) =>
            {
                while (dr.Read())
                {
                    int key = 0;
                    int.TryParse((dr[0] ?? "").ToString(), out key);
                    hash[key] = key;
                }
                if (dr != null)
                {
                    dr.Close();
                }
            };

            rdr(dr);

            //cmdTemp.CommandText = "SELECT DISTINCT SinkNodeKey FROM EESPathList";
            //dr = cmdTemp.ExecuteReader();
            //rdr(dr);
            VayuConnection.Close();

            List<int> lstPJMSourceSinkUpToList = hash.Keys.Cast<int>().ToList();

            return lstPJMSourceSinkUpToList;
        }

        public List<int> GetUpToNodeKeys()
        {
            LoadDB();
            Hashtable hash = new Hashtable();

            VayuConnection.Open();


            SqlCommand cmdTemp = VayuConnection.CreateCommand();
            //  cmdTemp.CommandText = "SELECT DISTINCT SourceNodeKey FROM EESPathList";
            cmdTemp.CommandText = "select distinct nodekey from PJM.VirtualValidNodes where NodeKey is not null";

            SqlDataReader dr = cmdTemp.ExecuteReader();

            Action<SqlDataReader> rdr = (sqlReader) =>
            {
                while (dr.Read())
                {
                    int key = 0;
                    int.TryParse((dr[0] ?? "").ToString(), out key);
                    hash[key] = key;
                }
                if (dr != null)
                {
                    dr.Close();
                }
            };

            rdr(dr);

            //cmdTemp.CommandText = "SELECT DISTINCT SinkNodeKey FROM EESPathList";
            //dr = cmdTemp.ExecuteReader();
            //rdr(dr);
            VayuConnection.Close();

            List<int> lstPJMSourceSinkUpToList = hash.Keys.Cast<int>().ToList();

            return lstPJMSourceSinkUpToList;
        }

        public List<int> GetFTRNodeKeys()
        {
            LoadDB();
            Hashtable hash = new Hashtable();
            VayuConnection.Open();

            SqlCommand cmdFTRTemp = VayuConnection.CreateCommand();
            //  cmdTemp.CommandText = "SELECT DISTINCT SourceNodeKey FROM EESPathList";
            cmdFTRTemp.CommandText = "select distinct nodekey from PJM.FtrNodes where NodeKey is not null";

            SqlDataReader dr = cmdFTRTemp.ExecuteReader();

            Action<SqlDataReader> rdr = (sqlReader) =>
            {
                while (dr.Read())
                {
                    int key = 0;
                    int.TryParse((dr[0] ?? "").ToString(), out key);
                    hash[key] = key;
                }
                if (dr != null)
                {
                    dr.Close();
                }
            };

            rdr(dr);

            //cmdTemp.CommandText = "SELECT DISTINCT SinkNodeKey FROM EESPathList";
            //dr = cmdTemp.ExecuteReader();
            //rdr(dr);
            VayuConnection.Close();

            List<int> lstPJMSourceSinkFTRList = hash.Keys.Cast<int>().ToList();

            return lstPJMSourceSinkFTRList;
        }

        public List<int> GetVitrualNodeKeys()
        {
            LoadDB();
            Hashtable hash = new Hashtable();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            SqlCommand cmdVirtualTemp = VayuConnection.CreateCommand();
            //  cmdTemp.CommandText = "SELECT DISTINCT SourceNodeKey FROM EESPathList";
            cmdVirtualTemp.CommandText = "select distinct nodekey from PJM.VirtualValidNodes where NodeKey is not null";

            SqlDataReader dr = cmdVirtualTemp.ExecuteReader();

            Action<SqlDataReader> rdr = (sqlReader) =>
            {
                while (dr.Read())
                {
                    int key = 0;
                    int.TryParse((dr[0] ?? "").ToString(), out key);
                    hash[key] = key;
                }
                if (dr != null)
                {
                    dr.Close();
                }
            };

            rdr(dr);

            //cmdTemp.CommandText = "SELECT DISTINCT SinkNodeKey FROM EESPathList";
            //dr = cmdTemp.ExecuteReader();
            //rdr(dr);
            VayuConnection.Close();

            List<int> lstPJMSourceSinkVirtualList = hash.Keys.Cast<int>().ToList();

            return lstPJMSourceSinkVirtualList;
        }
    }
}







//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Vayu.NodePriceLibrary
//{
//    public class NodeHelper
//    {
//        private SqlCommand cmdSelectSourceSinkNodeCommnad;
//        private SqlConnection VayuDBConnection;
//        private static NodeHelper nodeHelper;

//        public static NodeHelper Instance
//        {
//            get
//            {
//                if (nodeHelper == null)
//                {
//                    nodeHelper = new NodeHelper();
//                }
//                return nodeHelper;
//            }
//        }

//        public NodeHelper() { }

//        public void LoadDB()
//        {
//            VayuDBConnection = new SqlConnection();

//            cmdSelectSourceSinkNodeCommnad.CommandText = "select * from EESPathList src INNER join Node n  on n.NodeKey = src.SourceNodeKey "
//                                                + "INNER JOIN EESPathList sink on sink.SinkNodeKey = src.SinkNodeKey AND sink.EESPathListKey = src.EESPathListKey"
//                                                + "INNER JOIN Node n2 on n2.NodeKey = sink.SinkNodeKey where n.MarketKey = @MarketKey  and n2.MarketKey = @MarketKey  and src.MarketKey = @MarketKey  and sink.MarketKey = @MarketKey order by n.NodeName";
//            cmdSelectSourceSinkNodeCommnad.Parameters.AddWithValue("@MarketKey", "MarketKey");
//        }
//         public List<int> GetUpToNodeKeys()
//        {
//            LoadDB();
//            Hashtable hash = new Hashtable();
//            if(VayuDBConnection.State == ConnectionState.Closed)
//            {
//                VayuDBConnection.Open();
//            }
//            SqlCommand cmd = VayuDBConnection.CreateCommand();
//            cmd.CommandText = "select distinct SourceNodeKey from EESPathList";
//            SqlDataReader dr = cmd.ExecuteReader();
//            Action<SqlDataReader> rdr = (sqlReader) =>
//             {
//                 while (dr.Read())
//                 {
//                     int key = 0;
//                     int.TryParse((dr[0] ?? "").ToString(), out key);
//                     hash[key] = key;
//                 }
//                 if (dr != null)
//                 {
//                     dr.Close();
//                 }
//             };
//            rdr(dr);
//            cmd.CommandText= "select distinct SinkNodeKey from EESPathList";
//            dr = cmd.ExecuteReader();
//            rdr(dr);
//            VayuDBConnection.Close();
//            List<int> lstPJMSourceSinkUpToList = hash.Keys.Cast<int>().ToList();
//            return lstPJMSourceSinkUpToList;

//        }
//    }
//}
