using System;

namespace Vayu.EnergyPriceService
{
    class Program
    {
        static void Main(string[] args)
        {
            EnergyPriceServer objEnergyPrice = new EnergyPriceServer();
             objEnergyPrice.Connect();
        }
    }
}
