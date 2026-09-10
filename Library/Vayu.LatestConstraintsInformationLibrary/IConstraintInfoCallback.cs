using System.Collections.Generic;
using System.ServiceModel;

namespace Vayu.LatestConstraintsInformationLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConstraintInfoCallback
    {
        /// <summary>
        /// Sets the constraints.
        /// </summary>
        /// <param name="constraintList">The constraint list.</param>
        [OperationContract(IsOneWay = true)]
        void SetConstraints(List<LatestConstraint> constraintList);
    }
}
