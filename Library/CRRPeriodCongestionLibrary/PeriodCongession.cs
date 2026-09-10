namespace Vayu.CRRPeriodCongestionLibrary
{
    public class PeriodCongession
    {
        public int NodeKey;

        public int PeriodKey;

        public double? PeakDALMP;

        public double? OffPeakDALMP;

        public double? PeakRTLMP;

        public double? OffPeakRTLMP;

        public double? PeakDACongestion;

        public double? OffPeakDACongestion;

        public double? PeakRTCongestion;

        public double? OffPeakRTCongestion;

        public double? PeakDALoss;

        public double? OffPeakDALoss;

        public double? PeakRTLoss;

        public double? OffPeakRTLoss;

        public int PeakDACount;

        public int OffPeakDACount;

        public int PeakRTCount;

        public int OffPeakRTCount;

        public double? PeakWEDALMP;
        public double? PeakWERTLMP;
        public int PeakWEDACount;
        public int PeakWERTCount;
    }

    public struct Key
    {

        public int NodeKey;

        public int PeriodKey;

        public string TypeCode;
    }
}
