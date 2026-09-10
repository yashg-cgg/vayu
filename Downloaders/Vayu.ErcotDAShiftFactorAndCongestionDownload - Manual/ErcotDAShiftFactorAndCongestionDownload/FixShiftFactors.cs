using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.ErcotDAShiftFactorAndCongestionDownload
{
      class FixShiftFactors
    {
        private SqlConnection VayuDBConnection;
        private SqlCommand mUpdateErcotConstraintCommand;
        private SqlCommand mInsertErcotConstraintCommand;
        Dictionary<DateTime, List<ConstraintHelper>> mConstraintDict;
        private void InitDB()
        {
            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
        }
        public void GetDAConstraints()
        {
            InitDB();
            mConstraintDict = new Dictionary<DateTime, List<ConstraintHelper>>();
            if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
            {
                VayuDBConnection.Open();
            }
            SqlCommand SelectConstraintCommand = VayuDBConnection.CreateCommand();
            SelectConstraintCommand.Connection = VayuDBConnection;
            SelectConstraintCommand.CommandText = " select  a.MarketDateTime , a.MonitoredText , a.ContingencyText , a.ShadowPrice  , " +
                "SUM(b.ShadowPrice) ,  a.NumberConstraints from Vayu..DAMasterConstraint a  join Vayu..constraintDA  b " +
                " on a.MarketDateTime = b.MarketDateTime where ShiftFactor = 0  group by a.MonitoredText , a.ContingencyText , " +
                "a.ShadowPrice  , a.MarketDateTime ,  a.NumberConstraints  order by a.MarketDateTime desc  ";
            SqlDataReader reader = SelectConstraintCommand.ExecuteReader();
            while (reader.Read())
            {
                DateTime marketDateTime = Convert.ToDateTime(reader.GetValue(0));
                ConstraintHelper helper = new ConstraintHelper();
                string Constraint = Convert.ToString(reader.GetValue(1));
                string Contigency = Convert.ToString(reader.GetValue(2));
                double ShadowPrice = Convert.ToDouble(reader.GetValue(3));
                double TotalShadowPrice = Convert.ToDouble(reader.GetValue(4));
                int ConstraintCount = Convert.ToInt32(reader.GetValue(5));
                helper.ConstraintName = Constraint;
                helper.ContingencyName = Contigency;
                helper.ShadowPrice = ShadowPrice;
                helper.TotalSP = TotalShadowPrice;
                helper.ConstraintCount = ConstraintCount;
                if (!mConstraintDict.ContainsKey(marketDateTime))
                {
                    List<ConstraintHelper> templist = new List<ConstraintHelper>();
                    templist.Add(helper);
                    mConstraintDict.Add(marketDateTime, templist);
                }
                else
                {
                    List<ConstraintHelper> templist = mConstraintDict[marketDateTime];
                    templist.Add(helper);
                }
            }
            reader.Close();
            VayuDBConnection.Close();
            foreach (var item in mConstraintDict)
            {
                DateTime date = item.Key;
                List<ConstraintHelper> helperlist = item.Value;
                foreach (ConstraintHelper helper in helperlist)
                {
                    int id = InsertToMasterConstraint(helper, date, helper.ConstraintCount, helper.TotalSP);
                }
            }
        }
        public int InsertToMasterConstraint(ConstraintHelper Constraint, DateTime marketDateTime, int ConstraintCount, double TotalShadowPrice)
        {
            #region PriceRange
            //int ConstraintId = 0;
            double minLmp = double.MaxValue;
            double maxLmp = double.MinValue;
            if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
            {
                VayuDBConnection.Open();
            }
            SqlCommand SelectLMPCommand = VayuDBConnection.CreateCommand();
            SelectLMPCommand.Connection = VayuDBConnection;
            SelectLMPCommand.CommandText = " select MIN(LMP), MAX(LMP) from  Vayu..nodelmpmin where MarketDate = '" + 
                marketDateTime.Date.ToString("yyyy/MM/dd") + "' and MarketHour= " + marketDateTime.Hour.ToString() +
                " and MarketMin= " + marketDateTime.Minute.ToString();
            SqlDataReader reader = SelectLMPCommand.ExecuteReader();
            while (reader.Read())
            {
                minLmp = reader.IsDBNull(0) ? 0 : Convert.ToDouble(reader.GetValue(0));
                maxLmp = reader.IsDBNull(0) ? 0 : Convert.ToDouble(reader.GetValue(1));
            }
            reader.Close();
            if (maxLmp == 0 && minLmp == 0)
            {
                minLmp = double.MaxValue;
                maxLmp = double.MinValue;
            }
                /*if (maxLmp == 0 && minLmp == 0)
                {
                    SelectLMPCommand.CommandText = " select MIN(LMP), MAX(LMP) from  Vayu..nodelmpmin where MarketDate = '" +  
                        marketDateTime.Date.ToString("yyyy/MM/dd") + "' and MarketHour= " + marketDateTime.Hour.ToString();
                    try
                    {
                        reader = SelectLMPCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            minLmp = reader.IsDBNull(0) ? 0 : Convert.ToDouble(reader.GetValue(0));
                            maxLmp = reader.IsDBNull(0) ? 0 : Convert.ToDouble(reader.GetValue(1));
                        }
                    }
                    catch (Exception ex)
                    {
                        maxLmp = 0;
                        minLmp = 0;
                    }
                }*/
            VayuDBConnection.Close();
            #endregion PriceRange
            if (minLmp != double.MinValue && maxLmp != double.MaxValue)
            {
                ConstraintElement element = new ConstraintElement();
                element.Constraint = Constraint.ConstraintName;
                element.Contingency = Constraint.ContingencyName;
                element.ShadowPrice = Constraint.ShadowPrice;
                element.ShiftFactor = minLmp == 0 && maxLmp == 0 ? 0 : (Math.Abs(maxLmp - minLmp) / TotalShadowPrice) * 
                                                                                (Constraint.ShadowPrice / TotalShadowPrice);
                element.Score = 0;
                element.ImpactRatio = 1;
                element.Impact = Constraint.ShadowPrice * element.ShiftFactor;
                element.UpdateTime = DateTime.Now;
                element.MarketDateTimeInterval = marketDateTime;
                #region InsertandUpdate
                mUpdateErcotConstraintCommand = new SqlCommand();
                mUpdateErcotConstraintCommand.Connection = VayuDBConnection;
                mUpdateErcotConstraintCommand.CommandText = "update Vayu..DAMasterConstraint set marketdatetime = @marketdatetime, " +
                                                                "shadowprice = @shadowprice, numberconstraints = @numberconstraints, " +
                                                            "shiftfactor = @shiftfactor, dollarimpact = @dollarimpact, updatetime = " +
                                                            "@updatetime where monitoredtext = @MonitoredText and contingencytext = @ContingencyText";
                mUpdateErcotConstraintCommand.Parameters.AddWithValue("@marketdatetime", element.MarketDateTimeInterval);
                mUpdateErcotConstraintCommand.Parameters.AddWithValue("@shadowprice", element.ShadowPrice);
                mUpdateErcotConstraintCommand.Parameters.AddWithValue("@numberconstraints", ConstraintCount);
                mUpdateErcotConstraintCommand.Parameters.AddWithValue("@shiftfactor", element.ShiftFactor);
                mUpdateErcotConstraintCommand.Parameters.AddWithValue("@dollarimpact", element.Impact);
                mUpdateErcotConstraintCommand.Parameters.AddWithValue("@updatetime", DateTime.Now);
                mUpdateErcotConstraintCommand.Parameters.AddWithValue("@MonitoredText", element.Constraint);
                mUpdateErcotConstraintCommand.Parameters.AddWithValue("@ContingencyText", element.Contingency);
                if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
                {
                    VayuDBConnection.Open();
                }
                int Count = mUpdateErcotConstraintCommand.ExecuteNonQuery();
                if (Count == 0)
                {
                    mInsertErcotConstraintCommand = new SqlCommand();
                    mInsertErcotConstraintCommand.Connection = VayuDBConnection;
                    mInsertErcotConstraintCommand.CommandText = "insert Vayu..DAMasterConstraint values ( @MarketDateTime, " +
                        "                                       @MonitoredText, @ContingencyText, @ShadowPrice, @NumberConstraints, @ShiftFactor, " +
                                                                "0, 1, @DollarImpact, @UpdateTime,0)";
                    mInsertErcotConstraintCommand.Parameters.AddWithValue("@ShadowPrice", element.ShadowPrice);
                    mInsertErcotConstraintCommand.Parameters.AddWithValue("@NumberConstraints", ConstraintCount);
                    mInsertErcotConstraintCommand.Parameters.AddWithValue("@ShiftFactor", element.ShiftFactor);
                    mInsertErcotConstraintCommand.Parameters.AddWithValue("@MarketDateTime", element.MarketDateTimeInterval);
                    mInsertErcotConstraintCommand.Parameters.AddWithValue("@DollarImpact", element.Impact);
                    mInsertErcotConstraintCommand.Parameters.AddWithValue("@UpdateTime", DateTime.Now);
                    mInsertErcotConstraintCommand.Parameters.AddWithValue("@MonitoredText", element.Constraint);
                    mInsertErcotConstraintCommand.Parameters.AddWithValue("@ContingencyText", element.Contingency);
                    mInsertErcotConstraintCommand.ExecuteNonQuery();
                }
                #endregion InsertandUpdate
                VayuDBConnection.Close();
            }
            return 0;
        }
    }
}
