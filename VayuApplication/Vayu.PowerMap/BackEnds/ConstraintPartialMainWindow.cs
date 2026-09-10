using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vayu.PowerMap.Controls;
using Vayu.PowerMap.Model;

namespace Vayu.PowerMap.Views
{
    public partial class MainWindow
    {
        private List<Constraintobj> constraintRTList;
        private List<Constraintobj> constraintRTHList;
        private List<Constraintobj> constraintDAHList;

        private ObservableCollection<Constraintobj> myVar;
        public ObservableCollection<Constraintobj> GeoConstraintList
        {
            get { return myVar; }
            set { myVar = value; RaisePropertyChanged("GeoConstraintList"); }
        }



        private ObservableCollection<Constraintobj> nonmyVar;
        public ObservableCollection<Constraintobj> NonGeoConstraintList
        {
            get { return nonmyVar; }
            set { nonmyVar = value; RaisePropertyChanged("NonGeoConstraintList"); }
        }

        private ObservableCollection<Constraintobj> constraintDetailList;
        public ObservableCollection<Constraintobj> ConstraintDetailList
        {
            get { return constraintDetailList; }
            set { constraintDetailList = value; RaisePropertyChanged("ConstraintDetailList"); }
        }

        private ObservableCollection<string> constraintEquipmentTypeList;
        public ObservableCollection<string> ConstraintEquipmentTypeList
        {
            get { return constraintEquipmentTypeList; }
            set { constraintEquipmentTypeList = value; RaisePropertyChanged("ConstraintEquipmentTypeList"); }
        }

        private ConstraintPriceType mSelectedConstraintPriceType;
        public ConstraintPriceType SelectedConstraintPriceType
        {
            get
            {
                return mSelectedConstraintPriceType;
            }
            set
            {
                mSelectedConstraintPriceType = value;
                RaisePropertyChanged("SelectedConstraintPriceType");
                Constraint_Apply_button_Click(null, null);
            }
        }

        private void FetchConstraintData()
        {
            if (mMarketInContext == mMarkets["ERCOT"])
            {
                startTime = exactTime.AddHours(-3);
            }
            constraintRTList = MapConstraintHelper.GetConstraintObjRT(mMarketInContext, null, startTime.AddMinutes(5), endTime);
            constraintRTHList = MapConstraintHelper.GetConstraintObjRT(mMarketInContext, null, startTime.AddMinutes(5), endTime, true);

            if ((bool)MainCalendarSilder.IsRange)
                constraintDAHList = MapConstraintHelper.GetConstraintObjDA(mMarketInContext, null, startTime.Date, endTime.Date.AddHours(-1));
            else
                constraintDAHList = MapConstraintHelper.GetConstraintObjDA(mMarketInContext, null, exactDate.Date, exactDate.Date.AddDays(1).AddHours(-1));

            List<string> eqipments = constraintRTHList.Select(x => x.type).Distinct().ToList();
            HashSet<string> equipList = new HashSet<string>(eqipments);

            foreach (var item in constraintDAHList.Select(x => x.type))
                equipList.Add(item);

            ConstraintEquipmentTypeList = new ObservableCollection<string>(equipList.ToList());
            ConstraintEquipmentTypeListBox.SelectAll();
            ConstraintMain();
        }

