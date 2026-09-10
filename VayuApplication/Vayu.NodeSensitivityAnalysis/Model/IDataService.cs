using System;
using System.Collections.Generic;

namespace Vayu.NodeSensitivityAnalysis.Model
{
    public interface IDataService
    {
        ///</summary>
        /// <param name="callback">The callback.</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="isDa">if set to <c>true</c> [is da].</param>
        /// <param name="fromDate">From date.</param> // DateTime? throDate = null
        /// <param name="throDate">The thro date.</param>
        void GetConstraintData(Action<List<Constraint>, Exception> callback, int marketKey, bool isDa, DateTime fromDate, DateTime? throDate = null);

    }
}
