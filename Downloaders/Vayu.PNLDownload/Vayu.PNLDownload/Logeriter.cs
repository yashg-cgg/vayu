using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Vayu.PNLDownload
{
    static class Logeriter
    {
        /// <summary>
        /// The path
        /// </summary>
      // static string path = @"C:\VirtualPnlLogWriter\log"+DateTime.Now.ToString("yyMMddHH")+".txt";
       /// <summary>
       /// Writes the log.
       /// </summary>
       /// <param name="log">The log.</param>
        public static void writeLog(string log)
        {
            try
            {
                #region Commented Code
                /* if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                    TextWriter tw = new StreamWriter(path);
                    tw.WriteLine(log);
                    tw.Close();
                }
                else if (File.Exists(path))
                {
                   // File.Create(path).Dispose();
                    TextWriter tw = new StreamWriter(path,true);
                    tw.WriteLine(log);
                    tw.Close();
                }*/
                
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
