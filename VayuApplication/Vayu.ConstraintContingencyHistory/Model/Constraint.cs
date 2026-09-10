using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vayu.LatestConstraintsInformationLibrary;

namespace Vayu.ConstraintContingencyHistory.Model
{
    public class Constraint
    {

        public string ConstraintText { get; set; }

        public string ContingencyText { get; set; }

        public string MonitoredFacility { get; set; }

        public DateTime ConstraintDate { get; set; }

        public double? HE1 { get; set; }

        public double? HE2 { get; set; }

        public double? HE3 { get; set; }

        public double? HE4 { get; set; }

        public double? HE5 { get; set; }

        public double? HE6 { get; set; }

        public double? HE7 { get; set; }

        public double? HE8 { get; set; }

        public double? HE9 { get; set; }

        public double? HE10 { get; set; }

        public double? HE11 { get; set; }

        public double? HE12 { get; set; }

        public double? HE13 { get; set; }

        public double? HE14 { get; set; }

        public double? HE15 { get; set; }

        public double? HE16 { get; set; }

        public double? HE17 { get; set; }

        public double? HE18 { get; set; }

        public double? HE19 { get; set; }

        public double? HE20 { get; set; }

        public double? HE21 { get; set; }

        public double? HE22 { get; set; }

        public double? HE23 { get; set; }

        public double? HE24 { get; set; }

        public double? Price { get; set; }
        public double? Avg { get; set; }

        public bool Is15Min { get; set; }

        public double? MaxLoad { get; set; }

        public double? MaxShadowPrice { get; set; }

        public double? MaxRTShadowPrice { get; set; }

        public int? SourceNodekey { get; set; }

        public int? SinkNodekey { get; set; }

        public string SourceZone { get; set; }

        public string SinkZone { get; set; }


        public int WinterFrequency { get; set; }
        public int SpringFrequency { get; set; }
        public int SummerFrequency { get; set; }
        public int FallFrequency { get; set; }


        public static List<Constraint> Accept(List<LatestConstraint> constraintList)
        {
            List<Constraint> constList = new List<Constraint>();
            var constraintGroup = constraintList.GroupBy(x => KeyMaker.GetKey(x.ConstraintText, x.ContigencyText));
            foreach (var item in constraintGroup)
            {
                foreach (var dateItem in item.GroupBy(x => x.MarketDate.Date))
                {
                    Constraint cnt = new Constraint();
                    cnt.ConstraintText = item.Key.Constraint;
                    cnt.ContingencyText = item.Key.Contingency;
                    cnt.ConstraintDate = dateItem.Key;

                    foreach (var dItem in dateItem)
                    {
                        PropertyInfo info = typeof(Constraint).GetProperty("HE" + dItem.MarketDate.Hour + 1);
                        if (info != null)
                            info.SetValue(cnt, dItem.ShadowPrice);
                    }
                    constList.Add(cnt);
                }
            }
            return constList;
        }
    }
}
