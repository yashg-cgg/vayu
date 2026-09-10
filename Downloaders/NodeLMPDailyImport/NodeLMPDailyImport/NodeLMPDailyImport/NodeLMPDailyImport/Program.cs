using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLMPDailyImport
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime? mStartDate = null;
            DateTime? mEndDate = null;
            bool allMarket;
            try
            {
                NodeLMPDailyImport mNodeLMPDailyImport = new NodeLMPDailyImport();
                if (args != null && args.Length > 0 && args[0].ToLower() == "daily")
                {
                    mStartDate = DateTime.Today.AddDays(-5);
                    mEndDate = DateTime.Today;
                    allMarket = true;
                }
                else
                {
                    mStartDate =Sigma.AppConfigHelper.ConfigHelper.GetDateTime("Start");
                    mEndDate = Sigma.AppConfigHelper.ConfigHelper.GetDateTime("End");
                    string marketKey = Sigma.AppConfigHelper.ConfigHelper.GetString("MarketKey");

                    if(!mStartDate.HasValue)
                        mStartDate = DateTime.Today.AddDays(-5);

                    if (!mEndDate.HasValue)
                        mEndDate = DateTime.Today;

                    if (string.IsNullOrEmpty(marketKey))
                        allMarket = true;
                    else
                        allMarket = false;
                }

                if (allMarket)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int marketKey = 1;
                        if (i == 1)
                        {
                            marketKey = 2;
                        }
                        if (i == 2)
                        {
                            marketKey = 7;
                        }
                        if (i == 3)
                        {
                            marketKey = 12;
                        }
                        mNodeLMPDailyImport.Run(mStartDate.Value, mEndDate.Value, marketKey);
                    }
                }
                else
                {
                    string strMktKey = Sigma.AppConfigHelper.ConfigHelper.GetString("MarketKey");
                    string[] mkts = strMktKey.Trim().Split(',');
                    foreach (var item in mkts)
                    {
                        try
                        {
                            int marketKey = Convert.ToInt32(item);
                            Console.WriteLine("Running for Market Key : " + marketKey);
                            mNodeLMPDailyImport.Run(mStartDate.Value, mEndDate.Value, marketKey);
                        }
                        catch(Exception ex)
                        {
                            Console.Write(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                Console.ReadLine();
            }
        }
    }
}