 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.SensitivityCalculation
{
    class Program
    {
        static void Main(string[] args)
        {
            //ErcotSensitivytyManual ercotManual = new ErcotSensitivytyManual();
            //ercotManual.RunManual(constraintId: 1584, constraint: "HAMILT_MAVERI1_1/HAMILTON-MAVERICK/138-138", contingency: "SBRAUVA8");


            HourlyImpact hourlyImpact = new HourlyImpact("ERCOT");
            hourlyImpact.StartTimer();
             
        }
    }
}
