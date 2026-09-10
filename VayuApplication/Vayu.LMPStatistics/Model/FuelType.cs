namespace Vayu.LMPStatistics.Model
{
    public class FuelType
    {
        public string FuelTypes { get; set; }
        public string Name { get; set; }
        public string Zone { get; internal set; }
    }
    public class FuelTypeList
    {

        public string SourceType { get; set; }
        public string SinkType { get; set; }
    }
}
