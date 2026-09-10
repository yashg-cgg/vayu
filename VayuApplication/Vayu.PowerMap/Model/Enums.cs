namespace Vayu.PowerMap.Model
{
    /// <summary>
    /// 
    /// </summary>
    public enum LMPPriceType
    {
        /// <summary>
        /// The rt
        /// </summary>
        RT,
        /// <summary>
        /// The da
        /// </summary>
        DA,
        /// <summary>
        /// The dart
        /// </summary>
        DART,
        /// <summary>
        /// The r t5
        /// </summary>
        RT5,
        /// <summary>
        /// The r t5 maximum
        /// </summary>
        RT5Max,
        /// <summary>
        /// The r t5 minimum
        /// </summary>
        RT5Min,
    }
    /// <summary>
    /// 
    /// </summary>
    public enum ConstraintPriceType
    {
        /// <summary>
        /// The rt
        /// </summary>
        RT,
        /// <summary>
        /// The da
        /// </summary>
        DA,
        /// <summary>
        /// The r t5 minimum
        /// </summary>
        RT5Min
    }
    /// <summary>
    /// 
    /// </summary>
    public enum OutagesSourceType
    {
        /// <summary>
        /// The actual
        /// </summary>
        Actual,
        /// <summary>
        /// The planned
        /// </summary>
        Planned
    }
    /// <summary>
    /// 
    /// </summary>
    public enum OutagesStatusType
    {
        /// <summary>
        /// The planned
        /// </summary>
        Planned,
        /// <summary>
        /// The forced
        /// </summary>
        Forced
    }
    /// <summary>
    /// 
    /// </summary>
    public enum OutagesScheduleType
    {
        /// <summary>
        /// The active
        /// </summary>
        Active,
        /// <summary>
        /// The start date
        /// </summary>
        StartDate
    }
}
