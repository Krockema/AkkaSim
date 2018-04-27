using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace SimTest.Domain
{
    public class MaterialRequest : IComparable<MaterialRequest>
    {
        private static int count = 1;
        public MaterialRequest(Material material, Dictionary<int, bool> childRequests,int parrent, long due, bool isHead)
        {
            Id = GetCounter();
            Material = material;
            Parrent = parrent;
            ChildRequests = childRequests;
            Due = due;
            IsHead = isHead;
        }

        static int GetCounter()
        {
            return count++;
        }
        public bool IsHead { get; }
        public int Id { get;  }
        public Material Material { get;  }
        public int Parrent { get; }
        public Dictionary<int, bool> ChildRequests { get; }
        public long Due { get; }

        public int CompareTo(MaterialRequest other)
        {
            if (this.Due < other.Due) return -1;
            else if (this.Due > other.Due) return 1;
            else return 0;
        }
    }
}
