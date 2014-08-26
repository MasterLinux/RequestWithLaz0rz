using System;
using RequestWithLaz0rz.Extension;

namespace RequestWithLaz0rz
{
    /// <summary>
    /// Interface for requests with a priority
    /// </summary>
    public abstract class PriorityRequest : IComparable<PriorityRequest>
    {
        /// <summary>
        /// Gets the execution priority. A request with a higher
        /// priority will be executed before a request
        /// with a lower one.
        /// </summary>
        protected abstract RequestPriority Priority { get; }

        /// <summary>
        /// Executes the request asynchroniously
        /// </summary>
        /// <param name="onCompleted">Handler which is invoked when request is completed</param>
        internal abstract void RunAsync(Action onCompleted);

        /// <summary>
        /// Compares the priority of this request with the priority of another request.
        /// </summary>
        /// <param name="other">The other request to compare with</param>
        /// <returns>Returns whether the priority of this request is higher than the one of the other request</returns>
        public int CompareTo(PriorityRequest other)
        {
            return Priority.Compare(other.Priority);
        }
    }
}
