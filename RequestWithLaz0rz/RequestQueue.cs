using System;
using System.Collections.Generic;
using C5;

namespace RequestWithLaz0rz
{
    public class RequestQueue
    {
        private static readonly Lazy<RequestQueue> Lazy = new Lazy<RequestQueue>(() => new RequestQueue());
        private readonly IntervalHeap<Priority<IRequest>> _queue = new IntervalHeap<Priority<IRequest>>(new PriorityComparer<IRequest>());

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

        public void Enqueue(ref IPriorityQueueHandle<Priority<IRequest>> handle, IRequest request)
        {
            lock (_queue)
            {
                _queue.Add(ref handle, new Priority<IRequest>(request, 10)); //TODO add request.priority instead of 10
                Dequeue();
            }
        }

        public void Dequeue()
        {
            lock (_queue)
            {
                if (_queue.IsEmpty) return;
                var priority = _queue.DeleteMin();

                priority.Data.Run(); //TODO wait for events
            }
        }
    }

    internal struct PriorityComparer<T> : IComparer<Priority<T>>
    {
        public int Compare(Priority<T> x, Priority<T> y)
        {
            return x.CompareTo(y);
        }
    }

    public struct Priority<T> : IComparable<Priority<T>>
    {
        private readonly T _data;
        private readonly int _priority;

        public Priority(T data, int priority)
        {
            _data = data;
            _priority = priority;
        }

        public T Data
        {
            get
            {
                return _data;
            }
        }

        public int CompareTo(Priority<T> other)
        {
            return _priority.CompareTo(other._priority);
        }

        public bool Equals(Priority<T> other)
        {
            return _priority == other._priority;
        }

        /// <summary>
        /// Increments the priority
        /// </summary>
        /// <param name="priority">The priority to increment</param>
        /// <param name="delta">How much the priority should be incremented</param>
        /// <returns>Incremented priority</returns>
        public static Priority<T> operator +(Priority<T> priority, int delta)
        {
            return new Priority<T>(priority._data, priority._priority + delta);
        }

        /// <summary>
        /// Decrements the priority
        /// </summary>
        /// <param name="priority">The priority to decrement</param>
        /// <param name="delta">How much the priority should be decremented</param>
        /// <returns>Decremented priority</returns>
        public static Priority<T> operator -(Priority<T> priority, int delta)
        {
            return new Priority<T>(priority._data, priority._priority - delta);
        }
    }
}
