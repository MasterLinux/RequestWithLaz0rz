using System;
using System.Threading.Tasks;
using RequestWithLaz0rz.Type;

namespace RequestWithLaz0rz.Data
{
    /// <summary>
    /// Interface for queueable requests
    /// </summary>
    public interface IRequest<TResponse> : IPriorityQueueItem, IComparable<IRequest<TResponse>>
    {
        /// <summary>
        /// Gets the execution priority. A request with a higher
        /// priority will be executed before a request
        /// with a lower one.
        /// </summary>
        RequestPriority Priority { get; }

        /// <summary>
        /// Executes the request
        /// </summary>
        Task<Response<TResponse>> GetResponseAsync();

        /// <summary>
        /// Stops the execution of this request.
        /// </summary>
        /// <returns>This request</returns>
        Task AbortAsync();

        /// <summary>
        /// Flag which indicates whether the 
        /// request is currently executing.
        /// </summary>
        bool IsExecuting { get; }

        /// <summary>
        /// Gets whether the request is aborted
        /// </summary>
        bool IsAborted { get; }
    }
}
