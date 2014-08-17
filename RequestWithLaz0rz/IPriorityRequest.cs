using System;

namespace RequestWithLaz0rz
{
    /// <summary>
    /// Interface for requests with a priority
    /// </summary>
    public interface IPriorityRequest : IComparable<IPriorityRequest>
    {
        /// <summary>
        /// Gets the execution priority. A request with a higher
        /// priority will be executed before a request
        /// with a lower one.
        /// </summary>
        RequestPriority Priority { get; }
    }
}
