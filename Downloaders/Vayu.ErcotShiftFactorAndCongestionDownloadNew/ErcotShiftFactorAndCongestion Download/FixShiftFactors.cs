using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Data.SqlTypes;
using System.Timers;
using System.Globalization;
using System.Threading;
using Vayu.CommonAccessLibrary;

namespace Vayu.ErcotShiftFactorAndCongestionDownload
{
    class FixShiftFactors
    {
        private SqlConnection SigmaDbConn;
        private SqlCommand mUpdateErcotConstraintCommand;
        private SqlCommand mInsertErcotConstraintCommand;
        private void Init()
        {
            SigmaDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
        }
       
    }
}
