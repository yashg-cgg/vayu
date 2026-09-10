using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vayu.LatestConstraintsInformationLibrary;

namespace Vayu.MarketView.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{LatestConstraintsInformationLibrary.LatestConstraint}" />
    public class ConstraintList : ObservableCollection<LatestConstraint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstraintList"/> class.
        /// </summary>
        public ConstraintList() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstraintList"/> class.
        /// </summary>
        /// <param name="list">The list.</param>
        public ConstraintList(IEnumerable<LatestConstraint> list) : base(list) { }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is all day.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is all day; otherwise, <c>false</c>.
        /// </value>
        public bool IsAllDay { get; set; }
    }
}