        private void ConstraintMain([CallerMemberName] string memberName = "")
        {
            if (!EnableEvents && memberName != "FetchConstraintData")
                return;

            try
            {
                MapControl.myMap.Children.Remove(mConstraintMapLayer);
                mConstraintMapLayer.Children.Clear();

                using (new WaitCursor())
                {
                    if (SelectedConstraintPriceType == ConstraintPriceType.RT)
                    {
                        if (constraintRTList == null || constraintRTList.Count == 0)
                            return;

                        List<Constraintobj> filteredRTList = ConstraintFilter(constraintRTList);
                        List<Constraintobj> filteredRTHList = ConstraintFilter(constraintRTHList);
                        Constraint_SetGrid(filteredRTList, filteredRTHList);
                        Constraint_BuildMap(filteredRTHList);
                    }
                    if (SelectedConstraintPriceType == ConstraintPriceType.DA)
                    {
                        if (constraintDAHList == null || constraintDAHList.Count == 0)
                            return;

                        List<Constraintobj> filteredDAList = ConstraintFilter(constraintDAHList);
                        Constraint_SetGrid(filteredDAList, filteredDAList);
                        Constraint_BuildMap(filteredDAList);
                    }
                }
                if (ConstraintCheckBox.IsChecked != null && (bool)ConstraintCheckBox.IsChecked)
                {
                    MapControl.myMap.Children.Add(mConstraintMapLayer);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Constraint Exception" + e.Message);
            }
        }

        private List<Constraintobj> ConstraintFilter(List<Constraintobj> mytable)
        {
            Int16 constraint_max_kv;
            Int16 constraint_min_kv;
            if (!Int16.TryParse(Constraint_MaxPrice_textBox.Text, out constraint_max_kv))
            {
                constraint_max_kv = Int16.MaxValue;
            }
            if (!Int16.TryParse(Constraint_MinPrice_textBox.Text, out constraint_min_kv))
            {
                constraint_min_kv = Int16.MinValue;
            }

            if (!((mMarketInContext == mMarkets["MISO"]) || (mMarketInContext == mMarkets["SPP"]) || (mMarketInContext == mMarkets["ERCOT"])))
            {
                var stage1 = mytable.Where(x => x.constraint_KV.GetValueOrDefault() >= constraint_min_kv && x.constraint_KV.GetValueOrDefault() <= constraint_max_kv);
                var stage2 = stage1.Where(x => x.marketdatetime >= startTime && x.marketdatetime <= endTime);
                return stage2.ToList();
            }
            return mytable.ToList();
        }

        private void Constraint_BuildMap(List<Constraintobj> constrainthlist)
        {
            List<string> names = new List<string>();
            Dictionary<string, List<Constraintobj>> mConstraint_Collection = new Dictionary<string, List<Constraintobj>>();
            mConstraint_Collection.Clear();

            foreach (Constraintobj _constraint in constrainthlist)
            {
                string collectionkey = string.Format("{0}!{1}!{2}", _constraint.constraint, _constraint.constraint_stationFrom,
                              _constraint.constraint_stationTo);
                if (!mConstraint_Collection.ContainsKey(collectionkey))
                {
                    List<Constraintobj> constraint = new List<Constraintobj>();
                    constraint.Add(_constraint);
                    mConstraint_Collection.Add(collectionkey, constraint);
                }
                else
                    mConstraint_Collection[collectionkey].Add(_constraint);
            }

            List<Constraintobj> geoConstList = new List<Constraintobj>();
            List<Constraintobj> nonGeoConstList = new List<Constraintobj>();

            foreach (KeyValuePair<string, List<Constraintobj>> item in mConstraint_Collection)
            {
                Constraintobj _constraint = item.Value[0];
                string descripton = "Constraint: " + _constraint.constraint + "\n" +
                    "Contingency: " + _constraint.contingency + "\n" +
                    "Source: " + _constraint.constraint_stationFrom + "   " +
                    "Sink: " + _constraint.constraint_stationTo + "   " +
                    "Voltage: " + _constraint.constraint_KV + "\n";

                foreach (Constraintobj _con in item.Value)
                    descripton = string.Format("{0}\n{1}: {2}", descripton,
                        _con.marketdatetime.ToString("MM/dd/yy HH:mm:ss"), _con.shadowprice);

                if (_constraint.branch_location == null)
                    _constraint.branch_location = FindCoordinate(_constraint.constraint_stationFrom);

                if (_constraint.tobranch_location == null)
                    _constraint.tobranch_location = FindCoordinate(_constraint.constraint_stationTo);

                if (_constraint.type == "XF" || _constraint.type == "XFORMER" || string.IsNullOrEmpty(_constraint.constraint_stationTo))
                {
                    if (_constraint.branch_location != null)
                    {
                        Constraint_DrawXF(_constraint.branch_location, _constraint.constraint_stationFrom, descripton);
                        geoConstList.Add(_constraint);
                        names.Add(_constraint.constraint);
                    }
                    else
                        nonGeoConstList.Add(_constraint);
                }
                else
                {
                    if (_constraint.branch_location != null && _constraint.tobranch_location != null)
                    {
                        Constraint_DrawLine(_constraint.branch_location, _constraint.tobranch_location, _constraint.constraint_stationFrom,
                            _constraint.constraint_stationTo, descripton);
                        geoConstList.Add(_constraint);
                        names.Add(_constraint.constraint);
                    }
                    else
                        nonGeoConstList.Add(_constraint);
                }
            }

            GeoConstraintList = new ObservableCollection<Constraintobj>(geoConstList);
            NonGeoConstraintList = new ObservableCollection<Constraintobj>(nonGeoConstList);

            Constraint_autoCompleteBox.ItemsSource = names;
        }

        private void Constraint_DrawLine(Location start, Location finish, string source, string sink, string description)
        {
            Polygon soucePolygon = new Polygon();
            Polygon sinkPolygon = new Polygon();
            soucePolygon.Fill = new SolidColorBrush(Colors.DarkBlue);
            soucePolygon.Fill.Opacity = 0.5;
            sinkPolygon.Fill = new SolidColorBrush(Colors.DarkRed);
            sinkPolygon.Fill.Opacity = 0.5;
            PointCollection myPointCollection = new PointCollection();
            double unitlength = 5.5;
            myPointCollection.Add(new Point(0, unitlength));
            myPointCollection.Add(new Point(unitlength, 0));
            myPointCollection.Add(new Point(0, -unitlength));
            myPointCollection.Add(new Point(-unitlength, 0));
            //soucePolygon Points = myPointCollection;
            //sinkPolygon.Points = myPointCollection;

            ToolTip sourcett = new ToolTip();
            ToolTip sinktt = new ToolTip();
            sourcett.Content = source;
            sinktt.Content = sink;
            sourcett.FontWeight = FontWeights.Bold;
            sinktt.FontWeight = FontWeights.Bold;
            soucePolygon.ToolTip = sourcett;
            sinkPolygon.ToolTip = sinktt;

            MapLayer.SetPosition(soucePolygon, start);
            ToolTipService.SetShowDuration(soucePolygon, 300000);
            //ConstraintMapLayer.Children.Add(soucePolygon);

            MapLayer.SetPosition(sinkPolygon, finish);
            ToolTipService.SetShowDuration(sinkPolygon, 300000);
            //mConstraintMapLayer.Children.Add(sinkPolygon);

            MapPolygon polygon = new MapPolygon();
            polygon.Fill = new SolidColorBrush(Colors.DarkRed);
            polygon.Opacity = 0.5;
            polygon.Stroke = new SolidColorBrush(Colors.Black);
            polygon.StrokeThickness = 0.5;
            Point p1 = MapControl.myMap.LocationToViewportPoint(start);
            Point p2 = MapControl.myMap.LocationToViewportPoint(finish);
            double magnitude = 0.1;
            double xdistance = p1.X - p2.X;
            double ydistance = p1.Y - p2.Y;
            double slope = ydistance / xdistance;
            double inverslope = -1 / slope;
            Point midpoint = new Point();
            midpoint.X = p1.X - xdistance / 2;
            midpoint.Y = p1.Y - ydistance / 2;
            double distance = Math.Sqrt(Math.Pow(xdistance, 2) + Math.Pow(ydistance, 2));
            Point midpointdown = new Point(midpoint.X + magnitude * ydistance, midpoint.Y - magnitude * xdistance);
            Point midpointup = new Point(midpoint.X - magnitude * ydistance, midpoint.Y + magnitude * xdistance);
            LocationCollection coll2 = new LocationCollection();
            coll2.Add(start);
            coll2.Add(MapControl.myMap.ViewportPointToLocation(midpointdown));
            coll2.Add(finish);
            coll2.Add(MapControl.myMap.ViewportPointToLocation(midpointup));
            polygon.Locations = coll2;
            ToolTip tt = new ToolTip();
            tt.Content = description;
            tt.FontWeight = FontWeights.Bold;
            polygon.ToolTip = tt;
            ToolTipService.SetShowDuration(polygon, 300000);
            mConstraintMapLayer.Children.Add(polygon);
        }

        private void Constraint_DrawXF(Location start, string source, string description)
        {
            Ellipse myEllipse = new Ellipse();
            myEllipse.Fill = new SolidColorBrush(mConstraintColor);
            myEllipse.Fill.Opacity = 0.4;
            //myEllipse.Fill = new SolidColorBrush(Get_Color());
            myEllipse.Stroke = new SolidColorBrush(mConstraintColor);
            myEllipse.StrokeThickness = 0.5;
            double diameter = 30;
            double radius = diameter / 2;
            myEllipse.Width = diameter;
            myEllipse.Height = diameter;
            myEllipse.Margin = new Thickness(-radius, -radius, 0, 0);
            ToolTip tt = new ToolTip();
            tt.Content = description;
            tt.FontWeight = FontWeights.Bold;
            myEllipse.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(start);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(myEllipse, loc);
            ToolTipService.SetShowDuration(myEllipse, 300000);
            mConstraintMapLayer.Children.Add(myEllipse);
        }

        private void Outage_Grid_radioButton_Click(object sender, RoutedEventArgs e)
        {
            ConstraintMain();
        }

        private void Constraint_SetGrid(List<Constraintobj> constraintlist, List<Constraintobj> constrainthlist)
        {
            if ((bool)Outage_Grid_radioButton1.IsChecked && constraintlist != null)
                ConstraintDetailList = new ObservableCollection<Constraintobj>(constraintlist.OrderBy(x => x.marketdatetime));
            if ((bool)Outage_Grid_radioButton2.IsChecked && constrainthlist != null)
                ConstraintDetailList = new ObservableCollection<Constraintobj>(constrainthlist.OrderBy(x => x.marketdatetime));
        }
    }
}