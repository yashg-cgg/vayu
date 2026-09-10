using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;
using Vayu.DBLibrary;

namespace Vayu.ConstraintSensitivityAlgorithm.Model
{
    public class DataService : IDataService
    {
        #region Declaration

        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private SqlConnection VayuConnection;

        /// <summary>
        /// The m select constraint contingency command
        /// </summary>
        private SqlCommand mSelectConstraintContingencyCommand;
        private SqlCommand mSelectERCOTConstraintContingencyCommand;
        /// <summary>
        /// The m select vector command
        /// </summary>
        private SqlCommand mSelectVectorCommand;
        private SqlCommand mSelectERCOTVectorCommand;
        /// <summary>
        /// The m select uptos command
        /// </summary>
        private SqlCommand mSelectUptosCommand;
        private SqlCommand mSelectUptosECROTCommand;
        /// <summary>
        /// The m select virtuals command
        /// </summary>
        private SqlCommand mSelectVirtualsCommand;
        /// <summary>
        /// The m select family command
        /// </summary>
        private SqlCommand mSelectFamilyCommand;
        /// <summary>
        /// The m select uptos names command
        /// </summary>
        private SqlCommand mSelectUptosNamesCommand;
        private SqlCommand mSelectERCOTUptosNamesCommand;
        /// <summary>
        /// The m select invalid source command
        /// </summary>
        private SqlCommand mSelectInvalidSourceCommand;
        /// <summary>
        /// The m select invalid sink command
        /// </summary>
        private SqlCommand mSelectInvalidSinkCommand;

        #endregion

        /// <summary>
        /// Loads the database commands.
        /// </summary>
        private void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();            //
            mSelectConstraintContingencyCommand = new SqlCommand();
            mSelectConstraintContingencyCommand.CommandText = "select   monitoredtext, contingencytext, constraintrtnum from rtmasterconstraint where constraintrtnum in " +
                                                                "(select constraintrtnum from riskconstraints where date between @startdate and @enddate) order by monitoredtext";
            mSelectConstraintContingencyCommand.Parameters.AddWithValue("@startdate", "date");
            mSelectConstraintContingencyCommand.Parameters.AddWithValue("@enddate", "date");
            mSelectConstraintContingencyCommand.Connection = VayuConnection;
            //
            mSelectERCOTConstraintContingencyCommand = new SqlCommand();
            mSelectERCOTConstraintContingencyCommand.CommandText = "select   monitoredtext, contingencytext, constraintrtnum  from rtmasterconstraint  " +
                                                                " where MarketDateTime between @startdate and @enddate order by monitoredtext ";
            mSelectERCOTConstraintContingencyCommand.Parameters.AddWithValue("@startdate", "MarketDateTime");
            mSelectERCOTConstraintContingencyCommand.Parameters.AddWithValue("@enddate", "MarketDateTime");
            mSelectERCOTConstraintContingencyCommand.Connection = VayuConnection;
            //
            mSelectVectorCommand = new SqlCommand();
            mSelectVectorCommand.CommandText = "select  nodekey, a.Sensitivity * b.shiftfactor from RTMasterVector_new a, rtmasterconstraint b where a.Sensitivity <> 0 and b.constraintrtnum = @constraintrtnum and a.constraintrtnum = @constraintrtnum and a.sensitivity <> 0";
            mSelectVectorCommand.Parameters.AddWithValue("@constraintrtnum", "constraintrtnum");
            mSelectVectorCommand.Connection = VayuConnection;
            //
            mSelectERCOTVectorCommand = new SqlCommand();
            mSelectERCOTVectorCommand.CommandText = "select nodekey, a.Sensitivity * b.shiftfactor from RTMasterVector_new a, rtmasterconstraint b where a.Sensitivity <> 0 and b.constraintrtnum = @constraintrtnum and a.constraintrtnum = @constraintrtnum and a.sensitivity <> 0";
            mSelectERCOTVectorCommand.Parameters.AddWithValue("@constraintrtnum", "constraintrtnum");
            mSelectERCOTVectorCommand.Connection = VayuConnection;
            //
            mSelectUptosCommand = new SqlCommand();
            mSelectUptosCommand.CommandText = "select  sourcenodekey, sinknodekey from eespathlist where marketkey = 9";
            mSelectUptosCommand.Connection = VayuConnection;

            mSelectUptosECROTCommand = new SqlCommand();
            mSelectUptosECROTCommand.CommandText = "select sourcenodekey, sinknodekey from eespathlist where marketkey = 9";
            mSelectUptosECROTCommand.Connection = VayuConnection;

            mSelectUptosNamesCommand = new SqlCommand();
            mSelectUptosNamesCommand.CommandText = "select SourceName, SinkName from eespathlist where marketkey = 1";
            mSelectUptosNamesCommand.Connection = VayuConnection;

