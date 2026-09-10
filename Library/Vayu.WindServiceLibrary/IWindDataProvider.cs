using System;
using System.Collections.Generic;
using System.ServiceModel;


namespace Vayu.WindServiceLibrary
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IWindDataCallback))]
    public interface IWindDataProvider
    {
        /// <summary>
        /// Gets the latest wind data.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        [OperationContract]
        List<LatestWindData> GetLatestWindData(DateTime startDate, DateTime endDate, int Marketkey);

        /// <summary>
        /// Subscribes the specified start date.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        [OperationContract]
        void Subscribe(DateTime startDate, DateTime endDate);

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
