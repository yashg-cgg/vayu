using System;
using System.Collections.Generic;
using Vayu.ConstraintOutageMapping.ViewModels;

namespace Vayu.ConstraintOutageMapping.Model
{
    public interface IDataService
    {
        /// <summary>
        /// Gets the constraint list.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        List<Constraints> GetConstraintList(DateTime startDate, DateTime endDate);
        /// <summary>
        /// Gets the outage data.
        /// </summary>
        /// <param name="Selectedconstraint">The selectedconstraint.</param>
        /// <param name="SelectedContingency">The selected contingency.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="rangeChecked">if set to <c>true</c> [range checked].</param>
        /// <param name="IsStartChecked">if set to <c>true</c> [is start checked].</param>
        /// <returns></returns>
        List<ConstraintOutages> GetOutageData(string Selectedconstraint, string SelectedContingency, DateTime startDate, DateTime endDate, bool rangeChecked, bool IsStartChecked, bool isAllConstaintchecked);
        /// <summary>
        /// Gets the contingency list.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="constraintText">The constraint text.</param>
        /// <returns></returns>
        List<string> GetContingencyList(DateTime startDate, DateTime endDate, string constraintText);
        /// <summary>
        /// Gets all outage data.
        /// </summary>
        /// <param name="Selectedconstraint">The selectedconstraint.</param>
        /// <param name="SelectedContingency">The selected contingency.</param>
        /// <returns></returns>
        List<ConstraintOutages> GetAllOutageData(string Selectedconstraint, string SelectedContingency, bool isAllConstaintchecked, DateTime StartDate, DateTime EndDate);
    }
}
