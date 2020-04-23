using System;
using Akka.Actor;
using Akka.Configuration;
using AkkaSim.Definitions;
using System.Threading.Tasks;
using Akka.Monitoring;
using Akka.Monitoring.ApplicationInsights;
using Akka.Monitoring.PerformanceCounters;
using static AkkaSim.Definitions.SimulationMessage;

namespace AkkaSim
{
    public class Simulation
    {
        public const string SimulationContextName = "SimulationContext";
        public ActorSystem ActorSystem { get; }
        public IActorRef SimulationContext { get; }
        /// <summary>
        /// Prepare Simulation Environment
        /// </summary>
        /// <param name="simConfig">Several Simulation Configurations</param>
        public Simulation(SimulationConfig simConfig)
        {
            Config config = (simConfig.DebugAkka) ? ConfigurationFactory.ParseString(GetConfiguration(NLog.LogLevel.Debug)) 
                                       /* else */ : ConfigurationFactory.ParseString(GetConfiguration(NLog.LogLevel.Info));

            ActorSystem  = ActorSystem.Create(SimulationContextName, config);
            
            if(simConfig.AddApplicationInsights)
            {
               var monitor = new ActorAppInsightsMonitor(SimulationContextName);
               var monitorExtension = ActorMonitoringExtension.RegisterMonitor(ActorSystem, monitor);
            }

            simConfig.Inbox = Inbox.Create(ActorSystem);
            SimulationContext = ActorSystem.ActorOf(Props.Create(() => new SimulationContext(simConfig)), SimulationContextName);
        }

        

        public bool IsReady()
        {
            var r = SimulationContext.Ask(Command.IsReady).Result;
            if (r is Command.IsReady)
            {
                return true;
            }
            return false;
        }

        public void Continue() {
            SimulationContext.Tell(Command.Start);
        }


        public Task RunAsync()
        {
            SimulationContext.Tell(Command.Start);
            return ActorSystem.WhenTerminated;
        }

        /// <summary>
        /// should later read from app.config or dynamically created
        /// </summary>
        /// <returns></returns>
        private string GetConfiguration(NLog.LogLevel level)
        {
            return @"akka {
                      stdout-loglevel = " + level.Name + @"
                      loglevel = " + level.Name + @"
                      loggers=[""Akka.Logger.NLog.NLogLogger, Akka.Logger.NLog""]
                      log-dead-letters-during-shutdown = off
                      log-config-on-start = on
                      actor {
                          debug {
                              receive = on
                              autoreceive = on
                              lifecycle = on
                              event-stream = on
                              unhandled = on
                              }
                          }";
        }
    }
}
