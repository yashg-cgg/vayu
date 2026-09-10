using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.WeatherDownload
{
    class Program
    {
        /// <summary>
        /// Create WeatherDownload class object.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            //WeatherDownload weatherDownload = new WeatherDownload();   
            
            ErcotWeatherDownload obj = new ErcotWeatherDownload();
        }
    }
}
