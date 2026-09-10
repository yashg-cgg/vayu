using System;
using System.Collections.Generic;

namespace Vayu.ErcotShiftFactor.Model
{
    public interface IDataService
    {
        /// <summary>
        /// Gets the constraint data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="isDa">if set to <c>true</c> [is da].</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="throDate">The thro date.</param>
        void GetConstraintData(Action<List<Constraint>, Exception> callback, int marketKey, bool isDa, DateTime fromDate, DateTime? throDate = null);
        /// <summary>
        /// Gets the detailed constraint data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="constraint">The constraint.</param>
        /// <param name="market">The market.</param>
        void GetDetailedConstraintData(Action<List<Constraint>, Exception> callback, string hour, Constraint constraint, int market, string marketName = null);
        List<NodePriceHelper> GetFiveMinsPRicesForUptos(DateTime fromDate, DateTime toDate, string v, Constraint selectedConstraint, int marketKey);

        List<NodePriceHelper> GetFiveMinsPRicesForFTR(DateTime fromDate, DateTime toDate, string v, Constraint selectedConstraint, int marketKey);

        List<NodePriceHelper> GetFiveMinsPRicesForALL(DateTime fromDate, DateTime toDate, string v, Constraint selectedConstraint, int marketKey);

        List<Constraint> GetEneryPrice(DateTime fromDate, DateTime toDate, bool isDA);
    }
}
