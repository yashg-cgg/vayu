using System;

namespace Vayu.MarketViewNameSpace.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public static class LogWriter
    {
        /// <summary>
        /// The path
        /// </summary>
        static string path = "c:\\LMPConstraintsLog\\" + DateTime.Today.ToString("yyyyMMdd") + "_Log.log";
        /// <summary>
        /// The lock object
        /// </summary>
        static readonly object lockObj = new object();
        /*internal static void Log(string msg)
        {
            try
            {
                lock (lockObj)
                {
                    try
                    {
                        if (!File.Exists(path))
                        {
                            File.Create(path);
                        }
                    }
                    catch
                    {

                    }
                    TextWriter writer = new StreamWriter(path, true);
                    writer.Write(Environment.NewLine + DateTime.Now.ToString() + "\t" + msg);
                    writer.Flush();
                    writer.Close();
                }
            }
            catch
            {

            }
        }*/
    }
}
