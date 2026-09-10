using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.MarketViewService
{
    class Program
    {
        static void Main(string[] args)
        {
            PriceServer price = new PriceServer();
            price.FillSourceSinkHash();
            price.Connect();
        }
    }
}
