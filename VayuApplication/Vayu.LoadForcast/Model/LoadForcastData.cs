namespace Vayu.LoadForcast.Model
{
    public class LoadForecastData
    {
        public int LoadForecastKey { get; set; }
        public int MW { get; set; }
    }

    public class LoadForecastFinal
    {
        public string Zone { get; set; }
        public int CurrentDayMaxMW { get; set; }
        public int NextDayMaxMW { get; set; }
        public int LoadForecast { get; set; }
        public int DifferenceInMW { get; set; }
        public float Percentage { get; set; }
    }
    public class LoadForecastType
    {
        public int LoadForecastKey { get; set; }
        public string LoadForecastTypeName { get; set; }
    }
}
