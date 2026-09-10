using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.AppConfigHelper
{
    /// <summary>
    /// App Config Helper
    /// </summary>
    public class ConfigHelper
    {
        /// <summary>
        /// Gets the date time.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static DateTime? GetDateTime(string key, string format = "yyyy-MM-dd")
        {
            try
            {
                string strValue = GetAppSettings().Get(key);

                DateTime scanDate;
                if (!DateTime.TryParseExact(strValue, format, null, System.Globalization.DateTimeStyles.None, out scanDate))
                    return null;

                return scanDate;
            }
            catch { return null; }
        }

        /// <summary>
        /// Gets the bool.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static bool? GetBool(string key)
        {
            try
            {
                string strValue = GetAppSettings().Get(key);

                bool bValue;
                if (!bool.TryParse(strValue, out bValue))
                    return null;

                return bValue;
            }
            catch { return null; }
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetString(string key)
        {
            try
            {
                string strValue = GetAppSettings().Get(key);
                return strValue;
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// Gets the application settings.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Please validate AppConfig</exception>
        public static NameValueCollection GetAppSettings()
        {
            try
            {
                ConfigurationManager.RefreshSection("appSettings");
                return ConfigurationManager.AppSettings;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Please validate AppConfig : " + ex.Message);
                throw new Exception("Please validate AppConfig", ex);
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.Exception">Un-able to write AppConfig</exception>
        public static void SetValue(string key, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                    return;

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                if (config.AppSettings.Settings.AllKeys.Contains(key))
                    config.AppSettings.Settings[key].Value = value;
                else
                    config.AppSettings.Settings.Add(key, value);

                config.Save();
            }
            catch (Exception ex)
            {

                Console.WriteLine("Un-able to write AppConfig : " + ex.Message);
                throw new Exception("Un-able to write AppConfig", ex);
            }
        }

        

        /// <summary>
        /// Gets the int.
        /// </summary>
        /// <param name="objData">The object data.</param>
        /// <returns></returns>
        public static int GetInt(object objData)
        {
            string dStr = (objData ?? "").ToString();
            int dValue = 0;
            int.TryParse(dStr, out dValue);
            return dValue;
        }
    }
}
