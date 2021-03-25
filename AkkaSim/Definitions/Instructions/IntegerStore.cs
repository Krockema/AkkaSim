using AkkaSim.Interfaces;
using System;

namespace AkkaSim.Definitions.Instructions
{
    public class IntegerStore : ICurrentInstructions
    {
        private int _store;

        public bool Add(Guid key, ISimulationMessage message)
        {
            _store++;
            return true;
        }

        public int Count()
        {
            return _store;
        }

        public bool Remove(Guid msg)
        {
            _store--;
            return true;
        }

        public void WaitForDiastole(bool token)
        {
            _store += token ? +1 : -1;
        }
    }
}