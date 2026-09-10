using System.Timers;

namespace Vayu.WCFServerClientBase
{
    /// <summary>
    /// 
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Creates the proxy.
        /// </summary>
        /// <param name="myProxyIndex">Index of my proxy.</param>
        /// <returns></returns>
        bool CreateProxy(int myProxyIndex);
        /// <summary>
        /// Proxies the faulted.
        /// </summary>
        void Proxy_Faulted();
        /// <summary>
        /// Aborts the proxy.
        /// </summary>
        void AbortProxy();
        /// <summary>
        /// Sets the connected status.
        /// </summary>
        /// <param name="proxyIndex">Index of the proxy.</param>
        /// <param name="bConnected">if set to <c>true</c> [b connected].</param>
        void SetConnectedStatus(int proxyIndex, bool bConnected);
        /// <summary>
        /// Reconnects to proxy.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        void ReconnectToProxy(object sender, ElapsedEventArgs e);
    }
}
