using Akka.Actor;
using Akka.Configuration;
using AkkaSim.Definitions;
using System.Threading.Tasks;
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
        /// <param name="debug">Enables AKKA-Global message Debugging</param>
        public Simulation(SimulationConfig simConfig)
        {
            Config config = (simConfig.Debug) ? ConfigurationFactory.ParseString(GetConfiguration()) 
                         /* else */ : ConfigurationFactory.Load();

            ActorSystem  = ActorSystem.Create(SimulationContextName, config);
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
        /// should later read from app.config or dynamicly created
        /// </summary>
        /// <returns></returns>
        private string GetConfiguration()
        {
            return @"  akka {
                        stdout-loglevel = DEBUG
                        loglevel = DEBUG
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
