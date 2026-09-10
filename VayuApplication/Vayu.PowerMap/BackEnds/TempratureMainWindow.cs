
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Vayu.CityTemperatureServiceLibrary;

namespace Vayu.PowerMap.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    public partial class MainWindow
    {
        #region Declaration

        /// <summary>
        /// The temperature list
        /// </summary>
        private List<Temperature> temperatureList;
        /// <summary>
        /// The range temporary list
        /// </summary>
        private List<Temperature> rangeTempList;
        /// <summary>
        /// Gets or sets the temperature list.
        /// </summary>
        /// <value>
        /// The temperature list.
        /// </value>
        public List<Temperature> TemperatureList
        {
            get { return temperatureList; }
            set
            {
                temperatureList = value;
                RaisePropertyChanged("TemperatureList");
                if (value != null)
                {
                    sWeatherList = value.ToList();
                }
            }
        }


        /// <summary>
        /// The s weather list
        /// </summary>
        private static List<Temperature> sWeatherList = new List<Temperature>();
        /// <summary>
        /// The m temperature proxy
        /// </summary>
        private ITemperatureService mTemperatureProxy = null;
        /// <summary>
        /// The m temperature factory
        /// </summary>
        private DuplexChannelFactory<ITemperatureService> mTemperatureFactory = null;
        /// <summary>
        /// The m temporary hb timer
        /// </summary>
        private DispatcherTimer mTempHbTimer = new DispatcherTimer();


        //public GalaSoft.MvvmLight.Command.RelayCommand RunExportCSVCommand { private set; get; }


        #endregion

        #region Private Methods

        /// <summary>
        /// Hbs the caller.
        /// </summary>
        private void hbCaller()
        {
            if (mTemperatureFactory == null)
            {
                mTemperatureFactory = GetTemperatureProxy();
                mTemperatureProxy = mTemperatureFactory.CreateChannel();
            }
            mTemperatureProxy.HeartBeat();
        }

        /// <summary>
        /// Temperatures the pre load.
        /// </summary>
        private void TemperaturePreLoad()
        {
            //RunExportCSVCommand = new GalaSoft.MvvmLight.Command.RelayCommand(ExportToCSVCommand);
            string marketname = MarketlistBox.SelectedItem as string;
            List<Temperature> tempList = new List<Temperature>();
            mTemperatureFactory = GetTemperatureProxy();
            if (mTemperatureFactory != null)
            {
                mTemperatureProxy = mTemperatureFactory.CreateChannel();
                try
                {
                    if (MainCalendarSilder.IsRange.GetValueOrDefault())
                        tempList = mTemperatureProxy.GetCityTemperatures(MainCalendarSilder.from_date, MainCalendarSilder.to_date, true, marketname, IsCelsius, IsKmph, Isknots);
                    else
                        tempList = mTemperatureProxy.GetCityTemperatures(exactTime, exactDate.AddDays(1), false, marketname, IsCelsius, IsKmph, Isknots);

                    if (LiveCheckBox.IsChecked.GetValueOrDefault())
                    {
                        mTemperatureProxy.Subscribe(DateTime.Today);
                        if (mTempHbTimer != null && !mTempHbTimer.IsEnabled)
                        {
                            mTempHbTimer.Interval = new TimeSpan(0, 3, 0);
                            mTempHbTimer.IsEnabled = true;
                            mTempHbTimer.Tick += mTempHbTimer_Tick;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            if (tempList.Count > 0)
            {
                string market = MarketlistBox.SelectedItem as string;
                if (MainCalendarSilder.IsRange.GetValueOrDefault())
                {
                    RefreshRangeTemperatureLayer(tempList.Where(a => a.Market.ToUpper() == market.ToUpper()).ToList());
                    //RefreshRangeTemperatureLayer(tempList.Where(a => a.Market.ToUpper() == market.ToUpper()).Where(a => a.ClimateDate.Hour == MainCalendarSilder.HE).ToList());
                }
                else
                {
                    RefreshTemperatureLayer(tempList.Where(a => a.Market.ToUpper() == market.ToUpper()).Where(a => a.ClimateDate.Hour == MainCalendarSilder.HE).ToList());
                }

            }
            else
            {
                mWeatherMapLayer.Children.Clear();
                if (MapControl.myMap.Children.Contains(mWeatherMapLayer))
                    MapControl.myMap.Children.Remove(mWeatherMapLayer);
            }
        }
        /// <summary>
        /// Refreshes the temperature layer.
        /// </summary>
        /// <param name="tempList">The temporary list.</param>
        private void RefreshTemperatureLayer(List<Temperature> tempList)
        {
            if (tempList.Count > 0)
            {
                TemperatureList = tempList.ToList();
                mWeatherMapLayer.Children.Clear();

                foreach (Temperature item in TemperatureList)
                {

                    Ellipse myEllipse = new Ellipse
                    {
                        ToolTip = new ToolTip
                        {
                            Content = "City=" + item.City + "\n" + "Temperature=" + item.TempVal + "\n" + "Humidity=" + item.Humidity + "\n" + "Rain=" + item.Rain + "\n" + "Cloud=" + item.Clouds + "\n" + "Description=" + item.Description + "\n" + "Min Temp=" + item.MinTemp + "\n" + "Max Temp=" + item.MaxTemp + "\n" + "Wind Gust=" + item.WindGust + "\n" + "Wind Speed=" + item.WindSpeed,
                            FontWeight = FontWeights.Bold
                        },
                        Fill = new RadialGradientBrush { GradientOrigin = new Point(0, 0), GradientStops = GetGradientColor(item.TempVal) },
                        Stroke = Brushes.Transparent,
                        StrokeThickness = 0.1,
                        Height = 60,
                        Width = 60,
                        Opacity = 0.2
                    };
                    MapLayer.SetPosition(myEllipse, new Location { Latitude = item.Latitude, Longitude = item.Longitude });
                    ToolTipService.SetShowDuration(myEllipse, 300000);
                    Pushpin pin = new Pushpin
                    {
                        Content = item.TempVal + "'F" + item.City + "->" + item.TempVal,//item.City,//item.ClimateDate.ToString("yyyyMMdd HH:mm") + "->" + item.City + "->" + item.TempVal,
                        Location = new Location { Latitude = item.Latitude, Longitude = item.Longitude },
                        ToolTip = item.City + "->" + item.TempVal,
                    };
                    mWeatherMapLayer.Children.Add(pin);
                }
                if (MapControl.myMap.Children.Contains(mWeatherMapLayer))
                {
                    MapControl.myMap.Children.Remove(mWeatherMapLayer);
                }

                if (TemperatureCheckBox.IsChecked.GetValueOrDefault())
                    MapControl.myMap.Children.Insert(0, mWeatherMapLayer);
            }

            //GradientStopCollection coll = new GradientStopCollection();
            //coll.Add(new GradientStop { Color = Colors.Red, Offset = 0 });
            //coll.Add(new GradientStop { Color = Colors.White, Offset = 0.3 });

            //MapControl.myMap.Background = new RadialGradientBrush { GradientOrigin = new Point(0, 0), GradientStops = coll, Opacity = 0.6 };
        }

        /// <summary>
        /// Refreshes the range temperature layer.
        /// </summary>
        /// <param name="tempList">The temporary list.</param>
        private void RefreshRangeTemperatureLayer(List<Temperature> tempList)
        {
            if (tempList.Count > 0)
            {
                if (Temp_Avg_radioButton.IsChecked == true)
                {
                    RefreshAverageRangeTemperature(tempList);
                }
                else if (Temp_Max_radioButton.IsChecked == true)
                {
                    RefreshMaxRangeTemperature(tempList);
                }
                else if (Temp_Min_radioButton.IsChecked == true)
                {
                    RefreshMinRangeTemperature(tempList);
                }

                if (MapControl.myMap.Children.Contains(mWeatherMapLayer))
                {
                    MapControl.myMap.Children.Remove(mWeatherMapLayer);
                }
                if (TemperatureCheckBox.IsChecked.GetValueOrDefault())
                    MapControl.myMap.Children.Insert(0, mWeatherMapLayer);
            }
        }

        /// <summary>
        /// Refreshes the average range temperature.
        /// </summary>
        /// <param name="tempList">The temporary list.</param>
        private void RefreshAverageRangeTemperature(List<Temperature> tempList)
        {
            RangeTemperature();
            double longitude = 0;
            double lattitude = 0;
            mWeatherMapLayer.Children.Clear();
            List<Temperature> avgTemplist = new List<Temperature>();
            foreach (var item in rangeTempList.GroupBy(x => new { x.City, Date = x.ClimateDate.ToShortDateString() }))
            {
                string city = item.Key.City;
                var avgTemp = Math.Round(item.Average(x => x.TempVal));
                var avgMintemp = Math.Round(item.Average(x => x.MinTemp));
                var avgmaxxTemp = Math.Round(item.Average(x => x.MaxTemp));
                var avgwindgust = Math.Round(item.Average(x => x.WindGust));
                var avgwindspeed = Math.Round(item.Average(x => x.WindSpeed));
                if (item.Count() > 0)
                {
                    longitude = item.FirstOrDefault().Longitude;
                    lattitude = item.FirstOrDefault().Latitude;
                }
                Temperature Temp1 = new Temperature();
                foreach (var res in item)
                {
                    Temp1.City = city;
                    Temp1.Clouds = res.Clouds;
                    Temp1.Rain = res.Rain;
                    Temp1.Humidity = res.Humidity;
                    Temp1.TempVal = (int)avgTemp;
                    Temp1.Description = res.Description;
                    Temp1.MinTemp = (int)avgMintemp;
                    Temp1.MaxTemp = (int)avgmaxxTemp;
                    Temp1.WindGust = (int)avgwindgust;
                    Temp1.WindSpeed = (int)avgwindspeed;
                    Temp1.ClimateDate = Convert.ToDateTime(item.Key.Date);
                    Temp1.Latitude = res.Latitude;
                    Temp1.Longitude = res.Longitude;
                    avgTemplist.Add(Temp1);
                    break;
                }
                Ellipse myEllipse = new Ellipse
                {
                    ToolTip = new ToolTip
                    {
                        Content = "City=" + city + "\n" + "Avg.Temperature=" + avgTemp + "\n" + "Humidity=" + Temp1.Humidity + "\n" + "Rain=" + Temp1.Rain + "\n" + "Cloud=" + Temp1.Clouds + "\n" + "Description=" + Temp1.Description + "\n" + "Min Temp=" + avgMintemp + "\n" + "Max Temp=" + avgmaxxTemp,
                        FontWeight = FontWeights.Bold
                    },
                    Fill = new RadialGradientBrush { GradientOrigin = new Point(0, 0), GradientStops = GetGradientColor((int)avgTemp) },
                    Stroke = Brushes.Transparent,
                    StrokeThickness = 0.1,
                    Height = 60,
                    Width = 60,
                    Opacity = 0.2
                };
                MapLayer.SetPosition(myEllipse, new Location { Latitude = lattitude, Longitude = longitude });
                ToolTipService.SetShowDuration(myEllipse, 300000);
                mWeatherMapLayer.Children.Add(myEllipse);
            }
            TemperatureList = avgTemplist;
        }

        /// <summary>
        /// Refreshes the maximum range temperature.
        /// </summary>
        /// <param name="tempList">The temporary list.</param>
        private void RefreshMaxRangeTemperature(List<Temperature> tempList)
        {
            RangeTemperature();
            double longitude = 0;
            double lattitude = 0;
            mWeatherMapLayer.Children.Clear();
            List<Temperature> maxTemplist = new List<Temperature>();
            //var list = rangeTempList.GroupBy(x => new { x.City, Date = x.ClimateDate.ToShortDateString() }).ToList();
            foreach (var item in rangeTempList.GroupBy(x => new { x.City, Date = x.ClimateDate.ToShortDateString() }))
            {
                string city = item.Key.City;
                var maxTemp = item.Max(x => x.TempVal);
                // var dateTime = item.Max(x => x.DateAndTime);
                var MaxTempMintemp = item.Max(x => x.MinTemp);
                var MaxTempmaxxTemp = item.Max(x => x.MaxTemp);
                var MaxTempwindgust = item.Max(x => x.WindGust);
                var MaxTempwindspeed = item.Max(x => x.WindSpeed);
                if (item.Count() > 0)
                {
                    longitude = item.FirstOrDefault().Longitude;
                    lattitude = item.FirstOrDefault().Latitude;
                }
                Temperature Temp1 = new Temperature();
                foreach (var res in item.Where(c => c.TempVal == maxTemp))
                {
                    Temp1.City = res.City;
                    Temp1.Clouds = res.Clouds;
                    Temp1.Rain = res.Rain;
                    Temp1.Humidity = res.Humidity;
                    Temp1.TempVal = maxTemp;
                    Temp1.Description = res.Description;
                    Temp1.MinTemp = MaxTempMintemp;
                    Temp1.MaxTemp = MaxTempmaxxTemp;
                    Temp1.WindGust = MaxTempwindgust;
                    Temp1.WindSpeed = MaxTempwindspeed;
                    Temp1.ClimateDate = Convert.ToDateTime(item.Key.Date);
                    Temp1.Latitude = res.Latitude;
                    Temp1.Longitude = res.Longitude;
                    //Temp1.DateAndTime = dateTime;
                    maxTemplist.Add(Temp1);
                    break;
                }
                Ellipse myEllipse = new Ellipse
                {
                    ToolTip = new ToolTip
                    {
                        Content = "City=" + city + "\n" + "Max.Temperature=" + maxTemp + "\n" + "Humidity=" + Temp1.Humidity + "\n" + "Rain=" + Temp1.Rain + "\n" + "Cloud=" + Temp1.Clouds + "\n" + "Description=" + Temp1.Description + "\n" + "Min Temp=" + MaxTempMintemp + "\n" + "Max Temp=" + MaxTempmaxxTemp,
                        FontWeight = FontWeights.Bold
                    },
                    Fill = new RadialGradientBrush { GradientOrigin = new Point(0, 0), GradientStops = GetGradientColor((int)maxTemp) },
                    Stroke = Brushes.Transparent,
                    StrokeThickness = 0.1,
                    Height = 60,
                    Width = 60,
                    Opacity = 0.2
                };
                MapLayer.SetPosition(myEllipse, new Location { Latitude = lattitude, Longitude = longitude });
                ToolTipService.SetShowDuration(myEllipse, 300000);
                mWeatherMapLayer.Children.Add(myEllipse);
            }
            TemperatureList = maxTemplist;
        }

        /// <summary>
        /// Refreshes the minimum range temperature.
        /// </summary>
        /// <param name="tempList">The temporary list.</param>
        private void RefreshMinRangeTemperature(List<Temperature> tempList)
        {
            RangeTemperature();
            double longitude = 0;
            double lattitude = 0;
            mWeatherMapLayer.Children.Clear();
            List<Temperature> minTemplist = new List<Temperature>();
            foreach (var item in rangeTempList.GroupBy(x => new { x.City, Date = x.ClimateDate.ToShortDateString() }))
            {
                string city = item.Key.City;
                var minTemp = item.Min(x => x.TempVal);
                // var dateTime = item.Min(x => x.DateAndTime);
                var minTempMintemp = Math.Round(item.Min(x => x.MinTemp));
                var minTempmaxxTemp = Math.Round(item.Min(x => x.MaxTemp));
                var minTempwindgust = Math.Round(item.Min(x => x.WindGust));
                var minTempwindspeed = item.Min(x => x.WindSpeed);
                if (item.Count() > 0)
                {
                    longitude = item.FirstOrDefault().Longitude;
                    lattitude = item.FirstOrDefault().Latitude;
                }
                Temperature Temp1 = new Temperature();
                foreach (var res in item.Where(c => c.TempVal == minTemp))
                {
                    Temp1.City = res.City;
                    Temp1.Clouds = res.Clouds;
                    Temp1.Rain = res.Rain;
                    Temp1.Humidity = res.Humidity;
                    Temp1.TempVal = minTemp;
                    Temp1.Description = res.Description;
                    Temp1.MinTemp = minTempMintemp;
                    Temp1.MaxTemp = minTempmaxxTemp;
                    Temp1.WindGust = minTempwindgust;
                    Temp1.WindSpeed = minTempwindspeed;
                    Temp1.ClimateDate = Convert.ToDateTime(item.Key.Date);
                    Temp1.Latitude = res.Latitude;
                    Temp1.Longitude = res.Longitude;
                    // Temp1.DateAndTime = dateTime;
                    minTemplist.Add(Temp1);
                    break;
                }
                Ellipse myEllipse = new Ellipse
                {
                    ToolTip = new ToolTip
                    {
                        Content = "City=" + city + "\n" + "Min.Temperature=" + minTemp + "\n" + "Humidity=" + Temp1.Humidity + "\n" + "Rain=" + Temp1.Rain + "\n" + "Cloud=" + Temp1.Clouds + "\n" + "Description=" + Temp1.Description + "\n" + "Min Temp=" + minTempMintemp + "\n" + "Max Temp=" + minTempmaxxTemp,
                        FontWeight = FontWeights.Bold
                    },
                    Fill = new RadialGradientBrush { GradientOrigin = new Point(0, 0), GradientStops = GetGradientColor((int)minTemp) },
                    Stroke = Brushes.Transparent,
                    StrokeThickness = 0.1,
                    Height = 60,
                    Width = 60,
                    Opacity = 0.2
                };
                MapLayer.SetPosition(myEllipse, new Location { Latitude = lattitude, Longitude = longitude });
                ToolTipService.SetShowDuration(myEllipse, 300000);
                mWeatherMapLayer.Children.Add(myEllipse);
            }
            TemperatureList = minTemplist.ToList();
        }

        /// <summary>
        /// Gets the color of the gradient.
        /// </summary>
        /// <param name="temp">The temporary.</param>
        /// <returns></returns>
        private GradientStopCollection GetGradientColor(int temp)
        {
            double endGradient = .50;
            GradientStopCollection coll = new GradientStopCollection();
            coll.Add(new GradientStop
            {
                Color = Colors.Black,
                Offset = 0
            });

            coll.Add(new GradientStop { Color = GetTempColor(temp), Offset = endGradient });

            //if (temp > 97)
            //    coll.Add(new GradientStop { Color = Colors.Red, Offset = endGradient });
            //else if (temp > 90)
            //    coll.Add(new GradientStop { Color = Colors.Tomato, Offset = endGradient });
            //else if (temp > 86)
            //    coll.Add(new GradientStop { Color = Colors.LightSalmon, Offset = endGradient });
            //else if (temp > 83)
            //    coll.Add(new GradientStop { Color = Colors.Orange, Offset = endGradient });
            //else if (temp > 79)
            //    coll.Add(new GradientStop { Color = Colors.Chartreuse, Offset = endGradient });
            //else if (temp > 75)
            //    coll.Add(new GradientStop { Color = Colors.YellowGreen, Offset = endGradient });
            //else if (temp > 72)
            //    coll.Add(new GradientStop { Color = Colors.Aqua, Offset = endGradient });
            //else if (temp > 68)
            //    coll.Add(new GradientStop { Color = Colors.SkyBlue, Offset = endGradient });
            //else if (temp > 63)
            //    coll.Add(new GradientStop { Color = Colors.DodgerBlue, Offset = endGradient });
            //else if (temp > 58)
            //    coll.Add(new GradientStop { Color = Colors.LightSkyBlue, Offset = endGradient });
            //else if (temp > 53)
            //    coll.Add(new GradientStop { Color = Colors.DeepSkyBlue, Offset = endGradient });
            //else if (temp > 47)
            //    coll.Add(new GradientStop { Color = Colors.Purple, Offset = endGradient });
            //else if (temp > 43)
            //    coll.Add(new GradientStop { Color = Colors.Blue, Offset = endGradient });
            //else if (temp > 38)
            //    coll.Add(new GradientStop { Color = Colors.MediumBlue, Offset = endGradient });
            //else if (temp > 30)
            //    coll.Add(new GradientStop { Color = Colors.Navy, Offset = endGradient });
            //else
            //    coll.Add(new GradientStop { Color = Colors.MidnightBlue, Offset = endGradient });

            return coll;
        }

        /// <summary>
        /// Gets the temperature proxy.
        /// </summary>
        /// <returns></returns>
        private DuplexChannelFactory<ITemperatureService> GetTemperatureProxy()
        {
            NetTcpBinding binding = new NetTcpBinding
            {
                MaxReceivedMessageSize = int.MaxValue,
                Security = new NetTcpSecurity { Mode = SecurityMode.None },
                OpenTimeout = new TimeSpan(0, 25, 0),
                CloseTimeout = new TimeSpan(0, 25, 0),
                SendTimeout = new TimeSpan(0, 25, 0),
                ReceiveTimeout = new TimeSpan(0, 25, 0),

                // MaxConnections = int.MaxValue
            };

            DuplexChannelFactory<ITemperatureService> pipeFactory = new DuplexChannelFactory<ITemperatureService>
                (new InstanceContext(new TemperatureCaller()), binding, Vayu.CommonAccessLibrary.ServiceConnections.GetTemperatureService());

            foreach (var operationDescription in pipeFactory.Endpoint.Contract.Operations)
            {
                var dataContractBehavior = operationDescription.Behaviors[typeof(DataContractSerializerOperationBehavior)]
                                as DataContractSerializerOperationBehavior;
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                }
            }

            return pipeFactory;
        }

        /// <summary>
        /// Automatics the update weather.
        /// </summary>
        /// <param name="weatherList">The weather list.</param>
        private static void AutoUpdateWeather(List<Temperature> weatherList)
        {
            Dispatcher.CurrentDispatcher.Invoke(() => { });
        }

        /// <summary>
        /// Gets the color of the temporary.
        /// </summary>
        /// <param name="lmp">The LMP.</param>
        /// <returns></returns>
        private Color GetTempColor(double lmp)
        {
            double normalized_Temp = lmp / 11;
            var color = Dynamic_colorhash.Where(p => p.Key >= normalized_Temp);
            if (color.Count() > 0)
            {
                int colorkey = color.Min(d => d.Key);
                return Dynamic_colorhash[colorkey];
            }
            else
                return Dynamic_colorhash[11];
        }

        #endregion

        /// <summary>
        /// Ranges the temperature.
        /// </summary>
        public void RangeTemperature()
        {
            List<Temperature> tempList1 = new List<Temperature>();
            string market = MarketlistBox.SelectedItem as string;
            try
            {
                DateTime limitinitdate = DateTime.Today.AddDays(-10);
                DateTime limitlastdate = DateTime.Today.AddDays(10);
                if (MainCalendarSilder.from_date < limitinitdate)
                {
                    MessageBox.Show("'from date' should not be less than 10 days from current date");
                    MainCalendarSilder.from_date = limitinitdate;
                }
                if (MainCalendarSilder.to_date > limitlastdate)
                {
                    MessageBox.Show("'to date' should not be more than 10 days from current date");
                    MainCalendarSilder.to_date = limitlastdate;
                }
                if (MainCalendarSilder.IsRange.GetValueOrDefault())
                {
                    tempList1 = mTemperatureProxy.GetCityTemperatures(MainCalendarSilder.from_date, MainCalendarSilder.to_date, true, market, IsCelsius, IsKmph, Isknots);
                }


                if (LiveCheckBox.IsChecked.GetValueOrDefault())
                {
                    mTemperatureProxy.Subscribe(DateTime.Today);
                    if (mTempHbTimer != null && !mTempHbTimer.IsEnabled)
                    {
                        mTempHbTimer.Interval = new TimeSpan(0, 3, 0);
                        mTempHbTimer.IsEnabled = true;
                        mTempHbTimer.Tick += mTempHbTimer_Tick;
                    }
                }
            }
            catch { }
            if (tempList1.Count > 0)
            {
                if (MainCalendarSilder.IsRange.GetValueOrDefault())
                {
                    rangeTempList = (tempList1.Where(a => a.Market.ToUpper() == market.ToUpper()).ToList());
                }
            }
            else
            {
                mWeatherMapLayer.Children.Clear();
                if (MapControl.myMap.Children.Contains(mWeatherMapLayer))
                    MapControl.myMap.Children.Remove(mWeatherMapLayer);
            }
        }

        /// <summary>
        /// Handles the Tick event of the mTempHbTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void mTempHbTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (mTempHbTimer.IsEnabled)
                {
                    mTempHbTimer.IsEnabled = false;
                }
                hbCaller();
                mTempHbTimer.IsEnabled = true;
            }
            catch
            {

            }
        }
        public void ExportToCSVCommand()
        {
            Task.Factory.StartNew(() => { ExportToCSVThreaded(); });
        }

        private void ExportToCSVThreaded()
        {
            try
            {
                if (TemperatureList == null || TemperatureList.Count == 0)
                {
                    System.Windows.MessageBox.Show("No data to export to");
                    return;
                }
                SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
                dialog.FileName = "Portfolio_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
                if ((bool)dialog.ShowDialog())
                {
                    if (dialog.FileName != "")
                    {
                        if (TemperatureList != null && TemperatureList.Count > 0)
                        {
                            StringBuilder builder = new StringBuilder();
                            foreach (PropertyInfo item in TemperatureList.GetType().GetProperties())
                            {
                                if (item.Name.ToUpper() != "CITY" && item.Name.ToUpper() != "TEMPERATURE" && item.Name.ToUpper() != "HUMIDITY" && item.Name.ToUpper() != "RAIN" && item.Name.ToUpper() != "CLOUD" && item.Name.ToUpper() != "DESCRIPTION" && item.Name.ToUpper() != "WINDGUST" &&
                           item.Name.ToUpper() != "WINDSPEED" && item.Name.ToUpper() != "MINTEMPERATURE" && item.Name.ToUpper() != "MAXTEMPERATURE")
                                {
                                    continue;
                                }
                                builder.Append(item.Name.ToUpper() + ",");

                            }
                            builder.AppendLine();
                            foreach (Temperature item in TemperatureList)
                            {
                                foreach (PropertyInfo propItem in item.GetType().GetProperties())
                                {
                                    if (propItem.Name.ToUpper() == "CITY" || propItem.Name.ToUpper() == "TEMPERATURE" || propItem.Name.ToUpper() == "HUMIDITY" || propItem.Name.ToUpper() == "RAIN" || propItem.Name.ToUpper() == "CLOUD" || propItem.Name.ToUpper() == "DESCRIPTION" || propItem.Name.ToUpper() == "WINDGUST" ||
                                propItem.Name.ToUpper() == "WINDSPEED" || propItem.Name.ToUpper() == "MINTEMPERATURE" || propItem.Name.ToUpper() == "MAXTEMPERATURE")
                                    {
                                        builder.Append(propItem.GetValue(item) + ",");
                                    }
                                }
                                builder.AppendLine();
                            }
                            if (builder.Length > 0)
                            {
                                using (TextWriter str = new StreamWriter(dialog.FileName, false))
                                {
                                    str.Write(builder.ToString());
                                    str.Flush();
                                    str.Close();
                                    str.Dispose();
                                }
                                if (File.Exists(dialog.FileName))
                                {
                                    System.Windows.MessageBox.Show("Successfully saved the file " + dialog.FileName);
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("Couldnot save the file");
                                }
                            }
                        }
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="Vayu.ITemperatureCallback" />
        [ServiceBehavior(UseSynchronizationContext = false)]
        class TemperatureCaller : ITemperatureCallback
        {
            /// <summary>
            /// Sends the temperature.
            /// </summary>
            /// <param name="cityTemps">The city temps.</param>
            public void SendTemperature(List<Temperature> cityTemps)
            {
                if (cityTemps != null && cityTemps.Count > 0)
                {
                    if (cityTemps.Select(a => a.ClimateDate).Max() > sWeatherList.Select(a => a.ClimateDate).Max())
                    {
                        if (cityTemps.Select(a => a.TempVal).Max() != sWeatherList.Select(a => a.TempVal).Max())
                            AutoUpdateWeather(cityTemps);
                    }
                }
            }
        }
    }
}