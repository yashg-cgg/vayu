using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Collections.Concurrent;

namespace Vayu.BlockAlgorithmLibraryNamespace
{
    [ServiceContract]
    public interface IBlockAlgorithm
    {
        [OperationContract(IsOneWay=true)]
        void Calculate(int numPaths, SourceSink[] calculateNodeList, DateTime startDate, DateTime endDate,
                            ConcurrentDictionary<string, double> rtHash, ConcurrentDictionary<string, double> daHash, DateTime savedDate);
    }
}
