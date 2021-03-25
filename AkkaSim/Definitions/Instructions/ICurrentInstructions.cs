using System;
using AkkaSim.Interfaces;

namespace AkkaSim.Definitions.Instructions
{
    public interface ICurrentInstructions
    {
        int Count();
        bool Remove(Guid msg);
        bool Add(Guid key, ISimulationMessage message);
        void WaitForDiastole(bool token);
    }
}