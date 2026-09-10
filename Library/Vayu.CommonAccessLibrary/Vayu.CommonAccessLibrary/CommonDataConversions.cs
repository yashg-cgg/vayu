namespace Vayu.CommonAccessLibrary
{
    public class CommonDataConversions
    {
        /// <summary>
        /// Generic Method For Conversion of other Numeric Data types to Int
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static int? GetInt(object data)
        {
            string dataString = (data ?? "").ToString();
            int i = 0;
            if (int.TryParse(dataString, out i))
                return i;

            return null;
        }

        /// <summary>
        /// Generic Method For Conversion of other Numeric Data types to Double
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static double? GetDouble(object data)
        {
            string dataString = (data ?? "").ToString();
            double i = 0;
            if (double.TryParse(dataString, out i))
                return i;

            return null;
        }
    }
}
