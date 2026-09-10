using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.NodeLMPDailyImport
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
                //NodeLMPDailyImport mNodeLMPDailyImport = new NodeLMPDailyImport();
               ErcotNodeLMPDailyImportOPT mercotNodeLMPDailyImport = new ErcotNodeLMPDailyImportOPT();
                 
                if (args != null && args.Length > 0 && args[0].ToLower() == "daily")
                {
                     mStartDate = DateTime.Today.AddDays(-3);// new DateTime(2020, 01, 01); 
                    //mStartDate = new DateTime(2026, 03, 28);
                    mEndDate = DateTime.Today;//new DateTime(2020, 01, 01); //
                   // mEndDate = new DateTime(2026, 03, 29); //
                    allMarket = true;  
                }
                else
                {
                    mStartDate = Vayu.AppConfigHelper.ConfigHelper.GetDateTime("Start");
                    mEndDate = Vayu.AppConfigHelper.ConfigHelper.GetDateTime("End");
                    string marketKey = Vayu.AppConfigHelper.ConfigHelper.GetString("MarketKey");

                    if (!mStartDate.HasValue)
                        //mStartDate = new DateTime(2021, 05, 04);
                        //mStartDate = new DateTime(2026, 03, 28);
                    mStartDate = DateTime.Today.AddDays(-3); ;// DateTime.Today.AddDays(-5);//new DateTime(2020, 05, 20); 
                        //YYYY,MM,DD
                       // mStartDate = new DateTime(2014, 01, 01); 
                       // mStartDate = new DateTime(2021, 02, 03);

                    if (!mEndDate.HasValue)
                        mEndDate = DateTime.Today;//new DateTime(2020, 06, 02); 
                        //mEndDate =  new DateTime(2026, 03, 29); 

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
                        //Console.WriteLine("Running for OBL" );
                        //mNodeLMPDailyImport.Run(mStartDate.Value, mEndDate.Value, marketKey);
                        Console.WriteLine("Running for Opt");
                        mercotNodeLMPDailyImport.Run(mStartDate.Value, mEndDate.Value, marketKey);
                    }
                }
                else
                {
                    string strMktKey = Vayu.AppConfigHelper.ConfigHelper.GetString("MarketKey");
                    string[] mkts = strMktKey.Trim().Split(',');
                    foreach (var item in mkts)
                    {
                        try
                        {
                            int marketKey = Convert.ToInt32(item);
                            Console.WriteLine("Running for Market Key : " + marketKey);
                            //Console.WriteLine("Running for OBL");
                            //mNodeLMPDailyImport.Run(mStartDate.Value, mEndDate.Value, marketKey);
                            Console.WriteLine("Running for Opt");
                            mercotNodeLMPDailyImport.Run(mStartDate.Value, mEndDate.Value, marketKey);
                         }
                        catch (Exception ex)
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