using Akka.Actor;

namespace AkkaSim.Definitions
{
    public class SimulationConfig
    {
        /// <summary>
        /// Global Simulation Configuration 
        /// </summary>
        /// <param name="debug">Debug the Agent System</param>
        /// <param name="interruptInterval">At what TimeSpan shall the system stop and wait for further Commands
        /// the System will continue by calling the SimulationContext.Coninue() method.
        /// </param>
        public SimulationConfig(bool debug, long interruptInterval)
        {
            InteruptInterval = interruptInterval;
            Debug = debug;
        }
        
        public long InteruptInterval { get; }
        public bool Debug { get; }
        public Inbox Inbox { get; set; }
    }
}
