#define IsDa
//#define IsActiveConstraint
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Timers;
using System.Threading;
using System.IO;
using System.Net;
using System.Xml;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Data.OleDb;

namespace Vayu.ErcotConstraintsDownload
{
    class Program
    {

        static void Main(string[] args)
        {
                      // ActiveConstraint constraint = new ActiveConstraint();
            #if IsDa
                        bool isDa = true;
                        ErcotConstraintDownload erdownload = new ErcotConstraintDownload(false);

            #elif (IsActiveConstraint)

                                    ActiveConstraint constraint = new ActiveConstraint();
            #else
                        bool isDa = false;
                        ErcotConstraintDownload erdownload = new ErcotConstraintDownload(false);
            #endif

        }
    }
}
