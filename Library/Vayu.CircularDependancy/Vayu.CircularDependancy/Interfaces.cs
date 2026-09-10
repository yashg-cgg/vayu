using System;
using System.Collections.Generic;

namespace Vayu.CircularDependancy
{
    public interface IVirtualBidEntry
    {
        /// <summary>
        /// Shows the specified market.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="tradeDate">The trade date.</param>
        void Show(string market, Vayu.DBLibrary.Portfolio portfolio, DateTime tradeDate);
        /// <summary>
        /// Shows the parent window.
        /// </summary>
        /// <param name="incPriceList">The inc price list.</param>
        /// <param name="incMWList">The inc mw list.</param>
        /// <param name="decPriceList">The decimal price list.</param>
        /// <param name="decMWList">The decimal mw list.</param>
        void ShowParentWindow(List<VirtualPrice> incPriceList, List<VirtualMW> incMWList, List<VirtualPrice> decPriceList, List<VirtualMW> decMWList);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IMainApp
    {
        /// <summary>
        /// Gets the virtual bid entry interface.
        /// </summary>
        /// <returns></returns>
        IVirtualBidEntry GetVirtualBidEntryInterface();
    }
}
