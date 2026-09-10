using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Vayu.LatestConstraintsInformationLibrary
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IConstraintInfoCallback))]
    public interface IConstraintInfoProvider
    {
        /// <summary>
        /// Gets the latest constraint.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="date">The date.</param>
        /// <param name="isAllDay">if set to <c>true</c> [is all day].</param>
        /// <returns></returns>
        [OperationContract]
        List<LatestConstraint> GetLatestConstraint(int marketKey, DateTime date, bool isAllDay);

        /// <summary>
        /// Subscribes the specified market key.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="dateTime">The date time.</param>
        [OperationContract]
        void Subscribe(int marketKey, DateTime dateTime);

        /// <summary>
        /// Hearts the beat.
        /// </summary>
        [OperationContract]
        void HeartBeat();

        /// <summary>
        /// Unsubscribes this instance.
        /// </summary>
        [OperationContract]
        void Unsubscribe();

        /// <summary>
        /// Gets the constraint rt.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="hourly">if set to <c>true</c> [hourly].</param>
        /// <returns></returns>
        [OperationContract]
        List<LatestConstraint> GetConstraintRT(int marketKey, DateTime start, DateTime end, bool hourly);

        /// <summary>
        /// Gets the constraint da.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        [OperationContract]
        List<LatestConstraint> GetConstraintDA(int marketKey, DateTime start, DateTime end);

        /// <summary>
        /// Gets the nsa active constraint.
        /// </summary>
        /// <param name="Marketkey">The marketkey.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="IsAllDay">if set to <c>true</c> [is all day].</param>
        /// <returns></returns>
        [OperationContract]
        List<LatestConstraint> GetNSAActiveConstraint(int Marketkey, DateTime startDate, bool IsAllDay);
    }
}
