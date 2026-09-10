using System;
using System.Collections.Generic;

namespace Vayu.OutageConstraintMapping.Model
{
    public interface IDataService
    {
        //void GetData(Action<DataItem, Exception> callback);
        /// <summary>
        /// Gets the outage data.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="rangeChecked">if set to <c>true</c> [range checked].</param>
        /// <param name="IsStartChecked">if set to <c>true</c> [is start checked].</param>
        /// <returns></returns>
        List<OutageData> GetOutageData(DateTime startDate, DateTime endDate, bool rangeChecked, bool IsStartChecked);
        /// <summary>
        /// Gets the constraint data.
        /// </summary>
        /// <param name="SelectedOutages">The selected outages.</param>
        /// <param name="shadowPrice">The shadow price.</param>
        /// <returns></returns>
        Dictionary<string, List<string>> GetConstraintData(List<string> SelectedOutages, int shadowPrice);
        /// <summary>
        /// Gets the contigency data.
        /// </summary>
        /// <param name="SelectedConstraint">The selected constraint.</param>
        /// <returns></returns>
        List<Contingency> GetContigencyData(string SelectedConstraint);
        /// <summary>
        /// Constraints the family list.
        /// </summary>
        /// <returns></returns>
        List<Tuple<String, String, String>> ConstraintFamilyList();
    }
}
