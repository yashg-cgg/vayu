using Microsoft.Maps.MapControl.WPF;
using System;
using System.Windows;
using System.Windows.Media;

namespace Vayu.LMPStatistics.Model
{
    public class PointMapPath
    {
        public string Name { get; set; }

        private string toolTipText;

        public string ToolTipText
        {
            get { return toolTipText; }
            set
            {
                toolTipText = value;
            }
        }

        public Location MapLocation { get; set; }

        public Brush MyColor { get; set; }

        private string typeName;
        public string NodeTypeName { get; set; }
        public string TypeName
        {
            get { return typeName; }
            set
            {
                typeName = value;

                switch (typeName)
                {
                    case "GENERATOR":
                        UpdateStarPatternPoints();
                        break;

                    case "LOAD":
                        UpdateTrianglePatternPoints();
                        break;

                    case "EHV":
                        UpdateDiamodPoints();
                        break;


                    case "EXT":
                    case "BUS":

                    case "INTERFACE":

                    default:
                        break;
                }
            }
        }

        public PointCollection MyPointCollection { get; set; }

        private void UpdateDiamodPoints()
        {
            MyPointCollection = new PointCollection();
            double unitlength = 5.5;
            MyPointCollection.Add(new Point(0, unitlength));
            MyPointCollection.Add(new Point(unitlength, 0));
            MyPointCollection.Add(new Point(0, -unitlength));
            MyPointCollection.Add(new Point(-unitlength, 0));
        }

        private void UpdateTrianglePatternPoints()
        {
            MyPointCollection = new PointCollection();
            double radius = 6;
            for (int i = 120; i <= 360; i = i + 120)
                MyPointCollection.Add(new Point(radius * Math.Sin(i * Math.PI / 180), radius * Math.Cos(i * Math.PI / 180)));
        }

        private void UpdateStarPatternPoints()
        {
            MyPointCollection = new PointCollection();
            double outter_radius = 6;
            double inner_radius = 3;

            for (int i = 36; i <= 324; i = i + 72)
            {
                MyPointCollection.Add(new Point(outter_radius * Math.Sin(i * Math.PI / 180),
                                                outter_radius * Math.Cos(i * Math.PI / 180)));
                MyPointCollection.Add(new Point(inner_radius * Math.Sin((i + 36) * Math.PI / 180),
                                                inner_radius * Math.Cos((i + 36) * Math.PI / 180)));
            }
        }

        public PointMapPath()
        {
            MyColor = Brushes.GreenYellow;
            TypeName = "Default";
        }
    }
}
