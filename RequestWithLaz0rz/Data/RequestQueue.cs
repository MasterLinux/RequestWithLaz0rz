namespace RequestWithLaz0rz.Data
{
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
        public void Enqueue(IPriorityRequest request)
        {
            
        }

        /// <summary>
        /// Gets and removes the next request in the queue
        /// </summary>
        /// <returns>The request with the highest priority</returns>
        public IPriorityRequest Dequeue()
        {
            return null;
        }

    }
}