            mSelectERCOTUptosNamesCommand = new SqlCommand();
            mSelectERCOTUptosNamesCommand.CommandText = "select  SourceName, SinkName from  eespathlist where marketkey =9 ";
            mSelectERCOTUptosNamesCommand.Connection = VayuConnection;

            //
            mSelectVirtualsCommand = new SqlCommand();
            mSelectVirtualsCommand.CommandText = "select  nodekey from node where marketkey = 1";
            mSelectVirtualsCommand.Connection = VayuConnection;
            //

            mSelectFamilyCommand = new SqlCommand();
            mSelectFamilyCommand.CommandText = "select  monitoredtext from RTFamily where ConstraintRTNum=@ConstraintRTNum";
            mSelectFamilyCommand.Parameters.AddWithValue("@constraintrtnum", "ConstraintRTNum");
            mSelectFamilyCommand.Connection = VayuConnection;

            //
            mSelectInvalidSourceCommand = new SqlCommand();
            mSelectInvalidSourceCommand.CommandText = "select distinct NodeName from InvalidUptosSource";
            mSelectInvalidSourceCommand.Connection = VayuConnection;

            //
            mSelectInvalidSinkCommand = new SqlCommand();
            mSelectInvalidSinkCommand.CommandText = "select distinct NodeName from InvalidUptosSink";
            mSelectInvalidSinkCommand.Connection = VayuConnection;
        }
        /// <summary>
        /// Gets the vectors.
        /// </summary>
        /// <param name="constraintContingencyList">The constraint contingency list.</param>
        /// <param name="isUptos">if set to <c>true</c> [is uptos].</param>
        /// <returns></returns>
        /// 
        SqlDataReader readerGetVect;

        public List<Vector> GetVectors(List<ConstraintContingency> constraintContingencyList, bool isUptos, int Market)
        {

            loadDBCommands();

            List<int> sourcenodeList = new List<int>();
            List<int> sinknodeList = new List<int>();
            Dictionary<string, Vector> vectorHash = new Dictionary<string, Vector>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            if (Market == 1)
            {
                readerGetVect = isUptos ? mSelectUptosCommand.ExecuteReader() : mSelectVirtualsCommand.ExecuteReader();
            }
            else if (Market == 9)
            {
                readerGetVect = isUptos ? mSelectUptosECROTCommand.ExecuteReader() : mSelectVirtualsCommand.ExecuteReader(); ;
            }

            while (readerGetVect.Read())
            {
                int sourceNode = Vayu.CommonAccessLibrary.CommonDataConversions.GetInt(readerGetVect[0]).GetValueOrDefault();
                if (!sourcenodeList.Contains(sourceNode))
                {
                    sourcenodeList.Add(sourceNode);
                }
                if (isUptos)
                {
                    int sinkNode = Vayu.CommonAccessLibrary.CommonDataConversions.GetInt(readerGetVect[1]).GetValueOrDefault();
                    if (!sinknodeList.Contains(sinkNode))
                    {
                        sinknodeList.Add(sinkNode);
                    }
                }
            }
            readerGetVect.Close();
            // readerGetVect.Dispose();
            foreach (ConstraintContingency constraint in constraintContingencyList)
            {
                if (Market == 1)
                {
                    mSelectVectorCommand.Parameters["@constraintrtnum"].Value = constraint.ID;
                    readerGetVect = mSelectVectorCommand.ExecuteReader();
                }
                else if (Market == 9)
                {
                    mSelectERCOTVectorCommand.Parameters["@constraintrtnum"].Value = constraint.ID;
                    readerGetVect = mSelectERCOTVectorCommand.ExecuteReader();
                }

                while (readerGetVect.Read())
                {
                    int nodeKey = (int)readerGetVect.GetDecimal(0);
                    if (!sourcenodeList.Contains(nodeKey))
                    {
                        continue;
                    }
                    if (isUptos)
                    {
                        if (!sinknodeList.Contains(nodeKey))
                        {
                            continue;
                        }
                    }
                    Vector vector = new Vector();
                    PricingNode pnode = DBAccess.GetNode(nodeKey, Market);
                    if (pnode == null)
                        continue;

                    vector.Source = pnode.NodeName;
                    vector.SourceSensitivity = (double)readerGetVect.GetDecimal(1);
                    if (vectorHash.ContainsKey(vector.Source))
                    {
                        Vector tempVector = vectorHash[vector.Source];
                        if (Math.Abs(vector.SourceSensitivity) > Math.Abs(tempVector.SourceSensitivity))
                        {
                            vectorHash[vector.Source] = vector;
                        }
                    }
                    else
                    {
                        vectorHash.Add(vector.Source, vector);
                    }
                }
                readerGetVect.Close();
            }
            VayuConnection.Close();
            return new List<Vector>(vectorHash.Values);
        }
        /// Gets the path.
        /// </summary>
        /// <returns></returns>
        /// 
        SqlDataReader readerPath;
        public List<SourceSink> GetPath(int MarketId)
        {
            loadDBCommands();
            List<SourceSink> SourceSinkHash = new List<SourceSink>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            if (MarketId == 1)
            {
                readerPath = mSelectUptosNamesCommand.ExecuteReader();
            }
            else if (MarketId == 9)
            {
                readerPath = mSelectERCOTUptosNamesCommand.ExecuteReader();
            }
            while (readerPath.Read())
            {
                SourceSink mSourceSink = new SourceSink();
                mSourceSink.Source = readerPath.GetString(0);
                mSourceSink.Sink = readerPath.GetString(1);
                SourceSinkHash.Add(mSourceSink);
            }
            readerPath.Close();
            VayuConnection.Close();
            return SourceSinkHash;
        }
        /// <summary>
        /// Gets the constraint contingency.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        /// 

