
using Vayu;
using Vayu.LatestConstraintsInformationLibrary;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.LatestConstraintInformationService
{

    /// <summary>
    /// 
    /// </summary>
    public partial class ConstrainInformationServer
    {
        #region Private Members
        /// <summary>
        /// The realtime object
        /// </summary>
        private object rtObject = new object();
        /// <summary>
        /// The dayahead object
        /// </summary>
        private object daObject = new object();

        /// <summary>
        /// The CMDSPP realtime 5 minimum constraint
        /// </summary>
        private SqlCommand cmdsppRT5MinConstraint;
        /// <summary>
        /// The CMDSPP RTH minimum constraint
        /// </summary>
        private SqlCommand cmdsppRTHMinConstraint;
        /// <summary>
        /// The CMDSPP dayahead minimum constraint
        /// </summary>
        private SqlCommand cmdsppDAHMinConstraint;

        /// <summary>
        /// The cmdnyiso realtime 5 minimum constraint
        /// </summary>
        private SqlCommand cmdnyisoRT5MinConstraint;
        /// <summary>
        /// The cmdnyiso RTH minimum constraint
        /// </summary>
        private SqlCommand cmdnyisoRTHMinConstraint;
        /// <summary>
        /// The cmdnyiso dayahead minimum constraint
        /// </summary>
        private SqlCommand cmdnyisoDAHMinConstraint;

        /// <summary>
        /// The cmdercot realtime 5 minimum constraint
        /// </summary>
        private SqlCommand cmdercotRT5MinConstraint;
        /// <summary>
        /// The cmdercot RTH minimum constraint
        /// </summary>
        private SqlCommand cmdercotRTHMinConstraint;
        /// <summary>
        /// The cmdercot dayahead minimum constraint
        /// </summary>
        private SqlCommand cmdercotDAHMinConstraint;

        /// <summary>
        /// The cmdcaiso realtime 5 minimum constraint
        /// </summary>
        private SqlCommand cmdcaisoRT5MinConstraint;
        /// <summary>
        /// The cmdcaiso RTH minimum constraint
        /// </summary>
        private SqlCommand cmdcaisoRTHMinConstraint;
        /// <summary>
        /// The cmdcaiso dayahead minimum constraint
        /// </summary>
        private SqlCommand cmdcaisoDAHMinConstraint;

        /// <summary>
        /// The cmdmiso realtime 5 minimum constraint
        /// </summary>
        private SqlCommand cmdmisoRT5MinConstraint;
        /// <summary>
        /// The cmdmiso RTH minimum constraint
        /// </summary>
        private SqlCommand cmdmisoRTHMinConstraint;
        /// <summary>
        /// The cmdmiso dah minimum constraint
        /// </summary>
        private SqlCommand cmdmisoDAHMinConstraint;

        /// <summary>
        /// The CMDPJM realtime 5 minimum constraint
        /// </summary>
        private SqlCommand cmdpjmRT5MinConstraint;
        /// <summary>
        /// The CMDPJM RTH minimum constraint
        /// </summary>
        private SqlCommand cmdpjmRTHMinConstraint;
        /// <summary>
        /// The CMDPJM dayahead minimum constraint
        /// </summary>
        private SqlCommand cmdpjmDAHMinConstraint;
        #endregion

        #region Public Methods
        /// <summary>
        /// Partials the initialize.
        /// </summary>
        public void PartialInit()
        {
            

             
            //-----ERCOT
            cmdercotRT5MinConstraint = Configuration.GetErcotDBCommand();
            //cmdercotRT5MinConstraint.CommandText = " select distinct c.MarketDateTime,c.ContingencyText ,c.ConstraintText,ShadowPrice,g.SourceNodeKey,g.SinkNodeKey,5,g.ConstraintKV,g.Type " +
            //" from [ERCOT].[dbo].[ConstraintRT] c left join [ERCOT].[dbo].ConstraintGeo g on c.ConstraintText = g.ConstraintName and c.ContingencyText = g.ContingencyName " +
            //" where c.MarketDateTime between @startDate and @enddate order by MarketDateTime desc ";
            cmdercotRT5MinConstraint.CommandText = " select distinct c.MarketDateTime,c.ContingencyText ,c.ConstraintText,ShadowPrice,d.SourceNodeKey,d.SinkNodeKey , MAX(maxSHadowPrice) maxSHadowPrice  from Vayu..[ConstraintRT] c left  join Vayu..ConstraintGeo d on  " +
" c.ConstraintText =d.ConstraintText and c.ContingencyText=d.ContingencyText  where c.MarketDateTime between  @startDate  and  @enddate group by c.MarketDateTime,c.ContingencyText ,c.ConstraintText,ShadowPrice,d.SourceNodeKey,d.SinkNodeKey order by MarketDateTime desc";
            cmdercotRT5MinConstraint.Parameters.AddWithValue("@startDate", "");
            cmdercotRT5MinConstraint.Parameters.AddWithValue("@enddate", "");

            cmdercotRTHMinConstraint = Configuration.GetErcotDBCommand();
            cmdercotRTHMinConstraint.CommandText = " select distinct a.MarketDateTime ,a.ContingencyText, a.ConstraintText, SUM(A.ShadowPrice)/12 as ShadowPrice , a.SourceNodeKey,a.SinkNodeKey , " +
" COUNT(a.MarketDateTime) * 5 as Duration ,ConstraintKV , Type , maxShadowPrice from (select distinct DATEADD(HOUR, DATEDIFF(HOUR, '20000101', DATEADD(minute,-5, sc.MarketDateTime)) + 1,'20000101') as MarketDateTime, " +
" sc.ContingencyText as ContingencyText,sc.ConstraintText,sc.ShadowPrice,sg.SourceNodeKey,sg.SinkNodeKey ,sg.ConstraintKV,sg.Type" +
" from [dbo].ConstraintRT sc left join [dbo].ConstraintGeo sg on sc.ConstraintText = sg.ConstraintText and sc.ContingencyText = sg.ContingencyText " +
" where sc.MarketDateTime between @startDate and @enddate ) a " +
" group by a.MarketDateTime , a.ConstraintText,a.ContingencyText,a.SourceNodeKey,a.SinkNodeKey ,a.ConstraintKV,a.Type";
            cmdercotRTHMinConstraint.Parameters.AddWithValue("@startDate", "");
            cmdercotRTHMinConstraint.Parameters.AddWithValue("@enddate", "");

            cmdercotDAHMinConstraint = Configuration.GetErcotDBCommand();
            //cmdercotDAHMinConstraint.CommandText = " select distinct c.MarketDateTime, c.ContingencyText , c.ConstraintText ,c.ShadowPrice, cg.SourceNodeKey,cg.SinkNodeKey,cg.ConstraintKV,cg.Type from [ERCOT].[dbo].ConstraintDA c left join " +
            //" [ERCOT].[dbo].ConstraintGeo cg on c.ConstraintText = cg.ConstraintName and c.ContingencyText  = cg.ContingencyName where c.MarketDateTime between @startDate and @enddate order by MarketDateTime desc ";
            cmdercotDAHMinConstraint.CommandText = "select distinct c.MarketDateTime,c.ContingencyText ,c.ConstraintText,ShadowPrice,d.SourceNodeKey,d.SinkNodeKey from Vayu..[Constraintda] c left join Vayu..ConstraintGeo d on  " +
" c.ConstraintText =d.ConstraintText and c.ContingencyText=d.ContingencyText  where c.MarketDateTime between  @startDate  and  @enddate order by MarketDateTime desc";
            cmdercotDAHMinConstraint.Parameters.AddWithValue("@startDate", "");
            cmdercotDAHMinConstraint.Parameters.AddWithValue("@enddate", "");

            

            //-----MISO
            cmdmisoRT5MinConstraint = Configuration.GetErcotDBCommand();
            cmdmisoRT5MinConstraint.CommandText = " select distinct c.MarketDateTime,c.ContingencyName ,c.ConstraintName,ShadowPrice,g.RTSourceNodeKey,g.RTSinkNodeKey,5 ,g.ConstraintKV,g.Type" +
            " from MISO.[ConstraintRT] c left join MISO.ConstraintGeo g on c.ConstraintName = g.ConstraintName and c.ContingencyName = g.ContingencyName " +
            " where c.MarketDateTime between @startDate and @enddate order by MarketDateTime desc ";
            cmdmisoRT5MinConstraint.Parameters.AddWithValue("@startDate", "");
            cmdmisoRT5MinConstraint.Parameters.AddWithValue("@enddate", "");

            cmdmisoRTHMinConstraint = Configuration.GetErcotDBCommand();
            cmdmisoRTHMinConstraint.CommandText = " select distinct a.MarketDateTime ,a.ContingencyName, a.ConstraintName, SUM(A.ShadowPrice)/12 as ShadowPrice , a.RTSourceNodeKey,a.RTSinkNodeKey , " +
            " COUNT(a.MarketDateTime) * 5 as Duration ,ConstraintKV , Type from (select distinct DATEADD(HOUR, DATEDIFF(HOUR, '20000101', DATEADD(minute,-5, sc.MarketDateTime)) + 1,'20000101') as MarketDateTime, " +
            " sc.ContingencyName as ContingencyName,sc.ConstraintName ,sc.ShadowPrice,sg.RTSourceNodeKey,sg.RTSinkNodeKey ,sg.ConstraintKV,sg.Type" +
            " from MISO.ConstraintRT sc left join MISO.ConstraintGeo sg on sc.ConstraintName = sg.ConstraintName and sc.ContingencyName = sg.ContingencyName " +
            " where sc.MarketDateTime between @startDate and @enddate ) a " +
            " group by a.MarketDateTime , a.ConstraintName,a.ContingencyName,a.RTSourceNodeKey,a.RTSinkNodeKey ,a.ConstraintKV,a.Type ";
            cmdmisoRTHMinConstraint.Parameters.AddWithValue("@startDate", "");
            cmdmisoRTHMinConstraint.Parameters.AddWithValue("@enddate", "");

            cmdmisoDAHMinConstraint = Configuration.GetErcotDBCommand();
            cmdmisoDAHMinConstraint.CommandText = "select distinct c.MarketDateTime, c.ContingencyName , c.ConstraintName ,c.ShadowPrice, cg.DASourceNodeKey ,cg.DASinkNodeKey ,cg.ConstraintKV,cg.Type from MISO.ConstraintDA c left join " +
            " MISO.ConstraintGeo cg on c.ConstraintName = cg.ConstraintName and c.ContingencyName  = cg.ContingencyName where c.MarketDateTime between @startDate and @enddate order by MarketDateTime desc  ";
            cmdmisoDAHMinConstraint.Parameters.AddWithValue("@startDate", "");
            cmdmisoDAHMinConstraint.Parameters.AddWithValue("@enddate", "");

            //-----PJM
            cmdpjmRT5MinConstraint = Configuration.GetErcotDBCommand();
            cmdpjmRT5MinConstraint.CommandText = " select distinct c.MarketDateTime,c.ContingencyText ,c.ConstraintText,ShadowPrice,g.SourceNodeKey ,g.SinkNodeKey,5 ,g.ConstraintKV,g.Type " +
             " from PJM.[ConstraintRT] c left join PJM.ConstraintGeo g on c.ConstraintText = g.ConstraintName and c.ContingencyText = g.ContingencyName " +
             " where c.MarketDateTime >= @startDate and MarketDateTime < @enddate order by MarketDateTime desc ";
            //cmdpjmRT5MinConstraint.CommandText = " select distinct c.MarketDateTime,c.ContingencyText ,c.ConstraintText,ShadowPrice,g.SourceNodeKey ,g.SinkNodeKey,5 ,g.ConstraintKV,g.Type, " +
            //" MAX(c.ShadowPrice) MaxShadowprice  from NewTrading.PJM.[ConstraintRT] c left join NewTrading.PJM.ConstraintGeo g on c.ConstraintText = g.ConstraintName and c.ContingencyText = g.ContingencyName  " +
            //" where c.MarketDateTime >= @startDate and MarketDateTime < @enddate " +
            //" group by c.MarketDateTime,c.ContingencyText ,c.ConstraintText,ShadowPrice,g.SourceNodeKey ,g.SinkNodeKey ,g.ConstraintKV,g.Type order by MarketDateTime desc ";
            cmdpjmRT5MinConstraint.Parameters.AddWithValue("@startDate", "");
            cmdpjmRT5MinConstraint.Parameters.AddWithValue("@enddate", "");

            cmdpjmRTHMinConstraint = Configuration.GetErcotDBCommand();
            cmdpjmRTHMinConstraint.CommandText = " select distinct a.MarketDateTime ,a.ContingencyName, a.ConstraintText , SUM(A.ShadowPrice)/12 as ShadowPrice , a.SourceNodeKey,a.SinkNodeKey , " +
             " COUNT(a.MarketDateTime) * 5 as Duration ,ConstraintKV , Type from (select distinct  DATEADD(HOUR, DATEDIFF(HOUR, '20000101', DATEADD(minute,-5, sc.MarketDateTime)) + 1,'20000101') as MarketDateTime, " +
             " sc.ContingencyText as ContingencyName,sc.ConstraintText ,sc.ShadowPrice,sg.SourceNodeKey,sg.SinkNodeKey ,sg.ConstraintKV,sg.Type " +
             " from PJM.ConstraintRT sc left join PJM.ConstraintGeo sg on sc.ConstraintText = sg.ConstraintName and sc.ContingencyText = sg.ContingencyName " +
             " where sc.MarketDateTime between @startDate and @enddate ) a group by a.MarketDateTime , a.ConstraintText,a.ContingencyName,a.SourceNodeKey,a.SinkNodeKey,a.ConstraintKV,a.Type ";
            cmdpjmRTHMinConstraint.Parameters.AddWithValue("@startDate", "");
            cmdpjmRTHMinConstraint.Parameters.AddWithValue("@enddate", "");

            cmdpjmDAHMinConstraint = Configuration.GetErcotDBCommand();
            cmdpjmDAHMinConstraint.CommandText = "select distinct c.MarketDateTime, c.ContingencyText , c.ConstraintText ,c.ShadowPrice, cg.SourceNodeKey ,cg.SinkNodeKey ,cg.ConstraintKV , cg.Type from PJM.ConstraintDA c left join " +
             " PJM.ConstraintGeo cg on c.ConstraintText = cg.ConstraintName and c.ContingencyText = cg.ContingencyName where c.MarketDateTime between @startDate and @enddate order by MarketDateTime desc  ";
            cmdpjmDAHMinConstraint.Parameters.AddWithValue("@startDate", "");
            cmdpjmDAHMinConstraint.Parameters.AddWithValue("@enddate", "");
        }

        /// <summary>
        /// Gets the constraint realtime.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="hourly">if set to <c>true</c> [hourly].</param>
        /// <returns>Constraint List</returns>
        public List<LatestConstraint> GetConstraintRT(int marketKey, DateTime start, DateTime end, bool hourly)
        {
            lock (rtObject)
            {
                PartialInit();
                List<LatestConstraint> constraintList = new List<LatestConstraint>();
                SqlCommand command = null;
                switch (marketKey)
                {
                     
                    case 9:
                        if (hourly)
                            command = cmdercotRTHMinConstraint;
                        else
                            command = cmdercotRT5MinConstraint;
                        break;

                    

                    default:
                        return null;
                }

                command.Parameters["@startDate"].Value = start;
                command.Parameters["@enddate"].Value = end;

                if (command.Connection.State != System.Data.ConnectionState.Open)
                    command.Connection.Open();

                try
                {
                    SqlDataReader reader = command.ExecuteReader();
                    if (marketKey == 9)
                    {
                        while (reader.Read())
                        {
                            LatestConstraint constraint = new LatestConstraint();
                            constraint.MarketDate = Configuration.GetDate(reader[0]);
                            constraint.ContigencyText = reader[1].ToString();
                            constraint.ConstraintText = reader[2].ToString();
                            constraint.ShadowPrice = Configuration.GetDouble(reader[3]);
                            constraint.SourceNodeKey = Configuration.GetInt(reader[4]);
                            constraint.SinkNodeKey = Configuration.GetInt(reader[5]);
                            constraint.MaxSHadowPrice = Configuration.GetDouble(reader[6]);
                            
                            constraintList.Add(constraint);
                        }
                    }
                    else if (marketKey == 12)
                    {
                        while (reader.Read())
                        {
                            LatestConstraint constraint = new LatestConstraint();
                            constraint.MarketDate = Configuration.GetDate(reader[0]);
                            constraint.ContigencyText = reader[1].ToString();
                            constraint.ConstraintText = reader[2].ToString();
                            constraint.ShadowPrice = Configuration.GetDouble(reader[3]);
                            constraint.SourceNodeKey = Configuration.GetInt(reader[4]);
                            constraint.SinkNodeKey = Configuration.GetInt(reader[5]);
                            constraint.Duration = Configuration.GetInt(reader[6]);
                            constraint.MonitoredFacility = reader[7].ToString();
                            constraint.ConstraintKV = Configuration.GetDouble(reader[8]);
                            constraint.ConstraintType = reader[9].ToString();
                            constraint.ShadowPriceNaN = Configuration.GetDoubleNaN(reader[3]);
                            constraintList.Add(constraint);
                        }
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            LatestConstraint constraint = new LatestConstraint();
                            constraint.MarketDate = Configuration.GetDate(reader[0]);
                            constraint.ContigencyText = reader[1].ToString();
                            constraint.ConstraintText = reader[2].ToString();
                            constraint.ShadowPrice = Configuration.GetDouble(reader[3]);
                            constraint.SourceNodeKey = Configuration.GetInt(reader[4]);
                            constraint.SinkNodeKey = Configuration.GetInt(reader[5]);
                            constraint.Duration = Configuration.GetInt(reader[6]);
                            constraint.ConstraintKV = Configuration.GetDouble(reader[7]);
                            constraint.ConstraintType = reader[8].ToString();
                            constraint.ShadowPriceNaN = Configuration.GetDoubleNaN(reader[3]);
                            // constraint.MaxSHadowPrice = Configuration.GetDouble(reader[9]);
                            constraintList.Add(constraint);
                        }
                    }
                    reader.Close();
                }
                catch { }
                finally
                {
                    if (command.Connection.State != System.Data.ConnectionState.Closed)
                        command.Connection.Close();
                }

                return constraintList;
            }
        }

        /// <summary>
        /// Gets the constraint dayahead.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>Constraint List</returns>
        public List<LatestConstraint> GetConstraintDA(int marketKey, DateTime start, DateTime end)
        {
            lock (daObject)
            {
                PartialInit();
                List<LatestConstraint> constraintList = new List<LatestConstraint>();
                SqlCommand command = null;
                switch (marketKey)
                {
                   

                    case 9:
                        command = cmdercotDAHMinConstraint;
                        break;

                   
                    default:
                        return null;
                }

                command.Parameters["@startDate"].Value = start;
                command.Parameters["@enddate"].Value = end;

                if (command.Connection.State != System.Data.ConnectionState.Open)
                    command.Connection.Open();

                try
                {
                    SqlDataReader reader = command.ExecuteReader();
                    if (marketKey == 9)
                    {
                        while (reader.Read())
                        {
                            LatestConstraint constraint = new LatestConstraint();
                            constraint.MarketDate = Configuration.GetDate(reader[0]);
                            constraint.ContigencyText = reader[1].ToString();
                            constraint.ConstraintText = reader[2].ToString();
                            constraint.ShadowPrice = Configuration.GetDouble(reader[3]);
                            constraint.SourceNodeKey = Configuration.GetInt(reader[4]);
                            constraint.SinkNodeKey = Configuration.GetInt(reader[5]);
                            //constraint.ConstraintKV = Configuration.GetDouble(reader[6]);
                            //constraint.ConstraintType = reader[7].ToString();
                            constraintList.Add(constraint);
                        }
                    }
                    else if (marketKey == 12)
                    {
                        while (reader.Read())
                        {
                            LatestConstraint constraint = new LatestConstraint();
                            constraint.MarketDate = Configuration.GetDate(reader[0]);
                            constraint.ContigencyText = reader[1].ToString();
                            constraint.ConstraintText = reader[2].ToString();
                            constraint.ShadowPrice = Configuration.GetDouble(reader[3]);
                            constraint.SourceNodeKey = Configuration.GetInt(reader[4]);
                            constraint.SinkNodeKey = Configuration.GetInt(reader[5]);
                            constraint.ConstraintKV = Configuration.GetDouble(reader[6]);
                            constraint.ConstraintType = reader[7].ToString();
                            constraint.MonitoredFacility = reader[8].ToString();
                            constraintList.Add(constraint);
                        }
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            LatestConstraint constraint = new LatestConstraint();
                            constraint.MarketDate = Configuration.GetDate(reader[0]);
                            constraint.ContigencyText = reader[1].ToString();
                            constraint.ConstraintText = reader[2].ToString();
                            constraint.ShadowPrice = Configuration.GetDouble(reader[3]);
                            constraint.SourceNodeKey = Configuration.GetInt(reader[4]);
                            constraint.SinkNodeKey = Configuration.GetInt(reader[5]);
                            constraint.ConstraintKV = Configuration.GetDouble(reader[6]);
                            constraint.ConstraintType = reader[7].ToString();
                            constraintList.Add(constraint);
                        }
                    }
                    reader.Close();
                }
                catch { }
                finally
                {
                    if (command.Connection.State != System.Data.ConnectionState.Closed)
                        command.Connection.Close();
                }

                return constraintList;
            }
        }
        #endregion
    }
}
