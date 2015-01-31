using System;
using System.Linq;
using System.Threading;

namespace RequestWithLaz0rz.Data
{

    public interface IPriorityQueueItem
    {
        /// <summary>
        /// Gets the queue handle used to find
        /// the requests position inside the
        /// priority queue 
        /// </summary>
        int QueueHandle { get; set; }
    }

    /// <summary>
    /// Implementation of a priortiy queue
    /// </summary>
    public class PriorityQueue<TItem> where TItem : class, IComparable<TItem>, IPriorityQueueItem
    {
        private TItem[] _heap;
        private int _count;

        private const int InitialCapacity = 10;

        /// <summary>
        /// Queue handle for each item which is already deleted from queue
        /// </summary>
        public const int DeletionQueueHandle = 0;

        /// <summary>
        /// Initializes the queue
        /// </summary>
        public PriorityQueue()
        {
            _heap = new TItem[InitialCapacity + 1];
            _count = 0;
        }

        /// <summary>
        /// Gets the number of inserted items in this queue
        /// </summary>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Checks whether the queue is empty
        /// </summary>
        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        /// <summary>
        /// Checks wheter the queue is not empty
        /// </summary>
        public bool IsNotEmpty
        {
            get { return Count > 0; }
        }

        /// <summary>
        /// Gets the item with the highest priority
        /// </summary>
        public TItem Max
        {
            get
            {
                return _heap[1];
            }
        }

        /// <summary>
        /// Adds a new item to the queue
        /// </summary>
        /// <param name="item">The item to add</param>
        public void Insert(TItem item)
        {
            //increment size if max array capacity is reached
            if (Count + 1 == _heap.Length)
            {
                Array.Resize(ref _heap, _heap.Length + InitialCapacity);    
            }

            var idx = Count + 1;
            _heap[idx] = item;
            item.QueueHandle = idx;
            Interlocked.Increment(ref _count);
            Swim(idx);
        }

        /// <summary>
        /// Gets and removes the item with the highest priority
        /// </summary>
        /// <returns>The item with the highest priority</returns>
        public TItem DeleteMax()
        {
            var item = Max;
            Swap(1, Count);
            _heap[Count] = null;
            Interlocked.Decrement(ref _count);
            Sink(1);
            item.QueueHandle = DeletionQueueHandle;

            return item;
        }

        /// <summary>
        /// Deletes a specific item
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <returns>The deleted item or null if not found</returns>
        public TItem Delete(TItem item)
        {
            var index = item.QueueHandle;
            var result = _heap[index];

            if (result != null && result.Equals(item))
            {
                Swap(index, Count);
                _heap[Count] = null;
                Interlocked.Decrement(ref _count);
                result.QueueHandle = DeletionQueueHandle;
            }
            else
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Deletes all items from queue
        /// </summary>
        /// <returns>All deleted items</returns>
        public TItem[] DeleteAll()
        {
            var count = _heap.Count(item => item != null);
            var temp = new TItem[count];
            Array.Copy(_heap, 1, temp, 0, count);
            _heap = new TItem[InitialCapacity + 1];
            _count = 0;
            return temp;
        }

        private void Sink(int i)
        {
            //i = parent
            while (2 * i <= Count)
            {
                var childIdx = 2 * i;

                if (childIdx < Count && IsLess(childIdx, childIdx + 1))
                {
                    childIdx++;
                }

                if (!IsLess(i, childIdx))
                {
                    break;
                }

                Swap(i, childIdx);
                i = childIdx;
            }
        }

        private void Swim(int i)
        {
            while (i > 1 && IsLess(i / 2, i))
            {
                Swap(i, i / 2);
                i = i / 2;
            }           
        }

        private bool IsLess(int left, int right)
        {
            //TODO if comparable func exists use this instead of the default one

            var rightItem = _heap[right];
            var leftItem = _heap[left];

            return leftItem.CompareTo(rightItem) < 0;
        }

        private void Swap(int i, int j)
        {
            var swap = _heap[i];            
            _heap[i] = _heap[j];
            _heap[j] = swap;

            //update handles
            _heap[i].QueueHandle = i;
            _heap[j].QueueHandle = j;
        }

        /// <summary>
        /// Gets the request with a specific handle
        /// </summary>
        /// <param name="queueHandle">The handle of the request to get</param>
        /// <returns>The request or null if not found</returns>
        public TItem this[int queueHandle]
        {
            get { return _heap[queueHandle]; }
        }      
    }
}
