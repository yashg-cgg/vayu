using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Vayu.CityTemperatureServiceLibrary
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract(CallbackContract = typeof(ITemperatureCallback))]
    public interface ITemperatureService
    {
        /// <summary>
        /// Gets the city temperatures.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="isRange">if set to <c>true</c> [is range].</param>
        /// <returns></returns>
        [OperationContract]
        List<Temperature> GetCityTemperatures(DateTime fromDate, DateTime toDate, bool isRange = false, string market = "ERCOT", bool IsCelsius = false, bool IsKmph = false, bool Isknots = false);

        /// <summary>
        /// Subscribes the specified date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        [OperationContract]
        bool Subscribe(DateTime date);

        /// <summary>
        /// Hearts the beat.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool HeartBeat();

        /// <summary>
        /// Unsubscribes this instance.
        /// </summary>
        [OperationContract]
        void Unsubscribe();
    }
}
