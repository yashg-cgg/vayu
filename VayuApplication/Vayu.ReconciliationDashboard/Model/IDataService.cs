using System;
using System.Collections.Generic;
using Vayu.ReconciliationDashboard.ViewModels;

namespace Vayu.ReconciliationDashboard.Model
{
    public interface IDataService
    {
        List<ReconcilationMTLY> GetReconcilationList(DateTime startDate, DateTime endDate, int marketkey);
        List<ReconcilationDLY> GetReconcilationListDaily(DateTime startDate, DateTime endDate, int marketkey);

        List<ReconcilationMTLY> GetMonthlyFTR(DateTime startDate, DateTime endDate, int marketkey);
        List<ReconcilationMTLY> GetMonthlyCRR(DateTime startDate, DateTime endDate, int marketkey);
        List<ReconcilationDLY> GetDailyFTR(DateTime startDate, DateTime endDate, int marketkey);
        DateTime GetMaxDate(string product);

    }
}
