using System.ServiceModel;

namespace Vayu.VirtualBidSubmissionLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public interface IVirtualCallBack
    {
        /// <summary>
        /// Sends the generic results.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The e.</param>
        [OperationContract(IsOneWay = true)]
        void SendGenericResults(string[] s, string[] e);
        /// <summary>
        /// Sens the genericd cancel results.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The e.</param>
        [OperationContract(IsOneWay = true)]
        void SenGenericdCancelResults(string[] s, string[] e);
    }
}
