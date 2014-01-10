using System;
using C5;

namespace RequestWithLaz0rz
{
    public interface IRequest : IComparable<IRequest>
    {
        /// <summary>
        /// Queue handle which is used to get a specific request
        /// from queue. This getter must not be used directly. 
        /// </summary>
        IPriorityQueueHandle<IRequest> QueueHandle { get; set; }

        /// <summary>
        /// Gets the execution priority. A request with a higher
        /// priority will be executed before a request
        /// with a lower one.
        /// </summary>
        RequestPriority Priority { get; }

        /// <summary>
        ///  Starts the request. This method must not be used directly.
        ///  </summary>
        void Run(Action onCompleted);
    }
}
