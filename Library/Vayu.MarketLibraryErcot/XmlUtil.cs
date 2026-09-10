using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Xml.Serialization;
using System.IO.Compression;

namespace Vayu.MarketLibraryErcot
{
    public class XmlUtil
    {
        public static string Serialize(XmlSerializer serializer,
                               Encoding encoding,
                               XmlSerializerNamespaces ns,
                               bool omitDeclaration,
                               object objectToSerialize)
        {
            MemoryStream ms = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.OmitXmlDeclaration = omitDeclaration;
            settings.Encoding = encoding;
            //settings.Encoding = Encoding.ASCII;
            settings.NewLineHandling = NewLineHandling.None;
            XmlWriter writer = XmlWriter.Create(ms, settings);
            serializer.Serialize(writer, objectToSerialize, ns);
            return encoding.GetString(ms.ToArray()); ;
        }
        public static string EncodeToBase64(string myString)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(myString));
        }
        public static string DecodeFromBase64(string myString)
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(myString));
        }
        public static bool GetZipBase64(string str2Zip, out byte[] aryBase64Zip)
        {
            bool retVal = true;
            aryBase64Zip = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream gzs = new GZipStream(ms, CompressionMode.Compress))
                    {
                        using (StreamWriter sw = new StreamWriter(gzs))
                        {
                            sw.Write(EncodeToBase64(str2Zip));
                        }
                    }
                    aryBase64Zip = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                //throw ex;
            }
            return retVal;
        }
        public static bool GetUnZipUnBase64(byte[] zipData, out string strUnzipped)
        {
            bool retVal = true;
            strUnzipped = null;
            try
            {
                using (MemoryStream ms = new MemoryStream(zipData))
                {
                    using (GZipStream gzs = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        using (StreamReader sr = new StreamReader(gzs))
                        {
                            strUnzipped = DecodeFromBase64(sr.ReadToEnd());
                            //strUnzipped = Convert.FromBase64String(sr.ReadToEnd()).ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                //throw ex;
            }
            return retVal;
        }
        public static bool GetUnBase64UnZip(string base64Data, out string strUnzipped)
        {
            bool retVal = true;
            strUnzipped = null;
            string strUnBase64 = null;
            try
            {
                strUnBase64 = DecodeFromBase64(base64Data);
                byte[] btUnBase64 = Convert.FromBase64String(base64Data);
                //using (MemoryStream ms = new MemoryStream(new System.Text.UTF8Encoding().GetBytes(strUnBase64)))
                using (MemoryStream ms = new MemoryStream(btUnBase64))
                {
                    using (GZipStream gzs = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        using (StreamReader sr = new StreamReader(gzs))
                        {
                            strUnzipped = sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                //throw ex;
            }
            return retVal;
        }
    }
}
