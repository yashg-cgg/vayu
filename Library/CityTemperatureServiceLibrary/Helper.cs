using System;

namespace Vayu.CityTemperatureServiceLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class Helper
    {
        /// <summary>
        /// Gets the int.
        /// </summary>
        /// <param name="intValue">The int value.</param>
        /// <returns></returns>
        public static int GetInt(object intValue)
        {
            int val = 0;
            int.TryParse((intValue ?? "").ToString(), out val);
            return val;
        }

        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <param name="intValue">The int value.</param>
        /// <returns></returns>
        public static DateTime GetDate(object intValue)
        {
            DateTime val;
            if (DateTime.TryParse((intValue ?? "").ToString(), out val))
                return val;
            else
                return DateTime.Now;
        }

        /// <summary>
        /// Gets the double.
        /// </summary>
        /// <param name="intValue">The int value.</param>
        /// <returns></returns>
        public static double GetDouble(object intValue)
        {
            double val;
            double.TryParse((intValue ?? "").ToString(), out val);
            return val;
        }

        /// <summary>
        /// Gets the null double.
        /// </summary>
        /// <param name="intValue">The int value.</param>
        /// <returns></returns>
        public static double? GetNullDouble(object intValue)
        {
            double val;

            if (!double.TryParse((intValue ?? "").ToString(), out val))
                return null;

            return val;
        }
    }
}
