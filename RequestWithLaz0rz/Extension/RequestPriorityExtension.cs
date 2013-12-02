namespace RequestWithLaz0rz.Extension
{
    static internal class RequestPriorityExtension
    {
        private const int HighPriorityValue = 10;
        private const int MediumPriorityValue = 5;
        private const int LowPriorityValue = 0;

        /// <summary>
        /// Compares this priority with another one.
        /// </summary>
        /// <param name="prio">This priority</param>
        /// <param name="other">Another priority to compare with</param>
        /// <returns>Returns whether this priority is higher than the other one</returns>
        public static int Compare(this RequestPriority prio, RequestPriority other)
        {
            return GetPriorityValue(prio).CompareTo(GetPriorityValue(other));
        }

        /// <summary>
        /// Gets the value of this priority, used to
        /// compare two priorities.
        /// </summary>
        /// <param name="prio">This priority</param>
        /// <returns>The value of this priority</returns>
        private static int GetPriorityValue(this RequestPriority prio)
        {
            switch (prio)
            {
                case RequestPriority.High:
                    return HighPriorityValue;
                    
                case RequestPriority.Medium:
                    return MediumPriorityValue;

                case RequestPriority.Low:
                    return LowPriorityValue;
                
                default:
                    return MediumPriorityValue;
            }
        }
    }
}
