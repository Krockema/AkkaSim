using System;
using Akka.Actor;

namespace AkkaSim.Definitions
{
    public class SimulationConfig
    {
        /// <summary>
        /// Global Simulation Configuration 
        /// </summary>
        /// <param name="debugAkka">Debug the Akka Core System</param>
        /// <param name="debugAkkaSim">Set Akka Simulation in Debug Behaviour</param>
        /// <param name="interruptInterval">At what TimeSpan shall the system stop and wait for further Commands
        ///                                 the System will continue by calling the SimulationContext.Coninue() method.</param>
        /// <param name="timeToAdvance">minimum time to Advance to advance the simulation clock</param>
        public SimulationConfig(bool debugAkka, bool debugAkkaSim, long interruptInterval, TimeSpan timeToAdvance)
        {
            InterruptInterval = interruptInterval;
            DebugAkka = debugAkka;
            DebugAkkaSim = debugAkkaSim;
            TimeToAdvance = timeToAdvance;
        }
        public TimeSpan TimeToAdvance { get; }
        public long InterruptInterval { get; }
        public bool DebugAkka { get; }
        public bool DebugAkkaSim { get; }
        public Inbox Inbox { get; set; }
    }
}

