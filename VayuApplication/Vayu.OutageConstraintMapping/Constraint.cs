namespace Vayu.OutageConstraintMapping
{
    public class Constraint
    {
        /// <summary>
        /// Gets or sets the name of the constraint.
        /// </summary>
        /// <value>
        /// The name of the constraint.
        /// </value>
        public string ConstraintName { get; set; }
        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var item = obj as Constraint;
            if (item == null)
            {
                return false;
            }
            return this.ConstraintName.Equals(item.ConstraintName);
        }
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.ConstraintName.GetHashCode();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class Contingency
    {
        /// <summary>
        /// Gets or sets the name of the contingency.
        /// </summary>
        /// <value>
        /// The name of the contingency.
        /// </value>
        public string ContingencyName { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class Family
    {
        /// <summary>
        /// Gets or sets the constraint family.
        /// </summary>
        /// <value>
        /// The constraint family.
        /// </value>
        public string ConstraintFamily { get; set; }
        /// <summary>
        /// Gets or sets the name of the constraint driver.
        /// </summary>
        /// <value>
        /// The name of the constraint driver.
        /// </value>
        public string ConstraintDriverName { get; set; }
        /// <summary>
        /// Gets or sets the constraint source.
        /// </summary>
        /// <value>
        /// The constraint source.
        /// </value>
        public string ConstraintSource { get; set; }
    }
}
