using System.Collections.Generic;
using System.ServiceModel;

namespace Vayu.CityTemperatureServiceLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITemperatureCallback
    {
        /// <summary>
        /// Sends the temperature.
        /// </summary>
        /// <param name="cityTemps">The city temps.</param>
        [OperationContract(IsOneWay = true)]
        void SendTemperature(List<Temperature> cityTemps);
    }
}
