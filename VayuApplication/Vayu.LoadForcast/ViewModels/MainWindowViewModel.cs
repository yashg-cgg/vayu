using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Vayu.LoadForcast.Model;

namespace Vayu.LoadForcast.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private readonly IDataService _dataService;
        public DelegateCommand RunRefreshCommand { private set; get; }
        public DelegateCommand RunExportCSVCommand { private set; get; }
        private List<String> mISOMarketList;
        public List<String> ISOMarketList
        {
            get
            {
                return mISOMarketList;
            }
            set
            {
                mISOMarketList = value;
                RaisePropertyChanged("ISOMarketList");
            }
        }
        private string marketComboSelectedValue;
        public string MarketComboSelectedValue
        {
            get
            {
                return marketComboSelectedValue;
            }
            set
            {
                marketComboSelectedValue = value;
                RaisePropertyChanged("MarketComboSelectedValue");
            }
        }
        private DateTime mStartDate;
        public DateTime StartDate
        {
            get
            {
                return mStartDate;
            }
            set
            {
                mStartDate = value.Date;
                //SetMinMaxAvg();
                RaisePropertyChanged("StartDate");
            }
        }
        private DateTime mEndDate;
        public DateTime Enddate
        {
            get
            {
                return mEndDate;
            }
            set
            {
                mEndDate = value.Date;
                //SetMinMaxAvg();
                RaisePropertyChanged("Enddate");
            }
        }
        private IEnumerable<LoadForecastData> mCurrentLoadForecastDataList;
        public IEnumerable<LoadForecastData> CurrentLoadForecastDataList
        {
            get
            {
                return mCurrentLoadForecastDataList;
            }
            set
            {
                mCurrentLoadForecastDataList = value;
                RaisePropertyChanged("CurrentLoadForecastDataList");
            }
        }

        private IEnumerable<LoadForecastData> mPrevLoadForecastDataList;
        public IEnumerable<LoadForecastData> PrevLoadForecastDataList
        {
            get
            {
                return mPrevLoadForecastDataList;
            }
            set
            {
                mPrevLoadForecastDataList = value;
                RaisePropertyChanged("PrevLoadForecastDataList");
            }
        }
        private List<LoadForecastData> mLoadforecast7List;
        public List<LoadForecastData> Loadforecast7List
        {
            get
            {
                return mLoadforecast7List;
            }
            set
            {
                mLoadforecast7List = value;
                RaisePropertyChanged("Loadforecast7List");
            }
        }
        private List<LoadForecastFinal> mLoadforecastList;
        public List<LoadForecastFinal> LoadforecastList
        {
            get
            {
                return mLoadforecastList;
            }
            set
            {
                mLoadforecastList = value;
                RaisePropertyChanged("LoadforecastList");
            }
        }

        public MainWindowViewModel(IDataService dataService)
        {
            ISOMarketList = new List<string> { "ERCOT" };
            MarketComboSelectedValue = "ERCOT";
            StartDate = DateTime.Today;
            Enddate = DateTime.Today.AddDays(1);
            _dataService = dataService;
            RunRefreshCommand = new DelegateCommand(Refresh);
            RunExportCSVCommand = new DelegateCommand(ExportToCSVCommand);

        }
        private int GetMarketKey(string market)
        {

            if (market == "ERCOT")
            {
                return 9;
            }

            return 9;
        }
        private void Refresh()
        {
            int Marketkey = GetMarketKey(MarketComboSelectedValue.ToString());

            _dataService.GetAllForecastData((item1, error) =>
            {
                if (error != null)
                {
                    return;
                }
                CurrentLoadForecastDataList = item1;
            }, StartDate, Enddate, Marketkey);
            _dataService.GetAllForecastData((item1, error) =>
            {
                if (error != null)
                {
                    return;
                }
                PrevLoadForecastDataList = item1;
            }, StartDate.AddDays(1), Enddate.AddDays(1), Marketkey);
            _dataService.GetAllForecast7Data((item1, error) =>
            {
                if (error != null)
                {
                    return;
                }
                Loadforecast7List = item1;
            }, StartDate.AddDays(1), Enddate.AddDays(1), Marketkey);

            var list = from Current in CurrentLoadForecastDataList
                       join next in PrevLoadForecastDataList on Current.LoadForecastKey equals next.LoadForecastKey
                       select new
                       {
                           Current.LoadForecastKey,
                           Current.MW,
                           nextmw = next.MW,
                           diff = next.MW - Current.MW,
                           per = (float)((float)(next.MW - Current.MW) / (float)Current.MW) * 100
                       };
            list = list.OrderByDescending(x => x.per);
            List<LoadForecastType> LoadForecastTypeList = _dataService.GetAllForecastList(Marketkey);
            var finallist = from first in list
                            join name in LoadForecastTypeList on first.LoadForecastKey equals name.LoadForecastKey
                            select new
                            {
                                name.LoadForecastKey,
                                Zone = name.LoadForecastTypeName,
                                CurrentDayMaxMW = first.MW,
                                NextDayMaxMW = first.nextmw,
                                DifferenceInMW = first.diff,
                                Percentage = first.per
                            };
            finallist.OrderBy(x => x.Percentage);
            var flist = from f in finallist
                        join ff in Loadforecast7List on f.LoadForecastKey equals ff.LoadForecastKey
                        select new
                        {
                            LoadForecast = ff.MW,
                            Zone = f.Zone,
                            CurrentDayMaxMW = f.CurrentDayMaxMW,
                            NextDayMaxMW = f.NextDayMaxMW,
                            DifferenceInMW = f.DifferenceInMW,
                            Percentage = f.Percentage
                        };

            List<LoadForecastFinal> LoadForecastFinallist = new List<LoadForecastFinal>();
            foreach (var item in flist)
            {
                LoadForecastFinal mloadforecast = new LoadForecastFinal();
                mloadforecast.Zone = item.Zone;
                mloadforecast.CurrentDayMaxMW = item.CurrentDayMaxMW;
                mloadforecast.NextDayMaxMW = item.NextDayMaxMW;
                mloadforecast.DifferenceInMW = item.DifferenceInMW;
                mloadforecast.Percentage = item.Percentage;
                mloadforecast.LoadForecast = item.LoadForecast;
                LoadForecastFinallist.Add(mloadforecast);
            }

            LoadforecastList = LoadForecastFinallist;
        }
        public void ExportToCSVCommand()
        {
            Task.Factory.StartNew(() => { ExportToCSVThreaded(); });
        }
        private void ExportToCSVThreaded()
        {
            if (LoadforecastList == null)
            {
                Mouse.OverrideCursor = null;
                return;
            }
            if (LoadforecastList == null || LoadforecastList.Count == 0)
            {
                System.Windows.MessageBox.Show("No data to export to");
                return;
            }
            SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
            dialog.FileName = "LoadforecastPercentage" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
            if ((bool)dialog.ShowDialog())
            {
                if (dialog.FileName != "")
                {
                    if (LoadforecastList != null && LoadforecastList.Count > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (PropertyInfo item in LoadforecastList[0].GetType().GetProperties())
                        {
                            if (item.Name == "Zone" || item.Name == "CurrentDayMaxMW" || item.Name == "NextDayMaxMW" || item.Name == "LoadForecast" || item.Name == "DifferenceInMW" ||
                                item.Name == "Percentage")
                            {
                                builder.Append(item.Name + ",");
                            }

                        }
                        builder.AppendLine();
                        foreach (LoadForecastFinal item in LoadforecastList)
                        {
                            foreach (PropertyInfo propItem in item.GetType().GetProperties())
                            {
                                if (propItem.Name == "Zone" || propItem.Name == "CurrentDayMaxMW" || propItem.Name == "NextDayMaxMW" || propItem.Name == "LoadForecast" || propItem.Name == "DifferenceInMW" ||
                                    propItem.Name == "Percentage")
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
        }
    }

    public class DataGridBackColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush mybrush = new SolidColorBrush();
            double number;

            if (value != null)
            {
                if (value.ToString().Contains("($"))
                {
                    mybrush = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    double.TryParse(value.ToString(), out number);
                    if (number == 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                    else if (number > 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                    else if (number < 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                }

                if (value.ToString().Contains("SELL"))
                {
                    mybrush = new SolidColorBrush(Colors.Blue);
                }
                if (value.ToString().Contains("BUY"))
                {
                    mybrush = new SolidColorBrush(Colors.Black);
                }

            }
            return mybrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ValueToForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            double doubleValue = 0.0;
            double.TryParse(value.ToString(), out doubleValue);
            if (doubleValue < 0)
            {
                brush = new SolidColorBrush(Colors.Red);
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
