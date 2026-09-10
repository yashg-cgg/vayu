using System;
using System.Collections.Generic;

namespace Vayu.RT_Constraint_impact.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Loads the database commands.
        /// </summary>
        void loadDBCommands();
        /// <summary>
        /// Gets the constraints.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="MarketDate">The market date.</param>
        /// <param name="IsHourly">if set to <c>true</c> [is hourly].</param>
        void GetConstraints(Action<List<ViewModels.Constraints>, Exception> callback, string selectedMarket, DateTime MarketDate, bool IsHourly);
    }
}
