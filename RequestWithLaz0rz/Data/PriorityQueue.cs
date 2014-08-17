using System;
using System.Linq;
using System.Threading;

namespace RequestWithLaz0rz.Data
{
    /// <summary>
    /// Implementation of a priortiy queue
    /// </summary>
    public class PriorityQueue<TItem> where TItem : class, new() 
    {
        private readonly IComparable<TItem>[] _heap;
        private int _size;

        public const int DefaultCapacity = 20;

        /// <summary>
        /// Initializes the queue
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public PriorityQueue(int capacity = DefaultCapacity)
        {
            _heap = new IComparable<TItem>[capacity + 1];
        }

        /// <summary>
        /// Gets the number of inserted items in this queue
        /// </summary>
        public int Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Checks whether the queue is empty
        /// </summary>
        public bool IsEmpty
        {
            get { return Size == 0; }
        }

        /// <summary>
        /// Checks wheter the queue is not empty
        /// </summary>
        public bool IsNotEmpty
        {
            get { return Size > 0; }
        }

        /// <summary>
        /// Gets the item with the highest priority
        /// </summary>
        public IComparable<TItem> Max
        {
            get
            {
                return _heap[1];
            }
        }

        /// <summary>
        /// Adds a new item to the queue
        /// </summary>
        /// <param name="comparable">The item to add</param>
        public void Insert(IComparable<TItem> comparable)
        {
            var idx = Size + 1;
            _heap[idx] = comparable;
            Interlocked.Increment(ref _size);
            Swim(idx);
        }

        /// <summary>
        /// Gets and removes the item with the highest priority
        /// </summary>
        /// <returns>The item with the highest priority</returns>
        public IComparable<TItem> DelMax()
        {
            return null;
        }

        private void Sink(int i)
        {
            
        }

        private void Swim(int i)
        {
            while (i > 1 && IsLess(i, i / 2))
            {
                Swap(i, i / 2);
                i = i / 2;
            }           
        }

        private bool IsLess(int childIndex, int parentIndex)
        {
            //TODO if comparable func exists use this instead of the default one

            var parent = _heap[parentIndex];
            var child = _heap[childIndex];

            return parent.CompareTo(child as TItem) < 0;
        }

        private void Swap(int i, int j)
        {
            var swap = _heap[i];
            _heap[i] = _heap[j];
            _heap[j] = swap;
        }
    }
}
