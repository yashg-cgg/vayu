using System.Collections.Generic;
using System.ServiceModel;

namespace Vayu.WindServiceLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public interface IWindDataCallback
    {
        /// <summary>
        /// Sets the latest wind data.
        /// </summary>
        /// <param name="WindDataList">The Vayu wind data list.</param>
        [OperationContract(IsOneWay = true)]
        void SetLatestWindData(List<LatestWindData> WindDataList);
    }
}
