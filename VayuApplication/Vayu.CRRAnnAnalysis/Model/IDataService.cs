using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.CRRAnnAnalysis.Model
{
    public interface IDataService
    {
        // DataTable GetSequenceYearData(int startYear, int endYear, int month, string selectedPeakType, string v1, object sourceKey, object sinkKey, string v2);

        DataTable GetSequenceYearData(
          int startYear,
          int endYear,
          int month,
          string timeUse,
          string hedge,
          int sourceKey,
          int sinkKey,
          string auctionPattern);

        DataTable GetMonthlyShadowPrices(
            int startYear,
            int endYear,
            string timeUse,
            string hedge,
            string sourceName,
            string sinkName);
         
    }
}
