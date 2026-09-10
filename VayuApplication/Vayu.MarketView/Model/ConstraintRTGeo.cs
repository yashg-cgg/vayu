using Vayu.LatestConstraintsInformationLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.MarketView.Model
{
    //public class ConstraintRTGeo
    //{
    //    private int constraintRTGeoKey;
    //    private int marketKey;
    //    private int constraintBusNumFrom;
    //    private int constraintBusNumTo;
    //    private int constraintRTKey;
    //    private float contingency;
    //    private float kiloVolt;
    //    private string type;

    //    public string Type
    //    {
    //        get { return type; }
    //        set { type = value; }
    //    }

    //    public float KiloVolt
    //    {
    //        get { return kiloVolt; }
    //        set { kiloVolt = value; }
    //    }

    //    public float Contingency
    //    {
    //        get { return contingency; }
    //        set { contingency = value; }
    //    }

    //    public int ConstraintRTGeoKey
    //    {
    //        get { return constraintRTGeoKey; }
    //        set { constraintRTGeoKey = value; }
    //    }

    //    public int MarketKey
    //    {
    //        get { return marketKey; }
    //        set { marketKey = value; }
    //    }        

    //    public int ConstraintBusNumFrom
    //    {
    //        get { return constraintBusNumFrom; }
    //        set { constraintBusNumFrom = value; }
    //    }        

    //    public int ConstraintBusNumTo
    //    {
    //        get { return constraintBusNumTo; }
    //        set { constraintBusNumTo = value; }
    //    }        

    //    public int ConstraintRTKey
    //    {
    //        get { return constraintRTKey; }
    //        set { constraintRTKey = value; }
    //    }

    //    public void Read(SqlDataReader reader)
    //    {
    //        int.TryParse((reader[0] ?? "").ToString(), out constraintRTGeoKey);
    //        int.TryParse((reader[1] ?? "").ToString(), out marketKey);
    //        int.TryParse((reader[2] ?? "").ToString(), out constraintBusNumFrom);
    //        int.TryParse((reader[3] ?? "").ToString(), out constraintBusNumTo);
    //        int.TryParse((reader[4] ?? "").ToString(), out constraintRTKey);
    //        float.TryParse((reader[5] ?? "").ToString(), out kiloVolt);
    //        float.TryParse((reader[6] ?? "").ToString(), out contingency);
    //        type = (reader[6] ?? "").ToString();
    //    }
    //}

    //public class ConstraintRTGeoHash: Hashtable
    //{
    //    public int MarketKey { get; set; }

    //    public void UpdateConstraintHash(IEnumerable<LatestConstraint> constrains, int marketKey)
    //    {
    //        if (constrains == null || constrains.Count() == 0)
    //            return;
            
    //        SqlCommand mSelectConstrainsGeoCommand = new SqlCommand();
    //        mSelectConstrainsGeoCommand.Connection = SigmaDbConn;
    //        string missingKeys = string.Empty;

    //        foreach (var item in constrains)
    //        {
    //            if (this.ContainsKey(item.ConstraintKey))
    //                continue;

    //            missingKeys += item.ConstraintKey + " , ";
    //        }

    //        missingKeys = missingKeys.Trim(' ', ',');
    //        if (string.IsNullOrEmpty(missingKeys))
    //            return;

    //        mSelectConstrainsGeoCommand.CommandText = "select ConstraintRTGeoKey,MarketKey,ConstraintBusNumFrom,ConstraintBusNumTo,ConstraintRTKey, " +
    //            " ConstraintKV , ContingencyKV , Type " +
    //            " from ConstraintRTgeo(nolock) geo where geo.ConstraintRTKey in (" + missingKeys + ") and geo.MarketKey = " + marketKey;
    //        if (SigmaDbConn.State != System.Data.ConnectionState.Open)
    //            SigmaDbConn.Open();

    //        SqlDataReader reader = mSelectConstrainsGeoCommand.ExecuteReader();

    //        while (reader.Read())
    //        {
    //            ConstraintRTGeo geo = new ConstraintRTGeo();
    //            geo.Read(reader);
    //            this[geo.ConstraintRTKey] = geo;
    //        }

    //        reader.Close();

    //        if (SigmaDbConn.State != System.Data.ConnectionState.Closed)
    //            SigmaDbConn.Close();
    //    }
    //}
}
