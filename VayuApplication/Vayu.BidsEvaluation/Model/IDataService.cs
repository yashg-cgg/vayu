using System;
using System.Collections.Generic;
using Vayu.BidsEvaluation.ViewModels;

namespace Vayu.BidsEvaluation.Model
{
    public interface IDataService
    {
        List<BidsEvaluationDLY> GetBidsListDaily(DateTime startDate, DateTime endDate, int marketkey);
        List<BidsEvaluationDLY> GetBidsListDailyMonthly(DateTime startDate, DateTime endDate, int marketkey);
    }
}
