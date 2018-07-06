using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaSim.Definitions
{
    /// <summary>
    /// Not used yet
    /// </summary>
    public abstract class PriorityMessage : IComparable<PriorityMessage>
    {
        public decimal Priority { get; set; }

        public PriorityMessage(decimal priority)
        {
            Priority = priority;
        }

        public int CompareTo(PriorityMessage other)
        {
            if (this.Priority < other.Priority) return -1;
            else if (this.Priority > other.Priority) return 1;
            else return 0;
        }
    }
}
