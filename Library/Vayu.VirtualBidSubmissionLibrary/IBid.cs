using System;
using System.ServiceModel;

namespace Vayu.VirtualBidSubmissionLibrary
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IVirtualCallBack))]
    public interface IVirtual
    {
        /// <summary>
        /// Hearts the beat.
        /// </summary>
        /// <returns></returns>
        [OperationContract()]
        bool HeartBeat();
        /// <summary>
        /// Posts the specified prices.
        /// </summary>
        /// <param name="prices">The prices.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        [OperationContract()]
        string Post(VirtualBid[] prices, DateTime toDate, int portfolio, string user);
        /// <summary>
        /// Cancels the specified prices.
        /// </summary>
        /// <param name="prices">The prices.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="portfolio">The portfolio.</param>
        /// <returns></returns>
        [OperationContract()]
        string Cancel(VirtualBid[] prices, DateTime toDate, int portfolio);
        /// <summary>
        /// Creates the XML file.
        /// </summary>
        /// <param name="prices">The prices.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        [OperationContract]
        string CreateXmlFile(VirtualBid[] prices, DateTime toDate, string portfolio, string userName);
    }
}
