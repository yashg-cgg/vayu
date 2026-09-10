using System;
using System.Collections.Generic;

namespace Vayu.DAMImpact.Model
{
    public interface IDataService
    {

        (List<RTImpactModel>, int) GetRTImpactsource(DateTime StartDate);
        (List<RTImpactModel>, int) GetRTImpactSink(DateTime StartDate);
        (List<RTImpactModel>, int) GetRTImpactSourceFMA(DateTime StartDate);
        (List<RTImpactModel>, int) GetRTImpactSinkFMA(DateTime StartDate);
    }
}
