using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;

namespace Vayu.CRRCalculationLibrary
{
     
    [ServiceContract]
    public interface ISourceSinkCallback
    {
        [OperationContract(IsOneWay = true)]
        void GetSourceSink(List<SourceSink> SourceSinks);
    }
}
