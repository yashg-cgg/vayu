using System;
using System.Linq;

namespace Vayu.HourlyTemp_Grpah.Model
{
    public class TemperatureData
    {
        public int? Min { get; set; }
        public int? Max { get; set; }
        public int? Avg { get; set; }
        public string Zone { get; set; }
        public string City { get; set; }
        public DateTime Date { get; set; }
    }
    public class Temperature
    {
        public string Zone { get; set; }
        public string City { get; set; }
        public string MaxMinAvg { get; set; }
        public int? Day1 { get; set; }
        public int? Day2 { get; set; }
        public int? Day3 { get; set; }
        public int? Day4 { get; set; }
        public int? Day5 { get; set; }
        public int? Day6 { get; set; }
        public int? Day7 { get; set; }
        public int? Day8 { get; set; }
        public int? Day9 { get; set; }
        public int? Day10 { get; set; }
        public int? Day11 { get; set; }
        public int? Day12 { get; set; }
        public int? Day13 { get; set; }
        public int? Day14 { get; set; }
        public int? Day15 { get; set; }
        public int? Day16 { get; set; }
        public int? Day17 { get; set; }
        public int? Day18 { get; set; }
        public int? Day19 { get; set; }
        public int? Day20 { get; set; }
        public int? Day21 { get; set; }
        public int? Day22 { get; set; }
        public int? Day23 { get; set; }
        public int? Day24 { get; set; }
        public int? Day25 { get; set; }
        public int? Day26 { get; set; }
        public int? Day27 { get; set; }
        public int? Day28 { get; set; }
        public int? Day29 { get; set; }
        public int? Day30 { get; set; }
        public int? Day31 { get; set; }

    }

    public class HourlyTemperatureData
    {
        public DateTime Date { get; set; }

        public int? Max { get; set; }
        public double? Avg { get; set; }
        public int? Min { get; set; }
        public string Zone { get; set; }
        public string City { get; set; }

        public bool IsHE1CurrentTemperature { get; set; }
        public bool IsHE2CurrentTemperature { get; set; }
        public bool IsHE3CurrentTemperature { get; set; }
        public bool IsHE4CurrentTemperature { get; set; }
        public bool IsHE5CurrentTemperature { get; set; }
        public bool IsHE6CurrentTemperature { get; set; }
        public bool IsHE7CurrentTemperature { get; set; }
        public bool IsHE8CurrentTemperature { get; set; }
        public bool IsHE9CurrentTemperature { get; set; }
        public bool IsHE10CurrentTemperature { get; set; }
        public bool IsHE11CurrentTemperature { get; set; }
        public bool IsHE12CurrentTemperature { get; set; }
        public bool IsHE13CurrentTemperature { get; set; }
        public bool IsHE14CurrentTemperature { get; set; }
        public bool IsHE15CurrentTemperature { get; set; }
        public bool IsHE16CurrentTemperature { get; set; }
        public bool IsHE17CurrentTemperature { get; set; }
        public bool IsHE18CurrentTemperature { get; set; }
        public bool IsHE19CurrentTemperature { get; set; }
        public bool IsHE20CurrentTemperature { get; set; }
        public bool IsHE21CurrentTemperature { get; set; }
        public bool IsHE22CurrentTemperature { get; set; }
        public bool IsHE23CurrentTemperature { get; set; }
        public bool IsHE24CurrentTemperature { get; set; }
        public int? Hour1 { get; set; }
        public int? Hour2 { get; set; }
        public int? Hour3 { get; set; }
        public int? Hour4 { get; set; }
        public int? Hour5 { get; set; }
        public int? Hour6 { get; set; }
        public int? Hour7 { get; set; }
        public int? Hour8 { get; set; }
        public int? Hour9 { get; set; }
        public int? Hour10 { get; set; }
        public int? Hour11 { get; set; }
        public int? Hour12 { get; set; }
        public int? Hour13 { get; set; }
        public int? Hour14 { get; set; }
        public int? Hour15 { get; set; }
        public int? Hour16 { get; set; }
        public int? Hour17 { get; set; }
        public int? Hour18 { get; set; }
        public int? Hour19 { get; set; }
        public int? Hour20 { get; set; }
        public int? Hour21 { get; set; }
        public int? Hour22 { get; set; }
        public int? Hour23 { get; set; }
        public int? Hour24 { get; set; }
        public void CalculateMinMaxAvg()
        {
            int?[] hourlyTemperatures = new int?[]
            {
            Hour1, Hour2, Hour3, Hour4, Hour5, Hour6, Hour7, Hour8, Hour9, Hour10,
            Hour11, Hour12, Hour13, Hour14, Hour15, Hour16, Hour17, Hour18, Hour19, Hour20,
            Hour21, Hour22, Hour23, Hour24
            };

            // Calculate Min
            Min = hourlyTemperatures.Min();

            // Calculate Max
            Max = hourlyTemperatures.Max();

            // Calculate Avg
            Avg = hourlyTemperatures.Average();
        }
    }
}
