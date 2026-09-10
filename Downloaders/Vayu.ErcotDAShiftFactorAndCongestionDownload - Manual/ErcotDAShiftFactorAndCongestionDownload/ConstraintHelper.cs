using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.ErcotDAShiftFactorAndCongestionDownload
{
    class ConstraintHelper
    {
        public string ConstraintName { get; set; }
        public string ContingencyName { get; set; }
        public double ShadowPrice { get; set; }
        public int ConstraintCount { get; set; }
        public double TotalSP { get; set; }
    }
}
