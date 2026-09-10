using System;
using System.Collections.Generic;

namespace Vayu.ERCOT_Market_Overview.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataService
    {
        List<DAVolumes> GetDAVolumes(DateTime date);
        List<DataItem> GetRTEnergy(DateTime date);
        List<DataItem> GetDAEnergy(DateTime date);
        List<DAVolumes> GetDAVolumesBetween(DateTime date, DateTime date1, string Stlpnt);
        List<BetweenDataItem> GetDAEnergyBetween(DateTime sdate, DateTime edate);
        List<BetweenDataItem> GetRTEnergyBetween(DateTime sdate, DateTime edate);
        List<string> GetValidPTPList(DateTime StartDate);

    }
}
