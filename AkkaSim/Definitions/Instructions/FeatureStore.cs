using System.Collections.Generic;
using System.Linq;

namespace AkkaSim.Definitions.Instructions
{
    public class FeatureStore : IFeatureInstructions
    {

        private readonly Dictionary<long, ICurrentInstructions> _store = new ();
        public int Count()
        {
            return _store.Count;
        }

        public bool TryGetValue(long time, out ICurrentInstructions feature)
        {
            return _store.TryGetValue(time, out feature);

        }

        public void Add(long scheduleAt, ICurrentInstructions instructions)
        {
            _store.Add(scheduleAt, instructions);
        }

        public long Next()
        {
            return _store.Min(x => x.Key);
        }

        public void Remove(long timePeriod)
        {
            _store.Remove(timePeriod);
        }
    }
}