using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows;
using System.Windows.Threading;
using Vayu.CityTemperatureServiceLibrary;
using Vayu.DBLibrary;
using Vayu.TemperatureHistoryDetails.Model;

namespace Vayu.TemperatureHistoryDetails.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region MainViewModel

        public MainWindowViewModel(IDataService dataservice)
        {

            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
            MarketList = new List<string>() { "ERCOT" };
            SelectedMarket = "ERCOT";
            cityList = FilterCities();
            IsRefreshEnabled = true;
            RunRetrieveFetchDataCommand = new DelegateCommand(() => RefreshButton());
        }
        #endregion

        #region property Declaration 
        public DelegateCommand RunRetrieveFetchDataCommand { private set; get; }

        private bool mIsRefreshEnabled;

        private List<string> mcityList;
        public List<string> cityList
        {
            get { return mcityList; }
            set
            {
                mcityList = value;
                RaisePropertyChanged("cityList");

            }
        }

        private string mSelectedFamily;
        public string SelectedFamily
        {
            get
            {
                return mSelectedFamily;
            }
            set
            {
                mSelectedFamily = value;
                RaisePropertyChanged("SelectedFamily");
            }
        }

        private List<string> mSelecetedItemCity;
        public List<string> SelecetedItemCity
        {
            get { return mSelecetedItemCity; }
            set
            {
                mSelecetedItemCity = value;
                RaisePropertyChanged("SelecetedItemCity");
            }
        }

        private List<string> mMarketList;
        public List<string> MarketList
        {
            get { return mMarketList; }
            set
            {
                mMarketList = value;
                RaisePropertyChanged("MarketList");

            }
        }

        private string mSelectedMarket;
        public string SelectedMarket
        {
            get { return mSelectedMarket; }
            set
            {
                mSelectedMarket = value;
                RaisePropertyChanged("SelectedMarket");

            }
        }

        private DateTime mStartDate;
        public DateTime StartDate
        {
            get { return mStartDate; }
            set
            {
                mStartDate = value;
                RaisePropertyChanged("StartDate");

            }
        }
        private DateTime mEndDate;
        public DateTime EndDate
        {
            get { return mEndDate; }
            set
            {
                mEndDate = value;
                RaisePropertyChanged("EndDate");

            }
        }

        private List<Temperature> temperatureList;
        public List<Temperature> TemperatureList
        {
            get { return temperatureList; }
            set
            {
                temperatureList = value;
                RaisePropertyChanged("TemperatureList");

            }
        }
        private static List<Temperature> sWeatherList = new List<Temperature>();
        private ITemperatureService mTemperatureProxy = null;
        private DuplexChannelFactory<ITemperatureService> mTemperatureFactory = null;

        public List<string> SelectedCityList { get; set; }
        public bool IsRefreshEnabled
        {
            get { return mIsRefreshEnabled; }
            set
            {
                mIsRefreshEnabled = value;
                RaisePropertyChanged("IsRefreshEnabled");

            }
        }

        #endregion

        #region  City ListBox Cities Filtertation
        private List<string> FilterCities()
        {
            List<string> allcities = DBAccess.GetCity(SelectedMarket);
            List<string> tempavailcities = new List<string>();
            List<Temperature> temp = new List<Temperature>();

            if (SelectedMarket.ToUpper() == "ERCOT")
            {
                temp = TemperaturePreLoad();
            }
            foreach (string city in allcities)
            {
                String cname = null;
                if (city.EndsWith(" TX"))
                    cname = city.Replace(" TX", "");
                if (temp.Exists(X => X.City == cname))
                {
                    tempavailcities.Add(city);
                }
            }
            return tempavailcities;
        }
        #endregion

        #region Refresh Button
        private void RefreshButton()
        {
            try
            {
                List<string> onlyCitylist = SelectedCityList;
                List<string> list = new List<string>();
                foreach (string item in onlyCitylist)
                {
                    if (item.EndsWith(" TX"))
                    {
                        string cname = item.Replace(" TX", "");
                        list.Add(cname);
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
                List<Temperature> temp = new List<Temperature>();
                List<Temperature> templist = new List<Temperature>();
                if (SelectedMarket.ToUpper() == "ERCOT")
                {
                    temp = TemperaturePreLoad();
                }
                foreach (string item in list)
                {
                    if (item != null)
                    {
                        if (SelectedMarket.ToUpper() == "ERCOT")
                        {
                            List<Temperature> temptlist = new List<Temperature>();
                            temptlist = temp.Where(X => X.City == item).ToList();
                            templist.AddRange(temptlist);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select City");
                    }
                }
                TemperatureList = templist;
            }
            catch
            {
                MessageBox.Show("Please select City");
            }
        }
        private List<Temperature> TemperaturePreLoad()
        {
            string marketname = "ERCOT";
            List<Temperature> tempList = new List<Temperature>();
            mTemperatureFactory = GetTemperatureProxy();
            if (mTemperatureFactory != null)
            {
                mTemperatureProxy = mTemperatureFactory.CreateChannel();
                try
                {
                    tempList = mTemperatureProxy.GetCityTemperatures(StartDate, EndDate, false, marketname);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return tempList;
        }
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
        private static void AutoUpdateWeather(List<Temperature> weatherList)
        {
            Dispatcher.CurrentDispatcher.Invoke(() => { });
        }
        #endregion

        #region Vayu.ITemperatureCallback 
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
        #endregion
    }
}
