using System;
using C5;

namespace RequestWithLaz0rz
{
    public class RequestQueue
    {
        private static readonly Lazy<RequestQueue> Lazy = new Lazy<RequestQueue>(() => new RequestQueue());
        private IntervalHeap<IComparable> _queue; //TODO use IRequest?

        /// <summary>
        /// Gets the singleton instance of
        /// this queue.
        /// </summary>
        public static RequestQueue Instance
        {
            get { return Lazy.Value; }
        }

        /// <summary>
        /// Hidden constructor
        /// </summary>
        private RequestQueue() { }

        #region event handler

        public event Action Started;

        protected virtual void OnStarted()
        {
            var handler = Started;
            if (handler != null) handler();
        }

        public event Action Completed;

        protected virtual void OnCompleted()
        {
            var handler = Completed;
            if (handler != null) handler();
        }

        #endregion

        public void Enqueue(Action requestExecution) //TODO use task? or implement IRequest?
        {
            
        }

        public void Dequeue()
        {
            
        }
    }
}
