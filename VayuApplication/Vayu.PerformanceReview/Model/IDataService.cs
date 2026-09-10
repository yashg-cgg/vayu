using System;
using System.Collections.Generic;
using Vayu.PerformanceReview.ViewModels;

namespace Vayu.PerformanceReview.Model
{
    public interface IDataService
    {
        List<Performance> GetReconcilationListDaily(DateTime startDate, DateTime endDate, int marketkey);
        DateTime GetMaxDate(string product);

    }
}
