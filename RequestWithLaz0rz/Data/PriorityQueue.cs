using System;

namespace RequestWithLaz0rz.Data
{
    /// <summary>
    /// Implementation of a priortiy queue
    /// </summary>
    public class PriorityQueue<TItem>
    {
        public const int DefaultCapacity = 20;

        /// <summary>
        /// Initializes the queue
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public PriorityQueue(int capacity = DefaultCapacity)
        {
            
        }

        /// <summary>
        /// Gets the number of inserted items in this queue
        /// </summary>
        public int Size
        {
            get { return -1; }
        }

        /// <summary>
        /// Checks whether the queue is empty
        /// </summary>
        public bool IsEmpty
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the item with the highest priority
        /// </summary>
        public IComparable<TItem> Max
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Adds a new item to the queue
        /// </summary>
        /// <param name="comparable">The item to add</param>
        public void Insert(IComparable<TItem> comparable)
        {
            
        }

        /// <summary>
        /// Gets and removes the item with the highest priority
        /// </summary>
        /// <returns>The item with the highest priority</returns>
        public IComparable<TItem> DelMax()
        {
            return null;
        }

    }
}
