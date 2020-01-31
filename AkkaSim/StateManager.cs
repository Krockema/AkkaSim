using Akka.Actor;
using AkkaSim.Definitions;
using System;
using AkkaSim.Logging;
using NLog;

namespace AkkaSim
{
    public class StateManager
    {
        private Logger Logger = LogManager.GetLogger(TargetNames.LOG_AKKA);
        private bool isRunning = true;
        /// <summary>
        /// Method do handle simulation State
        /// </summary>
        /// <param name="inbox"></param>
        /// <param name="sim"></param>
        public void Continuation(Inbox inbox, Simulation sim)
        {
            while (isRunning)
            {
                var message = inbox.ReceiveAsync(timeout: TimeSpan.FromHours(value: 1)).Result;
                switch (message)
                {
                    case SimulationMessage.SimulationState.Started:
                        Logger.Log(LogLevel.Warn, "Sim Started !");
                        this.AfterSimulationStarted(sim);
                        Continuation(inbox: inbox, sim: sim);
                        break;
                    case SimulationMessage.SimulationState.Stopped:
                        Logger.Log(LogLevel.Warn, "Sim Stopped !");
                        this.AfterSimulationStopped(sim);
                        sim.Continue();
                        Continuation(inbox, sim);
                        break;
                    case SimulationMessage.SimulationState.Finished:
                        Logger.Log(LogLevel.Warn, "Sim Finished !");
                        this.SimulationIsTerminating(sim);
                        sim.ActorSystem.Terminate().Wait();
                        isRunning = false;
                        break;
                    default:
                        Logger.Log(LogLevel.Warn, "StateManager: Unhandled message -> " + message.GetType() + "recived!");
                        break;
                }
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
