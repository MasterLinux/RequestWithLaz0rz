namespace RequestWithLaz0rz.Data
{
    public class RequestQueue
    {
        private PriorityQueue<PriorityRequest> _queue;
 
        /// <summary>
        /// Checks whether the queue is empty
        /// </summary>
        public bool IsEmpty
        {
            get { return true; }
        }

        /// <summary>
        /// Enqueues a new request into the queue
        /// </summary>
        /// <param name="request">The request to enqueue</param>
        public void Enqueue(PriorityRequest request)
        {
            
        }

        /// <summary>
        /// Gets and removes the next request in the queue
        /// </summary>
        /// <returns>The request with the highest priority</returns>
        public PriorityRequest Dequeue()
        {
            return null;
        }

    }
}