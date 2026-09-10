using System;
using System.Collections.Generic;

namespace Vayu.ConstraintSensitivityAlgorithm.Model
{
    public interface IDataService
    {
        /// <summary>
        /// Gets the constraint contingency.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        List<ConstraintContingency> GetConstraintContingency(DateTime date, int MarketId);
        /// <summary>
        /// Gets the vectors.
        /// </summary>
        /// <param name="constraintContingencyList">The constraint contingency list.</param>
        /// <param name="isUptos">if set to <c>true</c> [is uptos].</param>
        /// <returns></returns>
        List<Vector> GetVectors(List<ConstraintContingency> constraintContingencyList, bool isUptos, int MarketId);
        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <returns></returns>
        List<SourceSink> GetPath(int MarketId);
        /// <summary>
        /// Gets the invalid source sink.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, List<Vector>> GetInvalidSourceSink();
    }
}
