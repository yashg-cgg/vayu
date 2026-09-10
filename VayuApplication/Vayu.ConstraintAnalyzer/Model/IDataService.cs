using System;
using System.Collections.Generic;
using Vayu.DBLibrary;

namespace Vayu.ConstraintAnalyzer.Model
{
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
        /// <param name="RtRadioButton">if set to <c>true</c> [rt RadioButton].</param>
        /// <param name="DaRadioButton">if set to <c>true</c> [da RadioButton].</param>
        /// <param name="ImpactRadioButton">if set to <c>true</c> [impact RadioButton].</param>
        /// <param name="ShiftedRadioButton">if set to <c>true</c> [shifted RadioButton].</param>
        /// <param name="ShadowRadioButton">if set to <c>true</c> [shadow RadioButton].</param>
        /// <param name="DateRangeCheckBox">if set to <c>true</c> [date range CheckBox].</param>
        /// <param name="DatePicker1">The date picker1.</param>
        /// <param name="DatePicker2">The date picker2.</param>
        /// <param name="SelectedFamily">The selected family.</param>
        /// <param name="FamilyCheckBox">if set to <c>true</c> [family CheckBox].</param>
        void GetConstraints(Action<List<Constraints>, Exception> callback, bool RtRadioButton, bool DaRadioButton, bool ImpactRadioButton, bool ShiftedRadioButton,
            bool ShadowRadioButton, bool DateRangeCheckBox, DateTime DatePicker1, DateTime DatePicker2, string SelectedFamily, bool FamilyCheckBox, int Marketkey);
        /// <summary>
        /// Gets the exposure node minimum details.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="constraint">The constraint.</param>
        void GetExposureNodeMinDetails(Action<List<SourceSinkData>, Exception> callback, Model.Constraints constraint);
        /// <summary>
        /// Gets the exposure node maximum details.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="constraint">The constraint.</param>
        void GetExposureNodeMaxDetails(Action<List<SourceSinkData>, Exception> callback, Model.Constraints constraint);
        /// <summary>
        /// Gets the exposure up minimum details.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="constraint">The constraint.</param>
        void GetExposureUpMinDetails(Action<List<SourceSinkData>, Exception> callback, Model.Constraints constraint);
        /// <summary>
        /// Gets the exposure Uptos maximum details.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="constraint">The constraint.</param>
        void GetExposureUpMaxDetails(Action<List<SourceSinkData>, Exception> callback, Model.Constraints constraint);
    }
}
