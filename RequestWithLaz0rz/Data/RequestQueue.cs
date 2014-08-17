namespace RequestWithLaz0rz.Data
{
    /// <summary>
    /// Priority: RequestPriority
    /// Request: Request<T>
    /// </summary>
    public interface IRequest<T>
    {
        RequestPriority Priority { get; }   
    }

    public class RequestQueue
    {
        /// <summary>
        /// Checks whether the queue is empty
        /// </summary>
        public bool IsEmpty
        {
            get { return true; }
        }

        /// <summary>
        /// Inserts a new request into the queue
        /// </summary>
        /// <param name="request">The request to insert</param>
        public void Enqueue(IRequest request)
        {
            
        }

        /// <summary>
        /// Gets and removes the next request in the queue
        /// </summary>
        /// <returns>The request with the highest priority</returns>
        public IRequest Dequeue()
        {
            return null;
        }
    }
}