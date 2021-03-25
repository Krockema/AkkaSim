using System;
using System.Collections.Generic;
using AkkaSim.Interfaces;

namespace AkkaSim.Definitions.Instructions
{
    public class MessageStore : ICurrentInstructions
    {
        private readonly Dictionary<Guid, ISimulationMessage> _store = new();
        private int _wait = 0;

        public bool Add(Guid key, ISimulationMessage message)
        {
            return _store.TryAdd(key, message);
        }

        public int Count()
        {
            return _store.Count + _wait;
        }

        public bool Remove(Guid msg)
        {
            _store.Remove(msg);
            return true;
        }

        public void WaitForDiastole(bool token)
        {
            _wait = token ? 1 : 0;
        }
    }
}