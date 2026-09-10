using System;
using System.Collections.Generic;
using Vayu.DBLibrary;

namespace Vayu.ConstraintExposure.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        void loadDBCommands();

        /// <summary>
        /// Gets the constraints.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="UptosChecked">if set to <c>true</c> [uptos checked].</param>
        /// <param name="VirtualsChecked">if set to <c>true</c> [virtuals checked].</param>
        /// <param name="NodalChecked">if set to <c>true</c> [nodal checked].</param>
        /// <param name="PathChecked">if set to <c>true</c> [path checked].</param>
        /// <param name="IncChecked">if set to <c>true</c> [inc checked].</param>
        /// <param name="DecChecked">if set to <c>true</c> [decimal checked].</param>
        /// <param name="LookBackChecked">if set to <c>true</c> [look back checked].</param>
        /// <param name="SinkComboSelectedItem">The sink combo selected item.</param>
        /// <param name="SourceComboSelectedItem">The source combo selected item.</param>
        /// <param name="MWChecked">if set to <c>true</c> [mw checked].</param>
        /// <param name="DollarsChecked">if set to <c>true</c> [dollars checked].</param>
        /// <param name="DaysVal">The days value.</param>
        /// <param name="DASettleSelected">The da settle selected.</param>
        /// <param name="RTSettleSelected">The rt settle selected.</param>
        /// <param name="FamilyChecked">if set to <c>true</c> [family checked].</param>
        /// <param name="SelectedFamily">The selected family.</param>
        /// <param name="BestChecked">if set to <c>true</c> [best checked].</param>
        /// <param name="StartChecked">if set to <c>true</c> [start checked].</param>
        /// <param name="OutageChecked">if set to <c>true</c> [outage checked].</param>
        /// <param name="ConstraintChecked">if set to <c>true</c> [constraint checked].</param>
        /// <param name="ShadowPriceValue">The shadow price value.</param>
        /// <param name="ConstraintsList">The constraints list.</param>
        void GetConstraints(Action<List<Constraints>, Exception> callback, bool UptosChecked, bool VirtualsChecked,
            bool NodalChecked, bool PathChecked, bool IncChecked, bool DecChecked, bool LookBackChecked,
            PricingNode SinkComboSelectedItem, PricingNode SourceComboSelectedItem, bool MWChecked,
            bool DollarsChecked, string DaysVal, DateTime DASettleSelected, DateTime RTSettleSelected,
            bool FamilyChecked, string SelectedFamily, bool BestChecked, bool StartChecked, bool OutageChecked, bool ConstraintChecked, string ShadowPriceValue, List<Model.Constraints> ConstraintsList, int marketKey, string RTORDA);
    }
}
