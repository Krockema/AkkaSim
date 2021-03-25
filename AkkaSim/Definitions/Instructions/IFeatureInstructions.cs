using System.Threading;

namespace AkkaSim.Definitions.Instructions
{
    public interface IFeatureInstructions
    {
        int Count();
        bool TryGetValue(long time, out ICurrentInstructions feature);
        void Add(long scheduleAt, ICurrentInstructions instructions);
        long Next();
        void Remove(long timePeriod);
    }
}