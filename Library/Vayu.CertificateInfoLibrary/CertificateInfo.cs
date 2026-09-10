using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.CertificateInfoLibrary
{
    public class CertificateInfo
    {
        public static CertificateHeler GetCertificateDetails(int marketKey, string applicationName)
        {
            CertificateHeler helper = new CertificateHeler();
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = "select * from Certificate where Application_Name = '" + applicationName + "' ";
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        helper.CertificateName = rdr.IsDBNull(1) ? "" : rdr.GetValue(1).ToString();
                        helper.UserName = rdr.IsDBNull(8) ? "" : rdr.GetValue(8).ToString();
                        helper.Password = rdr.IsDBNull(2) ? "" : rdr.GetValue(2).ToString();
                        helper.Path = rdr.IsDBNull(7) ? "" : rdr.GetValue(7).ToString();
                    }
                    con.Close();
                }
            }
            return helper;
        }


    }
    public class CertificateHeler
    {
        public string CertificateName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Path { get; set; }

        public int MarketKey { get; set; }

        public string ApplicationName { get; set; }
    }
}
