using Akka.Actor;
using AkkaSim.Definitions;
using System;

namespace AkkaSim
{
    public class StateManager
    {
        /// <summary>
        /// Method do handle simulation State
        /// </summary>
        /// <param name="inbox"></param>
        /// <param name="sim"></param>
        public void Continuation(Inbox inbox, Simulation sim)
        {
            while (true)
            {
                var message = inbox.ReceiveAsync();
                switch (message.Result)
                {
                    case SimulationMessage.SimulationState.Started:
                        this.AfterSimulationStarted(sim);
                        continue;
                    case SimulationMessage.SimulationState.Stopped:
                        this.AfterSimulationStopped(sim);
                        sim.Continue();
                        continue;
                    case SimulationMessage.SimulationState.Finished:
                        this.SimulationIsTerminating(sim);
                        sim.ActorSystem.Terminate();
                        break;
                    default:
                        throw new Exception($"Unhandled simulation state received: {message}");
                }
                break;
            }
        }

        /// <summary>
        /// This is Called after the Simulation started. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void AfterSimulationStarted(Simulation sim)
        {
            System.Diagnostics.Debug.WriteLine("Received simulation start!", "(AKKA:SIM)");
        }

        /// <summary>
        /// This is Called after the Simulation started. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void AfterSimulationStopped(Simulation sim)
        {
            System.Diagnostics.Debug.WriteLine("Received simulation stop!", "(AKKA:SIM)");
        }

        /// <summary>
        /// This is Called after the Simulation started. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void SimulationIsTerminating(Simulation sim)
        {
            System.Diagnostics.Debug.WriteLine("Received simulation finished!", "(AKKA:SIM)");
        }
    }
}