        SqlDataReader readerGetConstr;
        public List<ConstraintContingency> GetConstraintContingency(DateTime date, int MarketId)
        {
            try
            {
                loadDBCommands();
                List<ConstraintContingency> constraintContingencyList = new List<ConstraintContingency>();
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                int year = DateTime.Now.Year;
                DateTime startDate = new DateTime(year - 4, 1, 1);
                DateTime endDate = new DateTime(year, 12, 31);
                if (MarketId == 1)
                {
                    mSelectConstraintContingencyCommand.Parameters["@startdate"].Value = startDate;
                    mSelectConstraintContingencyCommand.Parameters["@enddate"].Value = endDate;
                    readerGetConstr = mSelectConstraintContingencyCommand.ExecuteReader();
                }
                else if (MarketId == 9)
                {
                    mSelectERCOTConstraintContingencyCommand.Parameters["@startdate"].Value = startDate;
                    mSelectERCOTConstraintContingencyCommand.Parameters["@enddate"].Value = endDate;
                    readerGetConstr = mSelectERCOTConstraintContingencyCommand.ExecuteReader();

                }
                while (readerGetConstr.Read())
                {
                    ConstraintContingency constraintContingency = new ConstraintContingency();
                    constraintContingency.Constraint = readerGetConstr.IsDBNull(0) ? null : readerGetConstr.GetString(0).Replace("COMPANY", "").Replace("Interface", "").Replace("Monitor", "").Replace("Actual", "").Trim();
                    constraintContingency.Contingency = readerGetConstr.IsDBNull(1) ? null : readerGetConstr.GetString(1).Replace("Contingency  ", "").Trim();
                    constraintContingency.ID = (int)readerGetConstr.GetDecimal(2);
                    constraintContingency.Family = GetFamily(constraintContingency.ID);
                    constraintContingencyList.Add(constraintContingency);
                }
                readerGetConstr.Close();
                VayuConnection.Close();
                return constraintContingencyList;
            }
            catch (Exception)
            {
                return null;
                // throw;
            }
        }

        /// <summary>
        /// Gets the family.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        private string GetFamily(int p)
        {
            try
            {
                loadDBCommands();
                string FamilyName = string.Empty;
                if (p > 0)
                {
                    if (VayuConnection.State == ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    mSelectFamilyCommand.Parameters["@ConstraintRTNum"].Value = p;
                    SqlDataReader reader = mSelectFamilyCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        FamilyName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                    }
                    reader.Close();
                    VayuConnection.Close();
                }
                return FamilyName;
            }
            catch (Exception)
            {
                return null;
                //throw;
            }
        }

        /// <summary>
        /// Gets the invalid source sink.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<Vector>> GetInvalidSourceSink()
        {
            try
            {
                Dictionary<string, List<Vector>> InvalidSourceSinkHash = new Dictionary<string, List<Vector>>();
                List<Vector> InvalidSourceData = new List<Vector>();
                List<Vector> InvalidSinkData = new List<Vector>();
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                mSelectInvalidSourceCommand.CommandTimeout = 30000;
                SqlDataReader reader = mSelectInvalidSourceCommand.ExecuteReader();
                while (reader.Read())
                {
                    Vector sourcevector = new Vector();
                    sourcevector.Source = reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0));
                    InvalidSourceData.Add(sourcevector);
                }
                InvalidSourceSinkHash.Add("Source", InvalidSourceData);
                mSelectInvalidSinkCommand.CommandTimeout = 30000;
                SqlDataReader reader1 = mSelectInvalidSinkCommand.ExecuteReader();
                while (reader1.Read())
                {
                    Vector sinkvector = new Vector();
                    sinkvector.Sink = reader1.IsDBNull(0) ? "" : Convert.ToString(reader1.GetValue(0));
                    InvalidSinkData.Add(sinkvector);
                }
                InvalidSourceSinkHash.Add("Sink", InvalidSinkData);
                VayuConnection.Close();
                return InvalidSourceSinkHash;
            }
            catch (Exception)
            {
                return null;
                // throw;
            }
        }
    }
}